using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QueryProviderTest {
    internal class Replacer : DbExpressionVisitor {
        Expression searchFor;
        Expression replaceWith;

        internal Replacer(TextWriter logger) : base(logger) { }

        internal Expression Replace(Expression expression, Expression searchFor, Expression replaceWith) {
            this.searchFor = searchFor;
            this.replaceWith = replaceWith;
            return this.Visit(expression);
        }
        protected override Expression Visit(Expression exp) {
            if (exp == this.searchFor) {
                return this.replaceWith;
            }
            return base.Visit(exp);
        }
    }
}
