using FisherTournament.Application.Competitions.Commands.AddScore;
using FisherTournament.Application.Tournaments.Commands.AddCompetitions;
using FisherTournament.Contracts.Competitions;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using Mapster;

namespace FisherTournament.API.Common.Mapping;

public class CompetitionMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AddCompetitionRequest, AddCompetitionCommand>()
            .Map(dest => dest.City, src => src.Location.City)
            .Map(dest => dest.State, src => src.Location.State)
            .Map(dest => dest.Country, src => src.Location.Country)
            .Map(dest => dest.Place, src => src.Location.Place);

        config.NewConfig<(AddCompetitionsRequest CR, TournamentId tournamentId), AddCompetitionsCommand>()
            .Map(dest => dest.TournamentId, src => src.tournamentId)
            .Map(dest => dest.Competitions, src => src.CR.Competitions);

        config.NewConfig<(AddScoreRequest SR, CompetitionId competitionId), AddScoreCommand>()
            .Map(dest => dest.CompetitionId, src => src.competitionId)
            .Map(dest => dest.FisherId, src => src.SR.FisherId)
            .Map(dest => dest.Score, src => src.SR.Score);

        config.NewConfig<Competition, CompetitionResponse>();
        config.NewConfig<List<Competition>, AddCompetitionsResponse>()
            .Map(dest => dest.Competitions, src => src);
    }
}