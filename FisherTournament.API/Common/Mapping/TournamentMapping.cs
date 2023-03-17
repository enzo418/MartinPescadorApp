using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Contracts.Tournaments;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using Mapster;

namespace FisherTournament.API.Common.Mapping;

public class TournamentMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<(AddInscriptionRequest IR, string tournamentId), AddInscriptionCommand>()
            .Map(dest => dest.TournamentId, src => src.tournamentId)
            .Map(dest => dest.FisherId, src => src.IR.FisherId);
    }
}