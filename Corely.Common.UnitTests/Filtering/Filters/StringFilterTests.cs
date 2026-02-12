using Corely.Common.Filtering;
using Corely.Common.Filtering.Filters;

namespace Corely.Common.UnitTests.Filtering.Filters;

public class StringFilterTests
{
    private static readonly List<TestEntity> TestData =
    [
        new() { Name = "Alice", Description = "Engineer" },
        new() { Name = "Bob", Description = null },
        new() { Name = "Charlie", Description = "Designer" },
        new() { Name = "Alice", Description = "Manager" },
    ];

    private List<TestEntity> ApplyFilter(FilterBuilder<TestEntity> builder)
    {
        var predicate = builder.Build();
        return predicate == null ? TestData : TestData.AsQueryable().Where(predicate).ToList();
    }

    [Fact]
    public void Where_WithNonMemberAccessSelector_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => Filter.For<TestEntity>().Where(e => nameof(TestEntity.Name), StringFilter.Equals("asdf"))
        );
        Assert.Contains("member access expression", ex.Message);
    }

    [Fact]
    public void Equals_FiltersToExactMatch()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.Equals("Alice"));
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.Equal("Alice", e.Name));
    }

    [Fact]
    public void NotEquals_ExcludesExactMatch()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.NotEquals("Alice"));
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.NotEqual("Alice", e.Name));
    }

    [Fact]
    public void Contains_FiltersToPartialMatch()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.Contains("li"));
        var results = ApplyFilter(builder);
        Assert.Equal(3, results.Count);
        Assert.All(results, e => Assert.Contains("li", e.Name));
    }

    [Fact]
    public void NotContains_ExcludesPartialMatch()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.NotContains("li"));
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.DoesNotContain("li", result.Name);
    }

    [Fact]
    public void StartsWith_FiltersToPrefix()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.StartsWith("Al"));
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.StartsWith("Al", e.Name));
    }

    [Fact]
    public void NotStartsWith_ExcludesPrefix()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.NotStartsWith("Al"));
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.False(e.Name.StartsWith("Al")));
    }

    [Fact]
    public void EndsWith_FiltersToSuffix()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.EndsWith("ob"));
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal("Bob", result.Name);
    }

    [Fact]
    public void NotEndsWith_ExcludesSuffix()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.NotEndsWith("ob"));
        var results = ApplyFilter(builder);
        Assert.Equal(3, results.Count);
        Assert.All(results, e => Assert.False(e.Name.EndsWith("ob")));
    }

    [Fact]
    public void In_FiltersToValueSet()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.In("Alice", "Bob"));
        var results = ApplyFilter(builder);
        Assert.Equal(3, results.Count);
        Assert.All(results, e => Assert.True(e.Name == "Alice" || e.Name == "Bob"));
    }

    [Fact]
    public void NotIn_ExcludesValueSet()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(e => e.Name, StringFilter.NotIn("Alice", "Bob"));
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal("Charlie", result.Name);
    }

    [Fact]
    public void IsNull_FiltersToNullValues()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Description!, StringFilter.IsNull());
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Null(result.Description);
    }

    [Fact]
    public void IsNotNull_FiltersToNonNullValues()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Description!, StringFilter.IsNotNull());
        var results = ApplyFilter(builder);
        Assert.Equal(3, results.Count);
        Assert.All(results, e => Assert.NotNull(e.Description));
    }
}
