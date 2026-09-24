using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using BillsSystem.Domain.Common;

namespace BillsSystem.Domain.Specifications
{
    public abstract class BaseSpecification<T> : ISpecification<T> where T : BaseEntity
    {
        #region Constractors
        protected BaseSpecification() { }
        protected BaseSpecification(Expression<Func<T, bool>> criteria) => Criteria = criteria;
        #endregion

        #region Properties
        public Expression<Func<T, bool>>? Criteria { get; private set; }

        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();

        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }

        public int Skip { get; private set; }
        public int Take { get; private set; }
        public bool IsPagingEnabled { get; private set; }
        #endregion

        #region Methods
        protected void AddCriteria(Expression<Func<T, bool>> criteria) => Criteria = criteria;
        protected void AddInclude(Expression<Func<T, object>> includeExpression) => Includes.Add(includeExpression);
        protected void AddInclude(string includeString) => IncludeStrings.Add(includeString);
        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;
        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression) => OrderByDescending = orderByDescExpression;

        protected void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        } 
        #endregion
    }
}
