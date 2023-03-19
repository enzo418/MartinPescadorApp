using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FisherTournament.UnitTests.Common.TestData
{
    public class NullEmptyStringTesData : BaseTestData
    {
        public NullEmptyStringTesData() : base(new List<object?[]>
        {
            ItemData((string)null!),
            ItemData(string.Empty),
            ItemData(" "),
        })
        { }
    }
}