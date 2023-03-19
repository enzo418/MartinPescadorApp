using System.Linq.Expressions;
using FisherTournament.Application.Common.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FisherTournament.UnitTests.Common.DbContext
{
    public static class MockDbContextFunctions
    {
        public static Task<T?> MockFirstOrDefault<T>(
            this DbSet<T> mockSet)
            where T : class
        {
            return mockSet.FirstOrDefaultAsync(
                            It.IsAny<Expression<Func<T, bool>>>(),
                            It.IsAny<CancellationToken>());
        }
    }
}