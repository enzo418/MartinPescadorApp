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