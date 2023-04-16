# Project FisherTournament


## Optimizations

[**Leader board update**](Docs/optimizing_leaderboard.md)

## Structure
    
```
├── Docs
│   ├── 1_introduction.md
│   ├── 2_implementation.md
│   ├── 4_setup.md
│   ├── Api
│   │   ├── Api.Competition.md
│   │   ├── Api.Fisher.md
│   │   └── Api.Tournament.md
│   └── Diagrams
│       ├── diagrams.juth
│       ├── DomainModel.png
│       ├── TournamentClassDiagram.png
│       └── UseCases.png
├── FisherTournament.API
│   ├── appsettings.Development.json
│   ├── appsettings.json
│   ├── AssemblyReference.cs
│   ├── Common
│   │   ├── CustomResults
│   │   │   └── ProblemResult.cs
│   │   ├── Errors
│   │   │   └── CustomProblemDetailsFactory.cs
│   │   ├── Http
│   │   │   └── HttpContextItemKeys.cs
│   │   └── Mapping
│   │       ├── CompetitionMapping.cs
│   │       ├── DependencyInjection.cs
│   │       ├── FisherMapping.cs
│   │       └── TournamentMapping.cs
│   ├── Controllers[NOTUSED]
│   │   ├── ApiController.cs
│   │   ├── CompetitionController.cs
│   │   ├── ErrorController.cs
│   │   ├── FisherController.cs
│   │   ├── README.md
│   │   └── TournamentController.cs
│   ├── DependencyInjection.cs
│   ├── Endpoints
│   │   ├── Competitions
│   │   │   ├── AddCompetitionsEndpoint.cs
│   │   │   ├── AddScoreEndpoint.cs
│   │   │   └── GetLeaderboardEndpoint.cs
│   │   ├── Fishers
│   │   │   └── CreateFisherEndpoint.cs
│   │   └── Tournaments
│   │       ├── AddInscriptionEndpoint.cs
│   │       └── CreateTournamentEndpoint.cs
│   ├── FisherTournament.API.csproj
│   ├── Program.cs
│   └── Properties
│       └── launchSettings.json
├── FisherTournament.Application
│   ├── Common
│   │   ├── Behavior
│   │   │   ├── ErrorOrBasedValidationBehavior.cs
│   │   │   └── ExceptionBasedValidationBehavior.cs
│   │   ├── Persistence
│   │   │   └── ITournamentFisherDbContext.cs
│   │   └── Validators
│   │       └── DateTimeValidators.cs
│   ├── Competitions
│   │   ├── Commands
│   │   │   └── AddScore
│   │   │       ├── AddScoreCommand.cs
│   │   │       └── AddScoreCommandValidator.cs
│   │   └── Queries
│   │       └── GetLeaderBoard
│   │           └── GetLeaderBoardQuery.cs
│   ├── DependencyInjection.cs
│   ├── Fishers
│   │   └── Commands
│   │       └── CreateFisher
│   │           ├── CreateFisherCommand.cs
│   │           ├── CreateFisherCommandResponse.cs
│   │           └── CreateFisherCommandValidator.cs
│   ├── FisherTournament.Application.csproj
│   └── Tournaments
│       └── Commands
│           ├── AddComepetitions
│           │   ├── AddCompetitionsCommand.cs
│           │   └── AddCompetitionsCommandValidation.cs
│           ├── AddInscription
│           │   ├── AddInscriptionCommand.cs
│           │   └── AddInscriptionCommandValidator.cs
│           └── CreateTournament
│               ├── CreateTournamentCommand.cs
│               ├── CreateTournamentCommandResponse.cs
│               └── CreateTournamentCommandValidator.cs
├── FisherTournament.Contracts
│   ├── Competitions
│   │   ├── AddCompetitionsContracts.cs
│   │   ├── AddScoreContracts.cs
│   │   └── GetLeaderboardContracts.cs
│   ├── Fisher
│   │   └── CreateFisherContracts.cs
│   ├── FisherTournament.Contracts.csproj
│   └── Tournaments
│       ├── AddInscriptionRequest.cs
│       └── CreateTournamentContracts.cs
├── FisherTournament.Domain
│   ├── AggregateRoot.cs
│   ├── Common
│   │   ├── Errors
│   │   │   ├── Errors.Competition.cs
│   │   │   ├── Errors.Fisher.cs
│   │   │   ├── Errors.Id.cs
│   │   │   └── Errors.Tournament.cs
│   │   └── ValueObjects
│   │       └── GuidId.cs
│   ├── CompetitionAggregate
│   │   ├── Competition.cs
│   │   ├── Entities
│   │   │   ├── CompetitionParticipation.cs
│   │   │   └── FishCaught.cs
│   │   └── ValueObjects
│   │       ├── CompetitionId.cs
│   │       └── Location.cs
│   ├── Entity.cs
│   ├── FisherAggregate
│   │   ├── Fisher.cs
│   │   └── ValueObjects
│   │       └── FisherId.cs
│   ├── FisherTournament.Domain.csproj
│   ├── Provider
│   │   └── IDateTimeProvider.cs
│   ├── TournamentAggregate
│   │   ├── Entities
│   │   │   └── TournamentInscription.cs
│   │   ├── Tournament.cs
│   │   └── ValueObjects
│   │       └── TournamentId.cs
│   ├── UserAggregate
│   │   ├── User.cs
│   │   └── ValueObjects
│   │       └── UserId.cs
│   └── ValueObject.cs
├── FisherTournament.Infrastracture
│   ├── DependencyInjection.cs
│   ├── FisherTournament.Infrastracture.csproj
│   ├── Persistence
│   │   ├── Configurations
│   │   │   ├── CompetitionConfiguration.cs
│   │   │   ├── FisherConfiguration.cs
│   │   │   ├── IdConverter.cs
│   │   │   ├── TournamentConfiguration.cs
│   │   │   └── UserConfiguration.cs
│   │   ├── Migrations
│   │   │   ├── 20230315213139_InitialMigration.cs
│   │   │   ├── 20230315213139_InitialMigration.Designer.cs
│   │   │   └── TournamentFisherDbContextModelSnapshot.cs
│   │   └── TournamentFisherDbContext.cs
│   ├── Provider
│   │   └── DateTimeProvider.cs
│   └── Settings
│       └── DataBaseConectionSettings.cs
├── FisherTournament.sln
├── Requests
│   ├── Competition
│   │   ├── AddCompetitionsToTournament.http
│   │   ├── AddScore.http
│   │   └── GetLeaderBoard.http
│   ├── Fisher
│   │   └── CreateFisher.http
│   └── Tournament
│       ├── AddInscription.http
│       └── CreateTournament.http
└── Tests
    ├── FisherTournament.ArchitectureTests
    │   ├── FisherTournament.ArchitectureTests.csproj
    │   ├── LayerDependenciesTests.cs
    │   ├── TestBase.cs
    │   └── Usings.cs
    ├── FisherTournament.IntegrationTests
    │   ├── appsettings.json
    │   ├── BaseUseCaseTest.cs
    │   ├── Competitions
    │   │   ├── Commands
    │   │   │   └── AddScoreHandlerTest.cs
    │   │   └── Queries
    │   │       └── GetLeaderBoardQueryHandlerTest.cs
    │   ├── Fishers
    │   │   └── Commands
    │   │       └── CreateFisherHandlerTest.cs
    │   ├── FisherTournament.IntegrationTests.csproj
    │   ├── Tournaments
    │   │   └── Commands
    │   │       ├── AddCompetitionsHandlerTest.cs
    │   │       ├── AddInscriptionHandlerTest.cs
    │   │       └── CreateTournamentHandlerTest.cs
    │   ├── UseCaseTestsFixture.cs
    │   └── Usings.cs
    └── FisherTournament.UnitTests
        ├── Common
        │   └── TestData
        │       ├── BaseTestData.cs
        │       ├── NegativeNumberTestData.cs
        │       └── NullEmptyStringTesData.cs
        ├── Competitions
        │   ├── Commands
        │   │   └── AddScoreCommandValidatorTest.cs
        │   └── Queries
        ├── Fishers
        │   └── Commands
        │       └── CreateFisherCommandValidatorTest.cs
        ├── FisherTournament.UnitTests.csproj
        ├── Tournaments
        │   └── Commands
        │       ├── AddCompetitionsCommandValidatorTest.cs
        │       ├── AddInscriptionCommandValidatorTest.cs
        │       └── CreateTournamentCommandValidatorTest.cs
        └── Usings.cs
```

> **NOTE:** I know, there is a typo in infrastructure.