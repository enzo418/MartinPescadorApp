using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.IntegrationTests.Common
{
    public class TournamentBuilder
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private ITournamentFisherDbContext _context = null!;
        private List<(FisherId, int number, string categoryName)> _inscriptions = new();
        private Tournament _tournament = null!;
        private string _name = null!;
        private DateTime _startDate;
        private DateTime _endDate;
        private List<string> _categories = new();

        private TournamentBuilder(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        public static TournamentBuilder Create(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
        {
            return new TournamentBuilder(context, dateTimeProvider);
        }

        public TournamentBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public TournamentBuilder WithStartDate(DateTime startDate)
        {
            _startDate = startDate;
            return this;
        }

        public TournamentBuilder WithEndDate(DateTime endDate)
        {
            _endDate = endDate;
            return this;
        }

        public TournamentBuilder WithCategory(string categoryName)
        {
            _categories.Add(categoryName);
            return this;
        }

        public TournamentBuilder WithInscription(FisherId fisherId, int number, string categoryName)
        {
            _inscriptions.Add((fisherId, number, categoryName));
            return this;
        }

        public async Task<Tournament> Build(CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(_name, nameof(_name));

            if (_startDate == _endDate)
                throw new ArgumentException("Missing calls");

            _tournament = Tournament.Create(_name, _startDate, _endDate);

            foreach (var categoryName in _categories)
            {
                _tournament.AddCategory(categoryName);
            }

            // setup categories id
            _context.Tournaments.Add(_tournament);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var (fisherId, number, categoryName) in _inscriptions)
            {
                var categoryId = _tournament.Categories.First(c => c.Name == categoryName).Id;
                if (_tournament.AddInscription(fisherId, categoryId, number, _dateTimeProvider) is var ret && ret.IsError)
                    throw new Exception(ret.FirstError.Code);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return _tournament;
        }
    }
}