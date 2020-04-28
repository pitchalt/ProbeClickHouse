using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryProviderTest
{
    public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable {
        readonly QueryProvider provider;
        readonly Expression expression;
        public Query(QueryProvider provider) {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.expression = Expression.Constant(this);
        }
        public Query(QueryProvider provider, Expression expression) {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type)) {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
            this.expression = expression;
        }
        Expression IQueryable.Expression {
            get { return this.expression; }
        }
        Type IQueryable.ElementType {
            get { return typeof(T); }
        }
        IQueryProvider IQueryable.Provider {
            get { return this.provider; }
        }
        public IEnumerator<T> GetEnumerator() {
            return ((IEnumerable<T>)this.provider.Execute(this.expression)).GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable)this.provider.Execute(this.expression)).GetEnumerator();
        }
        public override string ToString() {
            return this.provider.GetQueryText(this.expression);
        }
    }
}