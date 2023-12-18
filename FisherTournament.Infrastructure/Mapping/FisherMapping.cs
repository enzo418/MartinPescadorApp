using FisherTournament.Application.Fishers.Commands.CreateFisher;
using FisherTournament.Application.Fishers.Commands.EditFisher;
using FisherTournament.Application.Fishers.Queries;
using FisherTournament.Contracts.Fisher;
using FisherTournament.Contracts.Fishers;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using Mapster;

namespace FisherTournament.Infrastructure.Common.Mapping;

public class FisherMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateFisherCommand, CreateFisherRequest>()
               .Map(dest => dest.FirstName, src => src.FirstName)
               .Map(dest => dest.LastName, src => src.LastName)
               .Map(dest => dest.DNI, src => src.DNI);

        config.NewConfig<CreateFisherRequest, CreateFisherCommand>()
               .ConstructUsing(src => new CreateFisherCommand(src.FirstName, src.LastName, src.DNI));

        config.NewConfig<(string Id, EditFisherRequest R), EditFisherCommand>()
            .ConstructUsing(t => new EditFisherCommand(t.Id, t.R.FirstName, t.R.LastName, t.R.DNI));

        config.NewConfig<(FisherId Id, EditFisherRequest R), EditFisherCommand>()
            .ConstructUsing(t => new EditFisherCommand(t.Id.ToString(), t.R.FirstName, t.R.LastName, t.R.DNI));

        config.NewConfig<CreateFisherCommandResponse, CreateFisherResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<GetFishersRequest, GetFishersQuery>();
    }
}