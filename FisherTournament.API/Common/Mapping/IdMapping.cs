using FisherTournament.Domain;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using Mapster;

namespace FisherTournament.API.Common.Mapping
{
    public class IdMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.Default.MapToConstructor(true);

            // TODO: Add assembly scan in a common function because
            // i do this in several places
            RegisterFor<FisherId>(config);
            RegisterFor<CompetitionId>(config);
            RegisterFor<TournamentId>(config);
        }

        private static void RegisterFor<T>(TypeAdapterConfig config)
            where T : GuidId<T>
        {
            config.NewConfig<string, T>()
                .MapWith(s => GuidId<T>.Create(s));
        }
    }
}