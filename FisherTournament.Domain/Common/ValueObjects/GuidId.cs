using System.Reflection;

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

    public static T Create(Guid id)
    {
        return (T)Activator.CreateInstance(
                    typeof(T),
                    BindingFlags.Instance
                        | BindingFlags.NonPublic
                        | BindingFlags.Public,
                    null, new object[] { id }, null)!;
    }

    public static T Create(string id)
    {
        return (T)Activator.CreateInstance(
                    typeof(T),
                    BindingFlags.Instance
                        | BindingFlags.NonPublic
                        | BindingFlags.Public,
                    null, new object[] { Guid.Parse(id) }, null)!;
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
}