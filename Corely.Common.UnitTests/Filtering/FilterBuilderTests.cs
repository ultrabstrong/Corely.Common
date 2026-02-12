using Corely.Common.Filtering;
using Corely.Common.Filtering.Filters;

namespace Corely.Common.UnitTests.Filtering;

public class FilterBuilderTests
{
    private static readonly List<TestEntity> TestData =
    [
        new()
        {
            Name = "Alice",
            Age = 25,
            IsActive = true,
            Children =
            [
                new() { Name = "Child1", Score = 90 },
                new() { Name = "Child2", Score = 60 },
            ],
        },
        new()
        {
            Name = "Bob",
            Age = 30,
            IsActive = false,
            Children = [new() { Name = "Child3", Score = 80 }],
        },
        new()
        {
            Name = "Charlie",
            Age = 35,
            IsActive = true,
            Children = [],
        },
    ];

    private List<TestEntity> ApplyFilter(FilterBuilder<TestEntity> builder)
    {
        var predicate = builder.Build();
        return predicate == null ? TestData : TestData.AsQueryable().Where(predicate).ToList();
    }

    [Fact]
    public void Build_WithNoFilters_ReturnsNull()
    {
        var builder = Filter.For<TestEntity>();
        Assert.Null(builder.Build());
    }

    [Fact]
    public void Build_WithNoFilters_ReturnsAllResults()
    {
        var builder = Filter.For<TestEntity>();
        var results = ApplyFilter(builder);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void MultipleWhere_CombinesWithAnd()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(e => e.IsActive, BoolFilter.IsTrue())
            .Where(e => e.Age, ComparableFilter<int>.GreaterThan(30));
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal("Charlie", result.Name);
    }

    [Fact]
    public void MultipleWhere_ThreeConditions_AllApplied()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(e => e.IsActive, BoolFilter.IsTrue())
            .Where(e => e.Age, ComparableFilter<int>.GreaterThanOrEqual(25))
            .Where(e => e.Name, StringFilter.Contains("li"));
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.True(e.Name == "Alice" || e.Name == "Charlie"));
    }

    [Fact]
    public void NestedCollectionFilter_FiltersParentByChildPredicate()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(
                e => e.Children,
                children => children.Where(c => c.Score, ComparableFilter<int>.GreaterThan(85))
            );
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public void NestedCollectionFilter_WithStringFilter_Works()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(
                e => e.Children,
                children => children.Where(c => c.Name, StringFilter.Contains("Child3"))
            );
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal("Bob", result.Name);
    }

    [Fact]
    public void NestedCollectionFilter_WithMultipleChildConditions_Works()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(
                e => e.Children,
                children =>
                    children
                        .Where(c => c.Name, StringFilter.StartsWith("Child"))
                        .Where(c => c.Score, ComparableFilter<int>.GreaterThanOrEqual(80))
            );
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void NestedCollectionFilter_EmptyCollection_ExcludesParent()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(
                e => e.Children,
                children => children.Where(c => c.Score, ComparableFilter<int>.GreaterThan(0))
            );
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.NotEqual("Charlie", e.Name));
    }

    [Fact]
    public void NestedCollectionFilter_SecondLevel_ThrowsInvalidOperation()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () =>
                Filter
                    .For<TestEntity>()
                    .Where(
                        e => e.Children,
                        children =>
                            children.Where(c => (IEnumerable<TestChild>)new List<TestChild>(), _ => { })
                    )
        );
        Assert.Contains("one level deep", ex.Message);
    }

    [Fact]
    public void NestedCollectionFilter_EmptyChildFilter_IsIgnored()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.Children, children => { });
        var results = ApplyFilter(builder);
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public void CombinedPropertyAndCollectionFilter_Works()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(e => e.IsActive, BoolFilter.IsTrue())
            .Where(
                e => e.Children,
                children => children.Where(c => c.Score, ComparableFilter<int>.GreaterThan(50))
            );
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal("Alice", result.Name);
    }
}
