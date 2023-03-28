using FisherTournament.Application.Tournaments.Commands.AddCategory;
using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Contracts.Categories;
using FisherTournament.Contracts.Tournaments;
using Mapster;

using TournamentLeaderBoardCategoryQuery = FisherTournament.Application.Tournaments.Queries.GetTournamentLeaderBoard.TournamentLeaderBoardCategory;
using TournamentLeaderBoardCategoryContract = FisherTournament.Contracts.Tournaments.TournamentLeaderBoardCategory;

namespace FisherTournament.API.Common.Mapping;

public class TournamentMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<(AddInscriptionRequest IR, string tournamentId), AddInscriptionCommand>()
            .Map(dest => dest.TournamentId, src => src.tournamentId)
            .Map(dest => dest, src => src.IR);

        config.NewConfig<(AddCategoryRequest CR, string tournamentId), AddCategoryCommand>()
            .Map(dest => dest.TournamentId, src => src.tournamentId)
            .Map(dest => dest, src => src.CR);
        
        config.NewConfig<TournamentLeaderBoardCategoryQuery, TournamentLeaderBoardCategoryContract>()
            .Map(dest => dest.LeaderBoard, src => src.LeaderBoard)
            .Map(dest => dest.CategoryId, src => src.Id)
            .Map(dest => dest.CategoryName, src => src.Name);
    }
}