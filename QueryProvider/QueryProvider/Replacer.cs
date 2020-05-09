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

        private Replacer(Expression searchFor, Expression replaceWith,TextWriter logger) : base(logger){
            this.searchFor = searchFor;
            this.replaceWith = replaceWith;
        }
         internal static Expression Replace(Expression expression, Expression searchFor, Expression replaceWith,TextWriter logger) {
            return new Replacer(searchFor, replaceWith,logger).Visit(expression);
        }
        protected override Expression Visit(Expression exp) {
            if (exp == this.searchFor) {
                return this.replaceWith;
            }
            return base.Visit(exp);
        }
    }
}
