using System.Reflection;
using ErrorOr;
using FisherTournament.Domain.Common.Errors;
namespace FisherTournament.Domain;

public abstract class IntId<T> : ValueObject
{
    public int Value { get; }

    public static implicit operator string(IntId<T> value) => value.Value.ToString();

    public static ErrorOr<T> Create(int id)
    {
        return (T)Activator.CreateInstance(
                   typeof(T),
                   BindingFlags.Instance
                       | BindingFlags.NonPublic
                       | BindingFlags.Public,
                   null, new object[] { id }, null)!;
    }

    public static ErrorOr<T> Create(string id)
    {
        if (Int32.TryParse(id, out var parsedId))
        {
            return (T)Activator.CreateInstance(
                       typeof(T),
                       BindingFlags.Instance
                           | BindingFlags.NonPublic
                           | BindingFlags.Public,
                       null, new object[] { parsedId }, null)!;
        }
        else
        {
            return GenerateErrorWithNames();
        }
    }

    protected IntId(int value)
    {
        Value = value;
    }


    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    private static Error GenerateErrorWithNames()
    {
        var name = typeof(T).Name;
        var entityName = name.Substring(0, name.Length - 2);
        return Errors.Id.NotValidWithType(entityName, name);
    }
}