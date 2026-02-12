namespace Corely.Common.Filtering;

public static class Filter
{
    public static FilterBuilder<T> For<T>() => new();
}
