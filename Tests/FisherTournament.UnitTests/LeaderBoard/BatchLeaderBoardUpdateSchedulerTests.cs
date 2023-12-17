using FisherTournament.Application.LeaderBoard;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FisherTournament.UnitTests.LeaderBoard
{
    public class BatchLeaderBoardUpdateSchedulerTests
    {
        private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
        private readonly Mock<ILogger<BatchLeaderBoardUpdateScheduler>> _loggerMock;
        private readonly BatchLeaderBoardUpdateScheduler _scheduler;

        private DateTime RealNow => DateTime.UtcNow;

        // Common test data
        readonly TournamentId tournamentId1 = new(Guid.NewGuid());
        readonly TournamentId tournamentId2 = new(Guid.NewGuid());
        readonly CategoryId categoryId1 = CategoryId.Create(1).Value;
        readonly CategoryId categoryId2 = CategoryId.Create(2).Value;
        readonly CompetitionId competitionId1 = new(Guid.NewGuid());
        readonly CompetitionId competitionId2 = new(Guid.NewGuid());
        readonly CompetitionId competitionId3 = new(Guid.NewGuid());

        public BatchLeaderBoardUpdateSchedulerTests()
        {
            _loggerMock = new Mock<ILogger<BatchLeaderBoardUpdateScheduler>>();
            _dateTimeProviderMock = new Mock<IDateTimeProvider>();
            _scheduler = new BatchLeaderBoardUpdateScheduler(_loggerMock.Object, _dateTimeProviderMock.Object);

            _dateTimeProviderMock.Setup(x => x.Now).Returns(RealNow);
        }

        [Fact]
        public void ScheduleLeaderBoardUpdate_ToSingleCompetition_ShouldReturnToExecuteNow()
        {
            // Arrange

            // Act
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1);

            // Assert
            var job = _scheduler.GetNextJob();
            job.Should().NotBeNull();
            job!.TournamentId.Should().Be(tournamentId1);
            job.CategoryId.Should().Be(categoryId1);
            job.CompetitionsToUpdate.Should().HaveCount(1);
            job.CompetitionsToUpdate.First().Should().Be(competitionId1);
        }

        [Fact]
        public void ScheduleLeaderBoardUpdate_ToSingleCompetition_MultipleTimes_ShouldReturnToExecuteNow()
        {
            // Arrange

            // Act
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1);
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1);
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1);

            // Assert
            var job = _scheduler.GetNextJob();
            job.Should().NotBeNull();
            job!.TournamentId.Should().Be(tournamentId1);
            job.CategoryId.Should().Be(categoryId1);
            job.CompetitionsToUpdate.Should().HaveCount(1);
            job.CompetitionsToUpdate.First().Should().Be(competitionId1);
        }

        [Fact]
        public void ScheduleLeaderBoardUpdates_WithPreviousUpdate_ShouldUpdateItAndReturnBatched()
        {
            // Arrange
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1);

            // Act
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId2, categoryId1);

            // Assert
            var job = _scheduler.GetNextJob();
            job.Should().NotBeNull();
            job!.TournamentId.Should().Be(tournamentId1);
            job.CategoryId.Should().Be(categoryId1);
            job.CompetitionsToUpdate.Should().HaveCount(2);
            job.CompetitionsToUpdate.Should().Contain(competitionId1);
            job.CompetitionsToUpdate.Should().Contain(competitionId2);
        }

        [Fact]
        public void ScheduleCompLeaderBoardUpdate_WithPreviousToUpdateAll_ShouldUseAll()
        {
            // Arrange
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, categoryId1); // tell it to update all the comps

            // Act
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1); // won't change
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId2, categoryId1); // won't change

            // Assert
            var job = _scheduler.GetNextJob();
            job.Should().NotBeNull();
            job!.TournamentId.Should().Be(tournamentId1);
            job.CategoryId.Should().Be(categoryId1);
            job.CompetitionsToUpdate.Should().BeEmpty(); // Count 0 == update all
        }

        [Fact]
        public void ScheduleLeaderBoardUpdate_AllCompetitions_DifferentOrder()
        {
            // Arrange
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, categoryId1);

            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1);

            // Act
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, categoryId1);

            // Assert
            var job = _scheduler.GetNextJob();
            job.Should().NotBeNull();
            job!.TournamentId.Should().Be(tournamentId1);
            job.CategoryId.Should().Be(categoryId1);
            job.CompetitionsToUpdate.Should().BeEmpty();
        }

        [Fact]
        public void GetNextJob_ReturnsNull_WhenNoJobs()
        {
            // Arrange

            // Act
            var job = _scheduler.GetNextJob();

            // Assert
            job.Should().BeNull();
        }

        [Fact]
        public void GetNextJob_ReturnsJob_WhenMultipleJobsReady()
        {
            // Arrange
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1);
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId2, competitionId2, categoryId2);

            // Act
            var job1 = _scheduler.GetNextJob();
            var job2 = _scheduler.GetNextJob();

            // Assert
            job1.Should().NotBeNull();
            job1!.TournamentId.Should().Be(tournamentId1);
            job1.CategoryId.Should().Be(categoryId1);
            job1.CompetitionsToUpdate.Should().HaveCount(1);
            job1.CompetitionsToUpdate.First().Should().Be(competitionId1);

            job2.Should().NotBeNull();
            job2!.TournamentId.Should().Be(tournamentId2);
            job2.CategoryId.Should().Be(categoryId2);
            job2.CompetitionsToUpdate.Should().HaveCount(1);
            job2.CompetitionsToUpdate.First().Should().Be(competitionId2);
        }

        [Fact]
        public void GetNextJob_ReturnsNull_WhenNoJobsReady()
        {
            // Arrange
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1);

            // Since it was the first schedule, it will be ready to run now.
            // To avoid making the scheduler return it, we need to move the clock backwards.
            DateTime nDate = _dateTimeProviderMock.Object.Now.Add(BatchLeaderBoardUpdateScheduler.MaxUpdateInterval * -2);
            _dateTimeProviderMock.Setup(x => x.Now)
                .Returns(nDate);

            // Act
            var job = _scheduler.GetNextJob();

            // Assert
            job.Should().BeNull();
        }

        [Fact]
        public void GetNextJob_WithFrequentCalls_ShouldThrottleUpdates()
        {
            // Arrange
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId1, categoryId1);

            var jobReady1 = _scheduler.GetNextJob();
            // after this line, the next update should not happen until now + MaxUpdateInterval.

            // Act
            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId2, categoryId1);

            // even if we scheduled another update, it should not be ready yet.
            var jobNotReady = _scheduler.GetNextJob();

            _scheduler.ScheduleLeaderBoardUpdate(tournamentId1, competitionId3, categoryId1);

            DateTime timeAfterDelayEnded = _dateTimeProviderMock.Object
                            .Now.Add(BatchLeaderBoardUpdateScheduler.MaxUpdateInterval);

            _dateTimeProviderMock.Setup(x => x.Now).Returns(timeAfterDelayEnded);

            var jobReady2 = _scheduler.GetNextJob();

            // Assert
            jobReady1.Should().NotBeNull();
            jobReady1!.TournamentId.Should().Be(tournamentId1);
            jobReady1.CategoryId.Should().Be(categoryId1);
            jobReady1.CompetitionsToUpdate.Should().HaveCount(1);
            jobReady1.CompetitionsToUpdate.Should().Contain(competitionId1);

            jobNotReady.Should().BeNull();

            jobReady2.Should().NotBeNull();
            jobReady2!.TournamentId.Should().Be(tournamentId1);
            jobReady2.CategoryId.Should().Be(categoryId1);
            jobReady2.CompetitionsToUpdate.Should().HaveCount(2);
            jobReady2.CompetitionsToUpdate.Should().Contain(competitionId2);
            jobReady2.CompetitionsToUpdate.Should().Contain(competitionId3);
        }
    }
}