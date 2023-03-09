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