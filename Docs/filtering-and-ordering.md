# Filtering & Ordering

A type-safe, expression-based system for building filter predicates and sort orders against any entity type. Provides a controlled subset of expression tree construction — callers get flexible, functional filtering without the full power (and risk) of raw `Expression<Func<T, bool>>`. Works with `IQueryable` (EF Core, LINQ to SQL, etc.) and in-memory `IEnumerable` via `.AsQueryable()`.

## Features

- **Type-safe property selection** — lambda expressions verified at compile time
- **Pre-built filter types** — `StringFilter`, `ComparableFilter<T>`, `GuidFilter`, `BoolFilter`, `EnumFilter<TEnum>`
- **Composable** — multiple `.Where()` calls AND together automatically
- **Nested collection filters** — filter parent entities by child collection predicates (one level deep)
- **Ordering** — `OrderBuilder<T>` with primary and secondary sort support
- **Expression mapping** — remap filters and orders from one type to another (e.g., model → entity)

## Why FilterBuilder Instead of Raw Expressions?

Raw `Expression<Func<T, bool>>` requires callers to write expressions against the types that the query provider consumes — typically internal entity types. This leaks implementation details and couples callers to the data layer. If the entity changes, every expression at every call site breaks.

FilterBuilder solves this by separating **what to filter** (model properties + operations) from **how to filter** (expression trees against entities). Callers build filters against public model types using a discoverable API. The library converts those filters into expressions internally via `ExpressionMapper`, targeting whatever entity type the data layer uses. Callers never see or depend on internal types.

```csharp
// Caller builds a filter against the public model — no knowledge of entities needed
var filter = Filter.For<UserModel>()
    .Where(u => u.Name, StringFilter.Contains("alice"))
    .Where(u => u.Age, ComparableFilter<int>.GreaterThan(18));

// Library internally converts to an entity expression via ExpressionMapper
var predicate = ExpressionMapper.MapPredicate<UserModel, UserEntity>(filter.Build()!);
```

**Key benefits:**

- **No internal type exposure** — callers filter against public models, not data layer entities
- **Discoverable API** — IntelliSense shows available properties (from the model) and available operations (from the filter type). No guessing what the query provider can or can't translate.
- **Deterministic operations** — the set of supported filter operations is fixed and known. Every operation is guaranteed to translate cleanly across layers. No runtime surprises from untranslatable expressions.
- **Consistent usage** — every consumer builds filters the same way, making the API predictable and the codebase uniform
- **Cross-layer portability** — filters are structured data (property + operation + value) that can be inspected, logged, remapped, and eventually serialized

## Filtering

### Basic Usage

Build a filter using `Filter.For<T>()` and chain `.Where()` calls. The compiler enforces which filter types apply to which property types.

```csharp
using Corely.Common.Filtering;
using Corely.Common.Filtering.Filters;

var filter = Filter.For<User>()
    .Where(u => u.Name, StringFilter.Contains("alice"))
    .Where(u => u.Age, ComparableFilter<int>.GreaterThan(18))
    .Where(u => u.IsActive, BoolFilter.IsTrue());

// Build the expression — returns null if no filters were added
Expression<Func<User, bool>>? predicate = filter.Build();

// Use with any IQueryable
if (predicate != null)
{
    var results = queryable.Where(predicate).ToList();
}
```

### Filter Types

| Filter Type | Applies To | Operations |
|---|---|---|
| `StringFilter` | `string`, `string?` | `Equals`, `NotEquals`, `Contains`, `NotContains`, `StartsWith`, `NotStartsWith`, `EndsWith`, `NotEndsWith`, `In`, `NotIn`, `IsNull`, `IsNotNull` |
| `ComparableFilter<T>` | `int`, `long`, `float`, `double`, `decimal`, `DateTime`, `DateTimeOffset` (+ nullable) | `Equals`, `NotEquals`, `GreaterThan`, `GreaterThanOrEqual`, `LessThan`, `LessThanOrEqual`, `Between`, `NotBetween`, `In`, `NotIn`, `IsNull`, `IsNotNull` |
| `GuidFilter` | `Guid`, `Guid?` | `Equals`, `NotEquals`, `In`, `NotIn`, `IsNull`, `IsNotNull` |
| `BoolFilter` | `bool`, `bool?` | `IsTrue`, `IsFalse`, `IsNull`, `IsNotNull` |
| `EnumFilter<TEnum>` | any `enum` (+ nullable) | `Equals`, `NotEquals`, `In`, `NotIn`, `IsNull`, `IsNotNull` |

Each operation is a static factory method that returns a filter object:

```csharp
StringFilter.Contains("eng")
ComparableFilter<int>.Between(10, 50)
GuidFilter.Equals(someGuid)
BoolFilter.IsTrue()
EnumFilter<Status>.In(Status.Active, Status.Pending)
```

