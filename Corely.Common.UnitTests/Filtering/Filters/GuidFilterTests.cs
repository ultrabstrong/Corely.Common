using Corely.Common.Filtering;
using Corely.Common.Filtering.Filters;

namespace Corely.Common.UnitTests.Filtering.Filters;

public class GuidFilterTests
{
    private static readonly Guid Id1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid Id2 = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid Id3 = Guid.Parse("00000000-0000-0000-0000-000000000003");

    private static readonly List<TestEntity> TestData =
    [
        new()
        {
            Name = "A",
            AccountId = Id1,
            ParentId = null,
        },
        new()
        {
            Name = "B",
            AccountId = Id2,
            ParentId = Id1,
        },
        new()
        {
            Name = "C",
            AccountId = Id3,
            ParentId = Id2,
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
            () => Filter.For<TestEntity>().Where(e => Guid.NewGuid(), GuidFilter.Equals(Id1))
        );
        Assert.Contains("member access expression", ex.Message);
    }

    [Fact]
    public void Equals_FiltersToExactGuid()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.AccountId, GuidFilter.Equals(Id2));
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal(Id2, result.AccountId);
    }

    [Fact]
    public void NotEquals_ExcludesExactGuid()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.AccountId, GuidFilter.NotEquals(Id2));
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.NotEqual(Id2, e.AccountId));
    }

    [Fact]
    public void In_FiltersToGuidSet()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.AccountId, GuidFilter.In(Id1, Id3));
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.True(e.AccountId == Id1 || e.AccountId == Id3));
    }

    [Fact]
    public void NotIn_ExcludesGuidSet()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.AccountId, GuidFilter.NotIn(Id1, Id3));
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Equal(Id2, result.AccountId);
    }

    [Fact]
    public void IsNull_FiltersNullableGuidToNull()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.ParentId, GuidFilter.IsNull());
        var results = ApplyFilter(builder);
        var result = Assert.Single(results);
        Assert.Null(result.ParentId);
    }

    [Fact]
    public void IsNotNull_FiltersNullableGuidToNonNull()
    {
        var builder = Filter.For<TestEntity>().Where(e => e.ParentId, GuidFilter.IsNotNull());
        var results = ApplyFilter(builder);
        Assert.Equal(2, results.Count);
        Assert.All(results, e => Assert.NotNull(e.ParentId));
    }
}
