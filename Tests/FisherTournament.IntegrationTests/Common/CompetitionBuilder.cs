using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.IntegrationTests.Common
{
    public class CompetitionBuilder
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private Competition _competition = null!;
        private TournamentId _tournamentId = null!;
        private Location _location = null!;
        private List<(FisherId, int)> _scores = new();

        private CompetitionBuilder(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public static CompetitionBuilder Create(IDateTimeProvider dateTimeProvider)
        {
            return new CompetitionBuilder(dateTimeProvider);
        }

        public CompetitionBuilder WithScore(Guid fisherId, int score)
        {
            _scores.Add((fisherId, score));
            return this;
        }

        public CompetitionBuilder WithTournament(TournamentId tournamentId)
        {
            _tournamentId = tournamentId;
            return this;
        }

        public CompetitionBuilder WithLocation(Location location)
        {
            _location = location;
            return this;
        }

        public Competition Build()
        {
            _competition = Competition.Create(_dateTimeProvider.Now, _tournamentId, _location);

            foreach (var (fisherId, score) in _scores)
            {
                _competition.AddScore(fisherId, score, _dateTimeProvider);
            }

            return _competition;
        }
    }
}