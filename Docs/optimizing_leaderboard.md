# Context
For now the tournament was updated every time a related event was fired in the domain. This introduces latency in to the user response.

```csharp
public class UpdateLeaderBoardEventsHandler
 : INotificationHandler<ScoreAddedDomainEvent>,
    INotificationHandler<ParticipationAddedDomainEvent>,
    INotificationHandler<InscriptionAddedDomainEvent>
{
    public async Task Handle(ScoreAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        await UpdateLeaderBoard(notification.data);
    }

    public async Task Handle(ParticipationAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        await UpdateLeaderBoard(notification.data);
    }

    public async Task Handle(InscriptionAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        await UpdateLeaderBoard(notification.data);
    }

    private async Task UpdateLeaderBoard(LeaderBoardData data)
    {
        // code of expensive operation
    }
}
```

![](Images/BeforeScheduledLeaderBoardUpdate.png)

As you can see in the image, the add score request is delayed by the time it takes to update the tournament leaderboard.

# Solution
Now it schedules the update of the tournament leaderboard through a service. This service can be implemented in different ways, but the current follows this logic:
- Each job is assigned a unique TournamentID and CategoryID.
- If attempting to add a new job, and there is a scheduled job with matching TournamentID and CategoryID, the CompetitionID of the new job will be added to the previously scheduled job.
- If the CategoryID of the new job differs from the previously scheduled job, the new job will be added independently.
- If there is no pre-existing job associated with the TournamentID, the system will check whether the tournament has been recently updated for the specified CategoryID.
- If the tournament has been updated, a new job will be created and scheduled for execution in 5 seconds.
- If there is no record of a recent update, a new job will be created and scheduled for execution as soon as possible.

> **NOTE:** 5 seconds is used as a reference time, but it can be easily changed. Anyway this is a window that is used to batch the updates.

Important:
- This logic ensures that the leaderboard is updated **as soon as possible under normal circumstances**, but also that they are processed in batches if there are multiple events fired in a short period of time.
- This also **allows the system to replace the schedule with a more complex one**, such as a distributed queue using RabbitMQ where the updater and leader board is handled by microservices and such, without affecting the rest of the system.

```csharp
public class UpdateLeaderBoardEventsHandler
 : INotificationHandler<ScoreAddedDomainEvent>,
    INotificationHandler<ParticipationAddedDomainEvent>,
    INotificationHandler<InscriptionAddedDomainEvent>
{
    private readonly ILeaderBoardUpdateService _leaderBoardUpdateService;

    public UpdateLeaderBoardEventsHandler(ILeaderBoardUpdateService leaderBoardUpdateService)
    {
        _leaderBoardUpdateService = leaderBoardUpdateService;
    }

    public async Task Handle(ScoreAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    { 
        await _leaderBoardUpdateService.ScheduleUpdate(notification.data);
    }

    public async Task Handle(ParticipationAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        await _leaderBoardUpdateService.ScheduleUpdate(notification.data);
    }

    public async Task Handle(InscriptionAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        await _leaderBoardUpdateService.ScheduleUpdate(notification.data);
    }
}
```

Now the request look way cleaner:

![add score request](Images/AfterScheduledLeaderBoardUpdate_request.png)
*add score request*

![Leader board update method](Images/AfterScheduledLeaderBoardUpdate_update.png)
*leader board update method*

# Implementation details

```csharp
    public record Job(TournamentId TournamentId,
                    CategoryId CategoryId,
                    List<CompetitionId> CompetitionsToUpdate);

    public interface ILeaderBoardUpdateScheduler
    {
        /// <summary>
        /// Updates the competition leader board then updates the tournament leader board.
        /// Only for the given category.
        /// </summary>
        /// <param name="tournamentId"></param>
        /// <param name="competitionId"></param>
        /// <param name="categoryId"></param>
        void ScheduleLeaderBoardUpdate(TournamentId tournamentId, CompetitionId competitionId, CategoryId categoryId);

        /// <summary>
        /// Updates all the competition leader boards then updates the tournament leader board.
        /// Only for the given category.
        /// </summary>
        /// <param name="tournamentId"></param>
        void ScheduleLeaderBoardUpdate(TournamentId tournamentId, CategoryId categoryId);

        /// <summary>
        /// Get the next job that is ready to be executed, removing it from the job dictionary.
        /// </summary>
        /// <returns>A Job object representing the next job to be executed, or null if there are no jobs ready.</returns>
        Job? GetNextJob();
    }

```
*Implemented at [LeaderBoardUpdateScheduler.cs](/FisherTournament.Infrastracture/LeaderBoard/LeaderBoardUpdateScheduler.cs)*

# Comparing stress test

**Before**
![](Images/LBUpdateBefore.png)

**After**
![](Images/LBUpdateAfter.png)

You might think that the results were bad, but the important measure here is the number of requests per second, in which it did x2.5 better. The system is now capable of handling 2.5 times more updates than before, resulting in an increase in the number of requests for leaderboard updates. As a result, the databases are becoming overwhelmed, which is causing a slowdown in response times. This can be observed by monitoring the number of leaderboard updates per minute.

I can optimize it more and more but I am happy with the results. I will continue to improve the system in the future, I think at this point a microservice for the leaderboard would be a good idea to reduce the complexity of the core module.