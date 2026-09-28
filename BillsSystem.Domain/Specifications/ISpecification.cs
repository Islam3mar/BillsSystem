using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using BillsSystem.Domain.Common;

namespace BillsSystem.Domain.Specifications
{
    // القاعدة: أي Query بترجع List أو Paged Result بيتعمله Specification.
    public interface ISpecification<T> where T : BaseEntity
    {
        Expression<Func<T, bool>>? Criteria { get; }

        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }

        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }
        Expression<Func<T, object>>? ThenByDescending { get; }

        int Skip { get; }
        int Take { get; }
        bool IsPagingEnabled { get; }
    }
}
