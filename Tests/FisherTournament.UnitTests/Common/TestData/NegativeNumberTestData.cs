using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FisherTournament.UnitTests.Common.TestData
{
    public class NegativeNumberTestData : BaseTestData
    {
        public NegativeNumberTestData() : base(new List<object?[]>
        {
            ItemData(0),
            ItemData(-1),
            ItemData(-10),
        })
        { }
    }
}