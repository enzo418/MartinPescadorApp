using FisherTournament.Application.Tournaments.Commands.AddCategory;
using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Application.Tournaments.Commands.CreateTournament;
using FisherTournament.Application.Tournaments.Commands.EditInscription;
using FisherTournament.Application.Tournaments.Commands.EditTournament;
using FisherTournament.Contracts.Categories;
using FisherTournament.Contracts.Tournaments;
using Mapster;
using TournamentLeaderBoardCategoryContract = FisherTournament.Contracts.Tournaments.TournamentLeaderBoardCategory;
using TournamentLeaderBoardCategoryQuery = FisherTournament.Application.Tournaments.Queries.GetTournamentLeaderBoard.TournamentLeaderBoardCategory;

namespace FisherTournament.Infrastructure.Common.Mapping;

public class TournamentMapping : IRegister
{
	public void Register(TypeAdapterConfig config)
	{
		config.NewConfig<CreateTournamentRequest, CreateTournamentCommand>()
			.Map(dest => dest.Name, src => src.Name)
			.Map(dest => dest.StartDate, src => src.StartDate)
			.Map(dest => dest.EndDate, src => src.EndDate);

		config.NewConfig<(AddInscriptionRequest IR, string tournamentId), AddInscriptionCommand>()
			.MapWith(src => new AddInscriptionCommand(src.tournamentId, src.IR.FisherId, src.IR.CategoryId, src.IR.Number));

		config.NewConfig<(AddCategoryRequest CR, string tournamentId), AddCategoryCommand>()
			.Map(dest => dest.TournamentId, src => src.tournamentId)
			.Map(dest => dest, src => src.CR);

		config.NewConfig<TournamentLeaderBoardCategoryQuery, TournamentLeaderBoardCategoryContract>()
			.Map(dest => dest.LeaderBoard, src => src.LeaderBoard)
			.Map(dest => dest.CategoryId, src => src.Id)
			.Map(dest => dest.CategoryName, src => src.Name);

		config.NewConfig<EditTournamentContract, EditTournamentCommand>();


		config.NewConfig<(EditInscriptionRequest ER, string tournamentId), EditInscriptionCommand>()
			.MapWith(src => new EditInscriptionCommand(src.tournamentId, src.ER.FisherId, src.ER.CategoryId, src.ER.Number));

	}
}