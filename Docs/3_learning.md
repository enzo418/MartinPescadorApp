# Description
While i wrote this project i learned a lot of things. I will try to questions that come to my mind and try to answer them.

# Content
- [Description](#description)
- [Content](#content)
- [Aggregate Ids vs Entities Ids](#aggregate-ids-vs-entities-ids)
  - [I Gave up on EF7 "HaveConversion"](#i-gave-up-on-ef7-haveconversion)
    - [Update 1: It will not be so painfully because I wrote this extension](#update-1-it-will-not-be-so-painfully-because-i-wrote-this-extension)
- [Writing the painful configurations](#writing-the-painful-configurations)
- [Where in field](#where-in-field)
- [ASP.NET doesn't support IdTypes as route parameters](#aspnet-doesnt-support-idtypes-as-route-parameters)
  - [Solution](#solution)


# Aggregate Ids vs Entities Ids
So far i applied the rule:
- Aggregate -> Global unique id - As C# Guid, low collision probability.
- Entity -> Integer auto incremental id - Managed by the database.

Using A.M. implementation which takes the Id as a template parameter, but it makes sense to remove the template parameter and force entities to use integer ids to communicate that are auto generated, oppossed to GUID.

> **Note:** [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers/tree/dev/src/Services/Ordering/Ordering.Domain/SeedWork) uses the same approach of entities with integer ids.

## I Gave up on EF7 "HaveConversion"
I Tried to use the new EF7 "HaveConversion" to tell EF Core how to map the GuidId objects to/from the database. It works fine... until you use OwnsOne or OwnsMany in an Id type that you previously added a conversion with "HaveConversion". It will throw an exception without a clear message "Object not set to an instance of an object", a [EF dev says](https://github.com/dotnet/efcore/issues/30373#issuecomment-1451789198) it's because the type is first treated as a value object and then as an entity, which makes sense, but that is not intentional from my part. The issue is still open (Added to EF Backlog 3 Days ago since I wrote this), so there is no solution yet and they know that the message is 💩.

**A User in a similar issue:**

> *Following DDD, aggregate root can only  reference other aggregate roots by their ID, in this case ProductId*

**The same dev response:**

> *For the same reasons that DDD doesn't allow anything other than a manual ID here, EF also doesn't support anything other than a manual ID here. That is, you cannot set this up as a relationship that EF manages, because doing so would result in aggregates that reference each others internals.*
    https://github.com/dotnet/efcore/issues/30203#issuecomment-1465293806

This part of his response
> because doing so would result in aggregates that reference each others internals.

doesn't make any sense, because you are only using the internal id type inside the "HaveConversion" converter, but the other aggregates still are agnostic of it. The only new stuff would be supporting a value key type.

Anyway they will fix it? fine, else I don't care. the most viable alternative I see beyond using "manual ID" is adding the id types conversion in each usage while configuring 🫠, maybe extensions help a little bit. And of course list of ids is more than not supported at this point, so I will have to serialize/deserialize them to/from json string, if it's really impossible.

### Update 1: It will not be so painfully because I wrote this extension

> TL;DR; EF Core doesn't support value objects as keys and they don't plan to do so. C# Doesn't have global aliases so no `FuisherId = Guid`. An extension method was needed to avoid writing the same a few times. Conclusion: In this case using a no relational DB would be easier to setup but of course EF Core would not be an option there.

```c#
//// FisherConfiguration.cs

// BEFORE:
var converter = new ValueConverter<FisherId, Guid>(
    x => x.Value,
    x => FisherId.Create(x));

builder.ToTable("Fishers");
builder.HasKey(x => x.Id);
builder.Property(x => x.Id)
        .HasConversion(converter);
builder.Property(x => x.Name).IsRequired();

// NOW:
builder.ToTable("Fishers");
builder.HasGuidIdKey(x => x.Id);
builder.Property(x => x.Name).IsRequired();

//// Extension.cs
public static void HasGuidIdKey<F, T>(
        this EntityTypeBuilder<F> modelBuilder,
        Expression<Func<F, T>> keyExpression)
    where T : GuidId<T>
    where F : class
{
    // cast keyExpression to Expression<Func<F, object?>>
    Expression<Func<F, object?>> keyExpression2 = Expression.Lambda<Func<F, object?>>(
        keyExpression.Body,
        keyExpression.Parameters);

    var propertyBuilder = modelBuilder.HasKey(
        keyExpression2);

    var converter = new ValueConverter<T, Guid>(
        x => x.Value,
        x => GuidId<T>.Create(x));

    modelBuilder.Property(keyExpression)
                .HasConversion(converter);
}
```

andd this line works flawlessly 😎

*Note that CompetitionsIds is a `IReadOnlyCollection<CompetitionId>` that is backed by a private field `List<CompetitionId>`*
```c#
builder.OwnsMany(p => p.CompetitionsIds, ci =>
{
    ci.ToTable("TournamentCompetitions");
    ci.WithOwner().HasForeignKey("TournamentId");
    ci.HasKey("Id");
    ci.Property(c => c.Value)
        .HasColumnName("CompetitionId")
        .ValueGeneratedNever();
}).UsePropertyAccessMode(PropertyAccessMode.Field);
```

EF sucessfully detects the Tournament Id from the "Tournament" table and "Id" Column, and the same with the "CompetitionId".
Generating the following migration code
```c#
migrationBuilder.CreateTable(
name: "TournamentCompetitions",
columns: table => new
{
    Id = table.Column<int>(type: "int", nullable: false)
        .Annotation("SqlServer:Identity", "1, 1"),
    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
    CompetitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
},
constraints: table =>
{
    table.PrimaryKey("PK_TournamentCompetitions", x => x.Id);
    table.ForeignKey(
        name: "FK_TournamentCompetitions_Tournaments_TournamentId",
        column: x => x.TournamentId,
        principalTable: "Tournaments",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
});
```
Did you see it? There is no ForeignKey to the Competition table. Well, it doesn't matter to me.

> **NOTE:** All this was done the private repo called EFTests

All of this would not be a problem if c# had global type aliases.

# Writing the painful configurations
Don't rush to write all the foreign keys while using Strongly-typed ids because it will be hard to make it work the first time. Is better to `builder.Ignore(p => p.Field)` all the fields that are complex, and once it works you start adding them.
1. Configure all primitive types and PK
2. Configure owned types, if one throws continue to FK
3. Configure FK, make sure if the FK is GuidId it has a conversion.
4. In nested OwnedMany, you must USE the parents keys in `WithOwner().HasForeignKey(...)` and then add them as part of the PK in `HasKey("Id", ...)` or it will them something like "Unable to determine the owner for the relationship between ..." https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities#collections-of-owned-types

> **NOTE:** SQLite does not support generated values on composite keys. e.g. 'FishCaught' has composite key '{'Id', 'CompetitionParticipationId', 'CompetitionId'}'

# Where in field
Is where actually making a query? is it lazy loading even if it's not virtual? Does Find instead of query makes a query?
Asw: No lazy loading in non private fields.

# ASP.NET doesn't support IdTypes as route parameters
```c#
[HttpPost]
    public async Task<IActionResult> Create(
        AddCompetitionsRequest request,
        [FromRoute] TournamentId tournamentId) {...}
```

throws

`System.InvalidOperationException: Could not create an instance of type 'FisherTournament.Domain.TournamentAggregate.ValueObjects.TournamentId'. Model bound complex types must not be abstract or value types and must have a parameterless constructor. Record types must have a single primary constructor. Alternatively, give the 'tournamentId' parameter a non-null default value`

## Solution
Register a [TypeConverter](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.typeconverter?view=net-7.0) but only an attribute is available so i would need to modify each Id.

```c#
// GuidIdConverter
public class GuidIdConverter<T> : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) /*|| sourceType == typeof(int)*/;

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) /* || destinationType == typeof(int)*/;

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            string s => GuidId<T>.Create(s) ?? throw new ArgumentException($"Cannot convert from {value} to ProductId", nameof(value)),
            null => null,
            _ => throw new ArgumentException($"Cannot convert from {value} to ProductId", nameof(value))
        };
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            return value switch
            {
                GuidId<T> id => id.ToString(),
                null => null,
                _ => throw new ArgumentException($"Cannot convert {value} to string", nameof(value))
            };
        }

        throw new ArgumentException($"Cannot convert {value ?? "(null)"} to {destinationType}", nameof(destinationType));
    }
}

// DependencyInjection
TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(GuidIdConverter<T>)));
```

**Now it works and the response on invalids Id is this if your were asking**
```json
<rest of problemjson>

  "errors": {
    "tournamentId": [
      "The value '1' is not valid."
    ]
  }
```

Truly beautiful.

> Of course i am ususing a lot the word Guid but that doesn't mean that i thight the id to be a Guid. You can chage the internal to be a string or int or whatever you want. But i know i should have named it other way.