using System.Reflection;
using ErrorOr;
using FisherTournament.Domain.Common.Errors;

namespace FisherTournament.Domain;

public abstract class GuidId<T> : ValueObject
{
    public Guid Value { get; }

    public GuidId()
    {
        Value = Guid.NewGuid();
    }

    public GuidId(Guid value)
    {
        Value = value;
    }

    public static ErrorOr<T> Create(Guid id)
    {
        try
        {
            return (T)Activator.CreateInstance(
                       typeof(T),
                       BindingFlags.Instance
                           | BindingFlags.NonPublic
                           | BindingFlags.Public,
                       null, new object[] { id }, null)!;
        }
        catch (System.Exception)
        {
            return GenerateErrorWithNames();
        }
    }

    public static ErrorOr<T> Create(string id)
    {
        if (Guid.TryParse(id, out var guid))
        {
            return (T)Activator.CreateInstance(
                    typeof(T),
                    BindingFlags.Instance
                        | BindingFlags.NonPublic
                        | BindingFlags.Public,
                    null, new object[] { guid }, null)!;
        }

        return GenerateErrorWithNames();
    }

    public static implicit operator Guid(GuidId<T> value) => value.Value;

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