using System.Collections;

namespace FisherTournament.UnitTests.Common.TestData;

public class BaseTestData : IEnumerable<object?[]>
{
    private readonly IEnumerable<object?[]> _data;

    public BaseTestData(IEnumerable<object?[]> data)
    {
        _data = data;
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        return _data.GetEnumerator()!;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected static object?[] ItemData(string? data) => new object?[] { data };
    protected static object?[] ItemData(int? data) => new object?[] { data };
    protected static object?[] ItemData(double? data) => new object?[] { data };
}