### Multiple Filters (AND)

Multiple `.Where()` calls are combined with AND:

```csharp
var filter = Filter.For<Product>()
    .Where(p => p.Price, ComparableFilter<decimal>.GreaterThan(10.00m))
    .Where(p => p.Name, StringFilter.StartsWith("Widget"))
    .Where(p => p.IsAvailable, BoolFilter.IsTrue());

// Equivalent to: p => p.Price > 10.00m && p.Name.StartsWith("Widget") && p.IsAvailable == true
```

### Nullable Properties

The same filter types work with nullable properties. `IsNull` and `IsNotNull` are available on all filter types:

```csharp
var filter = Filter.For<Order>()
    .Where(o => o.ShippedDate, ComparableFilter<DateTime>.IsNull())    // nullable DateTime?
    .Where(o => o.ParentId, GuidFilter.IsNotNull());                   // nullable Guid?
```

### Nested Property Access

Property selectors support dotted paths into nested objects:

```csharp
// Filter by a property on a nested object
var filter = Filter.For<User>()
    .Where(u => u.Address.City, StringFilter.Contains("York"))
    .Where(u => u.Address.ZipCode, StringFilter.StartsWith("100"));

// Equivalent to: u => u.Address.City.Contains("York") && u.Address.ZipCode.StartsWith("100")
```

This works for any depth of navigation property access. The lambda expression is used as-is, so the resulting expression tree preserves the full member access chain.

### Nested Collection Filters

Filter parent entities based on child collection predicates. Translates to `.Any()` in LINQ:

```csharp
// "Groups containing a user whose username starts with 'j'"
var filter = Filter.For<Group>()
    .Where(g => g.Users, users => users
        .Where(u => u.Username, StringFilter.StartsWith("j")));

// Translates to: g => g.Users.Any(u => u.Username.StartsWith("j"))
```

Nested filters are capped at **one level deep** — the child `FilterBuilder` does not expose the collection `.Where()` overload. Attempting to nest deeper throws `InvalidOperationException`.

### Empty Filters

An empty filter (no `.Where()` calls) returns `null` from `.Build()`, meaning "no predicate — return all results":

```csharp
var filter = Filter.For<User>();
var predicate = filter.Build(); // null — no filtering applied
```

## Ordering

### Basic Usage

Build sort orders using `Order.For<T>()` with `.By()` (primary) and `.ThenBy()` (secondary):

```csharp
using Corely.Common.Filtering.Ordering;

var order = Order.For<User>()
    .By(u => u.LastName)
    .ThenBy(u => u.FirstName)
    .ThenBy(u => u.CreatedDate, SortDirection.Descending);

// Apply directly to an IQueryable
IOrderedQueryable<User> sorted = order.Apply(dbContext.Users);
```

### Sort Direction

`SortDirection.Ascending` is the default. Use `SortDirection.Descending` for reverse order:

```csharp
var order = Order.For<Product>()
    .By(p => p.Price, SortDirection.Descending);
```

### Rules

- `.By()` sets the primary sort and clears any previous sort
- `.ThenBy()` adds secondary/tertiary sorts (must be called after `.By()`)
- Calling `.By()` a second time resets the sort order
- `.ThenBy()` without a prior `.By()` throws `InvalidOperationException`
- `.Apply()` with no clauses throws `InvalidOperationException`

### Inspecting Clauses

Use `.Build()` to get the list of sort clauses without applying them:

```csharp
var order = Order.For<User>()
    .By(u => u.Name, SortDirection.Ascending)
    .ThenBy(u => u.Age, SortDirection.Descending);

IReadOnlyList<OrderClause<User>> clauses = order.Build();
// clauses[0]: PropertyExpression=u.Name, Direction=Ascending, IsPrimary=true
// clauses[1]: PropertyExpression=u.Age, Direction=Descending, IsPrimary=false
```

## Expression Mapping

`ExpressionMapper` remaps filter predicates and sort orders from one type to another. This is useful when filters are built against one type (e.g., a public model) but need to be applied to a different type with matching property names (e.g., a data layer entity).

### Map a Filter Predicate

```csharp
// Build a filter against a model type
var filter = Filter.For<UserModel>()
    .Where(u => u.Name, StringFilter.Contains("alice"));

var predicate = filter.Build()!;

// Remap to a different type (properties matched by name)
var entityPredicate = ExpressionMapper.MapPredicate<UserModel, UserEntity>(predicate);

// Use with any IQueryable
var results = queryable.Where(entityPredicate).ToList();
```

### Map Sort Orders

```csharp
var order = Order.For<UserModel>().By(u => u.Name);

// Apply remapped ordering to a different type's queryable
var sorted = ExpressionMapper.ApplyOrder<UserModel, UserEntity>(queryable, order);
```

If the target type is missing a property that exists on the source type, `MapPredicate` throws `InvalidOperationException`.
