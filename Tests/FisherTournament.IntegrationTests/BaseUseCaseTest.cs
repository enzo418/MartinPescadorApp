namespace FisherTournament.IntegrationTests
{
    [Collection(nameof(UseCaseTestsCollection))]
    public class BaseUseCaseTest
    {
        protected readonly UseCaseTestsFixture _fixture;

        public BaseUseCaseTest(UseCaseTestsFixture fixture)
        {
            _fixture = fixture;
        }
    }
}