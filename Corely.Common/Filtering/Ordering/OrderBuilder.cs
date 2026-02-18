using System.Linq.Expressions;

namespace Corely.Common.Filtering.Ordering;

public class OrderBuilder<T>
{
    private readonly List<OrderClause<T>> _clauses = [];

    internal OrderBuilder() { }

    public OrderBuilder<T> By<TProperty>(
        Expression<Func<T, TProperty>> property,
        SortDirection direction = SortDirection.Ascending
    )
    {
        _clauses.Clear();
        _clauses.Add(new OrderClause<T>(property, direction, IsPrimary: true));
        return this;
    }

    public OrderBuilder<T> ThenBy<TProperty>(
        Expression<Func<T, TProperty>> property,
        SortDirection direction = SortDirection.Ascending
    )
    {
        if (_clauses.Count == 0)
        {
            throw new InvalidOperationException("ThenBy must be called after By.");
        }

        _clauses.Add(new OrderClause<T>(property, direction, IsPrimary: false));
        return this;
    }

    public IReadOnlyList<OrderClause<T>> Build() => _clauses.AsReadOnly();

    public IOrderedQueryable<T> Apply(IQueryable<T> query)
    {
        if (_clauses.Count == 0)
        {
            throw new InvalidOperationException("No ordering specified. Call By() first.");
        }

        foreach (var clause in _clauses)
        {
            var methodName = (clause.IsPrimary, clause.Direction) switch
            {
                (true, SortDirection.Ascending) => nameof(Queryable.OrderBy),
                (true, _) => nameof(Queryable.OrderByDescending),
                (false, SortDirection.Ascending) => nameof(Queryable.ThenBy),
                (false, _) => nameof(Queryable.ThenByDescending),
            };

            query = query.Provider.CreateQuery<T>(
                Expression.Call(
                    typeof(Queryable),
                    methodName,
                    [typeof(T), clause.PropertyExpression.ReturnType],
                    query.Expression,
                    Expression.Quote(clause.PropertyExpression)
                )
            );
        }

        return (IOrderedQueryable<T>)query;
    }
}

public record OrderClause<T>(
    LambdaExpression PropertyExpression,
    SortDirection Direction,
    bool IsPrimary
);
