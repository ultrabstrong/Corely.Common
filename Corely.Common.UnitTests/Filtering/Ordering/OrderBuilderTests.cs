using Corely.Common.Filtering.Ordering;

namespace Corely.Common.UnitTests.Filtering.Ordering;

public class OrderBuilderTests
{
    private static readonly List<TestEntity> TestData =
    [
        new()
        {
            Name = "Charlie",
            Age = 30,
            CreatedDate = new DateTime(2024, 3, 1),
        },
        new()
        {
            Name = "Alice",
            Age = 20,
            CreatedDate = new DateTime(2024, 1, 1),
        },
        new()
        {
            Name = "Bob",
            Age = 20,
            CreatedDate = new DateTime(2024, 2, 1),
        },
    ];

    [Fact]
    public void By_Ascending_SortsByPropertyAscending()
    {
        var order = Order.For<TestEntity>().By(e => e.Name);
        var results = order.Apply(TestData.AsQueryable()).ToList();
        Assert.Equal(["Alice", "Bob", "Charlie"], results.Select(e => e.Name));
    }

    [Fact]
    public void By_Descending_SortsByPropertyDescending()
    {
        var order = Order.For<TestEntity>().By(e => e.Name, SortDirection.Descending);
        var results = order.Apply(TestData.AsQueryable()).ToList();
        Assert.Equal(["Charlie", "Bob", "Alice"], results.Select(e => e.Name));
    }

    [Fact]
    public void ThenBy_AppliesSecondarySort()
    {
        var order = Order.For<TestEntity>().By(e => e.Age).ThenBy(e => e.Name);
        var results = order.Apply(TestData.AsQueryable()).ToList();
        Assert.Equal(["Alice", "Bob", "Charlie"], results.Select(e => e.Name));
    }

    [Fact]
    public void ThenBy_Descending_AppliesSecondarySortDescending()
    {
        var order = Order
            .For<TestEntity>()
            .By(e => e.Age)
            .ThenBy(e => e.Name, SortDirection.Descending);
        var results = order.Apply(TestData.AsQueryable()).ToList();
        Assert.Equal(["Bob", "Alice", "Charlie"], results.Select(e => e.Name));
    }

    [Fact]
    public void MultipleThenBy_AppliesTertiarySort()
    {
        var order = Order
            .For<TestEntity>()
            .By(e => e.Age)
            .ThenBy(e => e.CreatedDate)
            .ThenBy(e => e.Name);
        var results = order.Apply(TestData.AsQueryable()).ToList();
        Assert.Equal(["Alice", "Bob", "Charlie"], results.Select(e => e.Name));
    }

    [Fact]
    public void By_CalledTwice_ResetsToNewSort()
    {
        var order = Order.For<TestEntity>().By(e => e.Age).By(e => e.Name);
        var results = order.Apply(TestData.AsQueryable()).ToList();
        Assert.Equal(["Alice", "Bob", "Charlie"], results.Select(e => e.Name));
    }

    [Fact]
    public void ThenBy_WithoutBy_ThrowsInvalidOperation()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => Order.For<TestEntity>().ThenBy(e => e.Name)
        );
        Assert.Contains("after By", ex.Message);
    }

    [Fact]
    public void Apply_WithNoClauses_ThrowsInvalidOperation()
    {
        var order = Order.For<TestEntity>();
        var ex = Assert.Throws<InvalidOperationException>(
            () => order.Apply(TestData.AsQueryable())
        );
        Assert.Contains("No ordering", ex.Message);
    }

    [Fact]
    public void Build_ReturnsClauseList()
    {
        var order = Order
            .For<TestEntity>()
            .By(e => e.Name, SortDirection.Ascending)
            .ThenBy(e => e.Age, SortDirection.Descending);
        var clauses = order.Build();
        Assert.Equal(2, clauses.Count);
        Assert.Equal(SortDirection.Ascending, clauses[0].Direction);
        Assert.True(clauses[0].IsPrimary);
        Assert.Equal(SortDirection.Descending, clauses[1].Direction);
        Assert.False(clauses[1].IsPrimary);
    }
}
