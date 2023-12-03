using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.IntegrationTests.Common
{
    public class CompetitionBuilder
    {
        private readonly ITournamentFisherDbContext? _context = null;
        private readonly IDateTimeProvider _dateTimeProvider;
        private Competition _competition = null!;
        private TournamentId _tournamentId = null!;
        private Location? _location = null;
        private List<(FisherId, int)> _scores = new();
        private int _n = 1;
        private DateTime? _startDate = null;

        private CompetitionBuilder(IDateTimeProvider dateTimeProvider, ITournamentFisherDbContext? context)
        {
            _dateTimeProvider = dateTimeProvider;
            _context = context;
        }

        public static CompetitionBuilder Create(IDateTimeProvider dateTimeProvider)
        {
            return new CompetitionBuilder(dateTimeProvider, null);
        }

        public static CompetitionBuilder Create(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
        {
            return new CompetitionBuilder(dateTimeProvider, context);
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

        public CompetitionBuilder WithStartDate(DateTime startDate)
        {
            _startDate = startDate;

            if (startDate.Kind is not DateTimeKind.Utc)
            {
                throw new ArgumentException("Start date must be UTC", nameof(startDate));
            }

            return this;
        }

        public CompetitionBuilder WithN(int n)
        {
            _n = n;
            return this;
        }

        public Competition Build()
        {
            _competition = Competition.Create(
                _startDate ?? _dateTimeProvider.Now,
                _tournamentId,
                _location ?? Location.Create("city", "state", "country", "place"),
                _n);

            foreach (var (fisherId, score) in _scores)
            {
                _competition.AddScore(fisherId, score, _dateTimeProvider);
            }

            return _competition;
        }

        public async Task<Competition> Build(CancellationToken cancellationToken)
        {
            _competition = Competition.Create(
                _startDate ?? _dateTimeProvider.Now,
                _tournamentId,
                _location ?? Location.Create("city", "state", "country", "place"),
                _n);

            foreach (var (fisherId, score) in _scores)
            {
                _competition.AddScore(fisherId, score, _dateTimeProvider);
            }

            if (_context is not null)
            {
                _context!.Competitions.Add(_competition);
                await _context.SaveChangesAsync(cancellationToken);

                ((DbContext)_context).Entry(_competition).State = EntityState.Detached;
            }

            return _competition;
        }
    }
}