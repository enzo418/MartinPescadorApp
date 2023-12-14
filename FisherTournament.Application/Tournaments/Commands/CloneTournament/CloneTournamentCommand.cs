using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Commands.CloneTournament
{
	public record struct CloneTournamentCommand(
		string SourceTournamentId,
		string NewTournamentName,
		DateTime NewTournamentStartDate)
		: IRequest<ErrorOr<CloneTournamentCommandResult>>;

	public record struct CloneTournamentCommandResult(
		string TournamentId,
		string Name,
		DateTime StartDate,
		DateTime? EndDate,
		List<Error>? Warnings);

	public class CloneTournamentCommandHandler
		: IRequestHandler<CloneTournamentCommand, ErrorOr<CloneTournamentCommandResult>>
	{
		private readonly IDateTimeProvider _dateTimeProvider;
		private readonly ITournamentFisherDbContext _context;
		private readonly ILeaderBoardRepository _leaderBoardRepository;

		public CloneTournamentCommandHandler(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider, ILeaderBoardRepository leaderBoardRepository)
		{
			_context = context;
			_dateTimeProvider = dateTimeProvider;
			_leaderBoardRepository = leaderBoardRepository;
		}

		public async Task<ErrorOr<CloneTournamentCommandResult>> Handle(CloneTournamentCommand request, CancellationToken cancellationToken)
		{
			var tournamentId = TournamentId.Create(request.SourceTournamentId);

			if (tournamentId.IsError)
			{
				return Errors.Id.NotValidWithProperty(nameof(request.SourceTournamentId));
			}

			var sourceTournament = await _context.Tournaments
				.Where(t => t.Id == tournamentId.Value)
				.FirstOrDefaultAsync(cancellationToken);

			if (sourceTournament == null)
			{
				return Errors.Tournaments.NotFound;
			}

			List<Error> errors = new();

			Tournament newTournament = Tournament.Create(
				request.NewTournamentName,
				request.NewTournamentStartDate,
				null);

			CategoryId generalCategoryId = sourceTournament.Categories.First(c => c.Name == Tournament.GeneralCategoryName).Id;

			// Map that maps from old category id to the new category id
			IDictionary<CategoryId, CategoryId> categoryMap = new Dictionary<CategoryId, CategoryId>();

			foreach (var category in sourceTournament.Categories)
			{
				if (category.Name != Tournament.GeneralCategoryName)
				{
					var addCategoryReq = newTournament.AddCategory(category.Name);
					if (addCategoryReq.IsError)
					{
						errors.AddRange(addCategoryReq.Errors);
					}
				}
			}

			_context.Tournaments.Add(newTournament);

			// Let the database set the categories ID
			await _context.SaveChangesAsync(cancellationToken);

			foreach (var srcCategory in sourceTournament.Categories)
			{
				foreach (var newCategory in newTournament.Categories)
				{
					if (srcCategory.Name == newCategory.Name)
					{
						categoryMap.Add(srcCategory.Id, newCategory.Id);
					}
				}
			}

			// Add inscriptions
			var tournamentLeaderBoard = _leaderBoardRepository.GetTournamentLeaderBoard(tournamentId.Value);

			var generalCategoryLeaderboard = tournamentLeaderBoard.Where(c => c.CategoryId == generalCategoryId);

			int inscriptionN = 0;

			// Add inscriptions, with inscription number equal to the fisher's position in the leaderboard
			foreach (var inscription in sourceTournament.Inscriptions)
			{
				++inscriptionN;

				if (categoryMap.TryGetValue(inscription.CategoryId, out var dstCat))
				{
					var position = generalCategoryLeaderboard
						.FirstOrDefault(i => i.FisherId == inscription.FisherId)?.Position ?? inscriptionN;

					var inscReq = newTournament.AddInscription(inscription.FisherId, dstCat, position, _dateTimeProvider);

					if (inscReq.IsError)
					{
						errors.AddRange(inscReq.Errors);
					}
				}
			}

			await _context.SaveChangesAsync(cancellationToken);

			return new CloneTournamentCommandResult(
				newTournament.Id.ToString(),
				newTournament.Name,
				newTournament.StartDate,
				newTournament.EndDate,
				errors.Any() ? errors : null);
		}
	}
}
