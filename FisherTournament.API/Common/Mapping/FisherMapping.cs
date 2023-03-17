using FisherTournament.Application.Fishers.Commands.CreateFisher;
using FisherTournament.Contracts.Fishers;
using Mapster;

namespace FisherTournament.API.Common.Mapping;

public class FisherMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateFisherCommand, CreateFisherRequest>();
        config.NewConfig<CreateFisherCommandResponse, CreateFisherResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}