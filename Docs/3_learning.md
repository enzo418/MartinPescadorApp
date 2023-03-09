# While i wrote this project i learned a lot of things. I will try to questions that come to my mind and try to answer them.

# Aggregate Ids vs Entities Ids
So far i applied the rule:
- Aggregate -> Global unique id - As C# Guid, low collision probability.
- Entity -> Integer auto incremental id - Managed by the database.

Using A.M. implementation which takes the Id as a template parameter, but it makes sense to remove the template parameter and force entities to use integer ids to communicate that are auto generated, oppossed to GUID.

> **Note:** [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers/tree/dev/src/Services/Ordering/Ordering.Domain/SeedWork) uses the same approach of entities with integer ids.