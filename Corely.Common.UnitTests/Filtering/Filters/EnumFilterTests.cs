using Corely.Common.Filtering;
using Corely.Common.Filtering.Filters;

namespace Corely.Common.UnitTests.Filtering.Filters;

public class EnumFilterTests
{
    private static readonly List<TestEntity> TestData =
    [
        new()
        {
            Name = "A",
            Status = TestStatus.Active,
            NullableStatus = TestStatus.Active,
        },
        new()
        {
            Name = "B",
            Status = TestStatus.Inactive,
            NullableStatus = null,
        },
        new()
        {
            Name = "C",
            Status = TestStatus.Pending,
            NullableStatus = TestStatus.Pending,
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
            () => Filter
                .For<TestEntity>()
                .Where(e => (TestStatus)1, EnumFilter<TestStatus>.Equals(TestStatus.Active))
        );
        Assert.Contains("member access expression", ex.Message);
    }

    [Fact]
    public void Equals_FiltersToExactEnum()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(e => e.Status, EnumFilter<TestStatus>.Equals(TestStatus.Active));
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal(TestStatus.Active, result.Status);
    }

    [Fact]
    public void NotEquals_ExcludesExactEnum()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(e => e.Status, EnumFilter<TestStatus>.NotEquals(TestStatus.Active));
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.NotEqual(TestStatus.Active, e.Status));
    }

    [Fact]
    public void In_FiltersToEnumSet()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(e => e.Status, EnumFilter<TestStatus>.In(TestStatus.Active, TestStatus.Pending));
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.True(e.Status == TestStatus.Active || e.Status == TestStatus.Pending));
    }

    [Fact]
    public void NotIn_ExcludesEnumSet()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(
                e => e.Status,
                EnumFilter<TestStatus>.NotIn(TestStatus.Active, TestStatus.Pending)
            );
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal(TestStatus.Inactive, result.Status);
    }

    [Fact]
    public void IsNull_FiltersNullableEnumToNull()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(e => e.NullableStatus, EnumFilter<TestStatus>.IsNull());
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Null(result.NullableStatus);
    }

    [Fact]
    public void IsNotNull_FiltersNullableEnumToNonNull()
    {
        var builder = Filter
            .For<TestEntity>()
            .Where(e => e.NullableStatus, EnumFilter<TestStatus>.IsNotNull());
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.NotNull(e.NullableStatus));
    }
}
