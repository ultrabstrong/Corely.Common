using Corely.Common.Filtering;
using Corely.Common.Filtering.Filters;

namespace Corely.Common.UnitTests.Filtering.Filters;

public class BoolFilterTests
{
    private static readonly List<TestEntity> TestData =
    [
        new()
        {
            Name = "A",
            IsActive = true,
            IsVerified = true,
        },
        new()
        {
            Name = "B",
            IsActive = false,
            IsVerified = null,
        },
        new()
        {
            Name = "C",
            IsActive = true,
            IsVerified = false,
        },
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
            () => Filter.For<TestEntity>().Where(e => nameof(TestEntity.IsActive) == "True", BoolFilter.IsTrue())
        );
        Assert.Contains("member access expression", ex.Message);
    }

    [Fact]
    public void IsTrue_FiltersToTrueValues()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.IsActive, BoolFilter.IsTrue());
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.True(e.IsActive));
    }

    [Fact]
    public void IsFalse_FiltersToFalseValues()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.IsActive, BoolFilter.IsFalse());
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.False(result.IsActive);
    }

    [Fact]
    public void IsNull_FiltersNullableBoolToNull()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.IsVerified, BoolFilter.IsNull());
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Null(result.IsVerified);
    }

    [Fact]
    public void IsNotNull_FiltersNullableBoolToNonNull()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.IsVerified, BoolFilter.IsNotNull());
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.NotNull(e.IsVerified));
    }
}
