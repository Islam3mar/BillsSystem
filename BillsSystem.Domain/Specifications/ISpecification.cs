using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using BillsSystem.Domain.Common;

namespace BillsSystem.Domain.Specifications
{
    // العقد العام لأي Specification: بيوصف "عايزين إيه" (شرط + Includes + ترتيب + صفحات)
    // القاعدة: أي Query بترجع List أو Paged Result بيتعمله Specification.
    // أي Query بترجع bool (Exists) أو Entity واحد بشرط بسيط، بيفضل LINQ مباشر جوه الـ Repository.
    public interface ISpecification<T> where T : BaseEntity
    {
        Expression<Func<T, bool>>? Criteria { get; }

        List<Expression<Func<T, object>>> Includes { get; }
        List<string> IncludeStrings { get; }

        Expression<Func<T, object>>? OrderBy { get; }
        Expression<Func<T, object>>? OrderByDescending { get; }

        int Skip { get; }
        int Take { get; }
        bool IsPagingEnabled { get; }
    }
}
