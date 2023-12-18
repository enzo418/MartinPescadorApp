namespace FisherTournament.IntegrationTests
{
    [Collection(nameof(UseCaseTestsCollection))]
    public class BaseUseCaseTest : IDisposable
    {
        protected readonly UseCaseTestsFixture _fixture;

        public BaseUseCaseTest(UseCaseTestsFixture fixture)
        {
            _fixture = fixture;
        }

        public void Dispose()
        {
            _fixture.LeaderboardRepositoryMock = null;
        }
    }
}