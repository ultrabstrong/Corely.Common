namespace Corely.Common.Filtering.Ordering;

public static class Order
{
    public static OrderBuilder<T> For<T>() => new();
}
