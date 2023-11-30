using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Domain.FisherAggregate;

public sealed class Fisher : AggregateRoot<FisherId>
{
	public string Name { get; private set; }

	private Fisher(FisherId id, string name) : base(id)
	{
		Name = name;
	}

	public static string GetName(string firstName, string secondName)
	{
		return $"{secondName} {firstName}";
	}

	public static Fisher Create(string firstName, string secondName)
	{
		return new Fisher(Guid.NewGuid(), GetName(firstName, secondName));
	}

	public void ChangeName(string firstName, string secondName)
	{
		Name = GetName(firstName, secondName);
	}

#pragma warning disable CS8618
	private Fisher()
	{
	}
#pragma warning restore CS8618
}