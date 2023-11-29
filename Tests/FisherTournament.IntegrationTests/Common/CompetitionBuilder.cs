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
		private int _n = 1;

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

		public CompetitionBuilder WithN(int n)
		{
			_n = n;
			return this;
		}

		public Competition Build()
		{
			_competition = Competition.Create(_dateTimeProvider.Now, _tournamentId, _location, _n);

			foreach (var (fisherId, score) in _scores)
			{
				_competition.AddScore(fisherId, score, _dateTimeProvider);
			}

			return _competition;
		}
	}
}