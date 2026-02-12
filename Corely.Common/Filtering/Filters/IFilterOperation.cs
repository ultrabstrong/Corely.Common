using System.Linq.Expressions;

namespace Corely.Common.Filtering.Filters;

public interface IFilterOperation
{
    Expression BuildExpression(Expression property);
}
