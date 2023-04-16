# Implementation
---
## Architecture
The architecture is based on the Onion Architecture following DDD. Each layer has its own responsibilities. The layers are:
- Presentation 
- Infrastructure
- Application
- Domain
The folder structure should be a reflection of this architecture, plus the tests.

## Stack
The architecture allows multiple presentation but the current implementation is an API REST:
- ASP.Net Core because it's multi platform and has a lot of support. 
- Entity Framework Core for the ORM.
- SQLITE for the database. Because the project needs to be portable.

# Internals
The project is divided in 3 main parts:
- The Presentation/API: Consumes the application layer and exposes the endpoints.
  1. Converts the user requests to commands and queries.
       - Done with the mapster library.
  2. Sends commands and query to the application layer trough the Mediator pattern.
       - Done with the MediatR library.
  3. Converts the Q/C response to the user.
  - First implemented with MVC controllers but now it uses [Minimal Api](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0).
- The Application: Handles the validation and execution of commands and queries.
  1. Validation is done with the FluentValidation library.
  2. Then some business validation is done to prevent invalid states.
       - Done with ErrorOr library. Possible alternative is the [DomainResult](https://www.nuget.org/packages/DomainResult) library.
- The domain: Contains the business logic and the entities.
  - Consists of aggregates, entities and value objects.
  - Define the possible errors.
- The infrastructure: Contains the implementation of the interfaces defined in the domain and application layers.
  - The database context.
    - Entities Configurations
  - The services/providers.
    - e.g. DateTimeProvider
  - The settings

## Domain events
The system publish and handles the following domain events:
- AddedScore:
  - Order: After save changes
  - Handlers:
    1. Update leaderboards
- AddedParticipation:
  - Order: After save changes
  - Handlers:
    1. Update leaderboards
- AddedInscription:
  - Order: After save changes
  - Handlers:
    1. Update leaderboards

> Discussion about domain events below.

# Rules
## Tournament leaderboard
- The tournament leaderboards are calculated on the fly.
- Add score triggers an domain event that is handled and updates the leaderboard.

> Why domain event? See [Tournament positions](private/3_learning.md##tournament-positions)


# Discussions
## 1. Domain events
### 1.1. Domain events data access
Domain events need to be as close to the domain layer as possible, obviously. The problem lies with the handlers: you can either implement them in the Application layer, leaving the ITournamentDbContext there, or implement them in the Domain layer. The latter gives you two options for accessing the data: 
1. Adding a new DAL/DAO interface that specifies the methods needed to handle the domain events. Harder to maintain and more code to test.
2. Moving ITournamentDbContext to the domain layer. Requires a reference to EFCore.
 
A third option is to add repositories to the domain layer and use them to handle the domain events and command handlers, leaving EFCore exposed to query handlers.

The second and third options are the most appealing to me, definitely the third one is the cleanest but, in the current project state, the second one is easier to implement. Probably will end up with the third one in the long run.

### 1.2. Domain events handlers
> TL;DR; There is no practical alternative to use MediatR in the Domain Layer.
The most common way I have seen domain events implemented is using the MediatR library. The problem with this is that if you want to handle the domain events in the domain layer you need to add a reference to MediatR to the domain layer, violating Clean Architecture rules. Still if you implement the handlers in the App layer, your domain events need to implement `INotificator` which comes from the same library. In this case you can define all the interfaces and implement meditoR by yourself, but I think that the trade off of having MediatR in the Domain layer is worth it.

## Tournament positions/leaderboard
From the system analisis the following items are of importance for this requeriment
- There will be only one notary at the competition. That means no concurrency problems on add score.
- Each tournament might have up to a few hundred competitors per category.

### Where to store it
First, we need to store it beause if we don't it will need to be calculate it on the fly each time, while making sure to apply that rule correctly. 

> "When a fisher is registered in a tournament but does not present to a competition, the system should consider the fisher position to be N + 1. Where N is the position of the fishers that did not fish anything in this competition and N - 1 is the position of the fisher with the lower score in the competition but that did fish something."

**OPT 1: Competition participantion with a position**
- The position is stored in the competition participation.
- "Update leaderboards" will update this field to update the competition leaderboard.
- Get competition leaderboard will do a query to the participations with a ascending position order.
- To query the tournament leaderboard, it will do the same but with the sum of the positions of the competitions.

*Pros*:
- Easy to implement and integrate.

*Cons*:
- Constant change to multiple domain entities.
- Not read optimized.

But the main problem is that participation in a competition may never exist for a given fisher. This means that registered fishers who have not participated in a competition will not have a position that meets the system rules for that competition.

**OPT 2: Read Models**
- Create a read model for the competition leaderboard.
- Create a read model for the tournament leaderboard.
- "Update leaderboards" will update the read models positions.
- Get competition leaderboard will do a query to the read model.
- Get tournament leaderboard will do a query to the read model.

*Pros*
- Read optimized.
- No changes in the domain entities.
- Can use a separated database just for the leaderboards (or read models).

*Cons*
- More complex to implement, requires more configuration and classes.
- Detached from the domain.

Possible implementations of read models:
1. Integrate it in the current projects:
   - Define the domain models in the App or Domain layer in a ReadModels folders under Leaderboard.
   - Add the read models to the DbContext
   - Configure the read models in the infrastructure layer.
   - "Update leaderboards" uses the existing db context to update the read models.
   - Queries can easily join the read models with the domain entities, for example to get the fiser id.
2. Create a new project of ReadModels:
   - Define the domain models in this new project
   - Define how data will be access:
      - Repositories
         - Define the Repositories to access the read models.
     - interface of DbContext (relational)
   - Configure the database to use in the infrastructure layer.
   - Implement the data access interfaces in the infrastructure layer.
   - Queries will use the repositories/context to read/update the read models. 
   - Queries requires at least one extra query to get the fishers data from their ids.


# Algorithms
## Tournament/Competition positions
The algorithm needs achieve the following result:
```
Sample 1:

1° FisherId: 4, Participated: true, Score: 60
2° FisherId: 1, Participated: true, Score: 50
3° FisherId: 2, Participated: true, Score: 35
4° FisherId: 3, Participated: false, Score: -1
4° FisherId: 5, Participated: false, Score: -1

Sample 2:

1° FisherId: 1, Participated: true, Score: 40
1° FisherId: 2, Participated: true, Score: 40
1° FisherId: 3, Participated: true, Score: 40
1° FisherId: 4, Participated: true, Score: 40
1° FisherId: 5, Participated: true, Score: 40

Sample 3:

1° FisherId: 1, Participated: false, Score: -1
1° FisherId: 2, Participated: false, Score: -1
1° FisherId: 3, Participated: false, Score: -1
1° FisherId: 4, Participated: false, Score: -1
1° FisherId: 5, Participated: false, Score: -1

Sample 4:

1° FisherId: 1, Participated: true, Score: 20
2° FisherId: 3, Participated: true, Score: 10
3° FisherId: 5, Participated: true, Score: 5
4° FisherId: 2, Participated: false, Score: -1
4° FisherId: 4, Participated: false, Score: -1

Sample 5:

1° FisherId: 1, Participated: true, Score: 20
2° FisherId: 3, Participated: true, Score: 0
3° FisherId: 5, Participated: true, Score: -1
3° FisherId: 2, Participated: false, Score: -1
3° FisherId: 4, Participated: false, Score: -1
```