using FisherTournament.Application.Fishers.Commands.CreateFisher;
using FisherTournament.Application.Fishers.Queries;
using FisherTournament.Contracts.Fisher;
using FisherTournament.Contracts.Fishers;
using Mapster;

namespace FisherTournament.Infrastracture.Common.Mapping;

public class FisherMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateFisherCommand, CreateFisherRequest>();
        config.NewConfig<CreateFisherCommandResponse, CreateFisherResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);


        config.NewConfig<GetFishersRequest, GetFishersQuery>();
    }
}