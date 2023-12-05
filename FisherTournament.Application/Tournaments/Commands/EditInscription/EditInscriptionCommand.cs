using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Commands.EditInscription
{
	public record struct EditInscriptionCommand(string TournamentId, string FisherId, string? CategoryId, int? NewNumber)
		: IRequest<ErrorOr<Updated>>;

	public class EditInscriptionCommandHandler
		: IRequestHandler<EditInscriptionCommand, ErrorOr<Updated>>
	{
		private readonly ITournamentFisherDbContext _context;
		private readonly IDateTimeProvider _dateTimeProvider;

		public EditInscriptionCommandHandler(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
		{
			_context = context;
			_dateTimeProvider = dateTimeProvider;
		}

		public async Task<ErrorOr<Updated>> Handle(EditInscriptionCommand request, CancellationToken cancellationToken)
		{
			var tournamentId = TournamentId.Create(request.TournamentId);

			if (tournamentId.IsError)
			{
				return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
			}

			var fisherId = FisherId.Create(request.FisherId);

			if (fisherId.IsError)
			{
				return Errors.Id.NotValidWithProperty(nameof(request.FisherId));
			}

			var tournament = await _context.Tournaments.FindAsync(tournamentId.Value);

			if (tournament is null)
			{
				return Errors.Tournaments.NotFound;
			}

			var fisherExists = await _context.Fishers.Where(f => f.Id == fisherId.Value).AnyAsync(cancellationToken);

			if (!fisherExists)
			{
				return Errors.Fishers.NotFound;
			}

			CategoryId? categoryId = null;

			if (request.CategoryId != null)
			{
				var categoryIdRes = CategoryId.Create(request.CategoryId);

				if (categoryIdRes.IsError)
				{
					return Errors.Id.NotValidWithProperty(nameof(request.CategoryId));
				}

				categoryId = categoryIdRes.Value;
			}

			bool fisherHasAlreadyScored = await _context.Competitions
							.Where(c => c.TournamentId == tournamentId.Value)
							.Where(c => c.Participations.Any(p => p.FisherId == fisherId.Value))
							.AnyAsync(cancellationToken);

			if (fisherHasAlreadyScored)
			{
				return Errors.Tournaments.FisherHasAlreadyScored;
			}

			var res = tournament.UpdateInscription(fisherId.Value, categoryId, request.NewNumber, _dateTimeProvider);

			if (res.IsError)
			{
				return res.Errors;
			}

			await _context.SaveChangesAsync(cancellationToken);

			return Result.Updated;
		}
	}
}
