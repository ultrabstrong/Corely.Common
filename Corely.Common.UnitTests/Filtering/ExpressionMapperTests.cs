using Corely.Common.Filtering;
using Corely.Common.Filtering.Filters;
using Corely.Common.Filtering.Ordering;

namespace Corely.Common.UnitTests.Filtering;

public class TargetEntity
{
    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public DateTime CreatedDate { get; set; }
    public decimal Price { get; set; }
}

public class ExpressionMapperTests
{
    private static readonly List<TargetEntity> TargetData =
    [
        new()
        {
            Name = "Charlie",
            Age = 30,
            CreatedDate = new DateTime(2024, 3, 1),
            Price = 3.50m,
        },
        new()
        {
            Name = "Alice",
            Age = 20,
            CreatedDate = new DateTime(2024, 1, 1),
            Price = 1.50m,
        },
        new()
        {
            Name = "Bob",
            Age = 25,
            CreatedDate = new DateTime(2024, 2, 1),
            Price = 2.50m,
        },
    ];

    [Fact]
    public void MapPredicate_StringFilter_MapsToTargetType()
    {
        var filter = Filter.For<TestEntity>().Where(e => e.Name, StringFilter.Contains("li"));
        var predicate = filter.Build()!;
        var mapped = ExpressionMapper.MapPredicate<TestEntity, TargetEntity>(predicate);
        var results = TargetData.AsQueryable().Where(mapped).ToList();
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.Contains("li", e.Name));
    }

    [Fact]
    public void MapPredicate_ComparableFilter_MapsToTargetType()
    {
        var filter = Filter
            .For<TestEntity>()
            .Where(e => e.Age, ComparableFilter<int>.GreaterThan(22));
        var predicate = filter.Build()!;
        var mapped = ExpressionMapper.MapPredicate<TestEntity, TargetEntity>(predicate);
        var results = TargetData.AsQueryable().Where(mapped).ToList();
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.True(e.Age > 22));
    }

    [Fact]
    public void MapPredicate_MultipleFilters_MapsAndCombinesCorrectly()
    {
        var filter = Filter
            .For<TestEntity>()
            .Where(e => e.Age, ComparableFilter<int>.GreaterThan(18))
            .Where(e => e.Name, StringFilter.StartsWith("A"));
        var predicate = filter.Build()!;
        var mapped = ExpressionMapper.MapPredicate<TestEntity, TargetEntity>(predicate);
        var results = TargetData.AsQueryable().Where(mapped).ToList();
        var result = Assert.Single(results);
        Assert.Equal("Alice", result.Name);
    }

    [Fact]
    public void MapPredicate_PropertyNotOnTarget_ThrowsInvalidOperation()
    {
        var filter = Filter.For<TestEntity>().Where(e => e.IsActive, BoolFilter.IsTrue());
        var predicate = filter.Build()!;
        var ex = Assert.Throws<InvalidOperationException>(
            () => ExpressionMapper.MapPredicate<TestEntity, TargetEntity>(predicate)
        );
        Assert.Contains("IsActive", ex.Message);
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public void ApplyOrder_AscendingSort_MapsAndSortsCorrectly()
    {
        var order = Order.For<TestEntity>().By(e => e.Name);
        var results = ExpressionMapper
            .ApplyOrder<TestEntity, TargetEntity>(TargetData.AsQueryable(), order)
            .ToList();
        Assert.Equal(["Alice", "Bob", "Charlie"], results.Select(e => e.Name));
    }

    [Fact]
    public void ApplyOrder_DescendingSort_MapsAndSortsCorrectly()
    {
        var order = Order.For<TestEntity>().By(e => e.Age, SortDirection.Descending);
        var results = ExpressionMapper
            .ApplyOrder<TestEntity, TargetEntity>(TargetData.AsQueryable(), order)
            .ToList();
        Assert.Equal(["Charlie", "Bob", "Alice"], results.Select(e => e.Name));
    }

    [Fact]
    public void ApplyOrder_ThenBy_MapsMultipleSortLevels()
    {
        var data = new List<TargetEntity>
        {
            new()
            {
                Name = "B",
                Age = 20,
                CreatedDate = new DateTime(2024, 2, 1),
            },
            new()
            {
                Name = "A",
                Age = 20,
                CreatedDate = new DateTime(2024, 1, 1),
            },
            new()
            {
                Name = "C",
                Age = 30,
                CreatedDate = new DateTime(2024, 3, 1),
            },
        };
        var order = Order.For<TestEntity>().By(e => e.Age).ThenBy(e => e.Name);
        var results = ExpressionMapper
            .ApplyOrder<TestEntity, TargetEntity>(data.AsQueryable(), order)
            .ToList();
        Assert.Equal(["A", "B", "C"], results.Select(e => e.Name));
    }

    [Fact]
    public void ApplyOrder_EmptyClauses_ReturnsUnchangedQuery()
    {
        var order = Order.For<TestEntity>();
        var results = ExpressionMapper
            .ApplyOrder<TestEntity, TargetEntity>(TargetData.AsQueryable(), order)
            .ToList();
        Assert.Equal(3, results.Count);
    }
}
