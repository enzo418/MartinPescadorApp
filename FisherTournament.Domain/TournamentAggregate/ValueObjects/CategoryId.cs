using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FisherTournament.Domain.TournamentAggregate.ValueObjects
{
    public class CategoryId : IntId<CategoryId>
    {
        protected CategoryId(int value) : base(value)
        {
        }
    }
}