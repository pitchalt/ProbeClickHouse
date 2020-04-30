using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;
using System.Linq.Expressions;

namespace QueryProviderTest
{
    internal class ProjectionBuilder : DbExpressionVisitor {
        ParameterExpression row;
        string rowAlias;
        static MethodInfo miGetValue;
        static MethodInfo miExecuteSubQuery;
        
        internal ProjectionBuilder() {
            if (miGetValue == null) {
                miGetValue = typeof(ProjectionRow).GetMethod("GetValue");
                miExecuteSubQuery = typeof(ProjectionRow).GetMethod("ExecuteSubQuery");
            }
        }

        internal LambdaExpression Build(Expression expression, string alias) {
            this.row = Expression.Parameter(typeof(ProjectionRow), "row");
            this.rowAlias = alias;
            Expression body = this.Visit(expression);
            return Expression.Lambda(body, this.row);
        }

        protected override Expression VisitColumn(ColumnExpression column) {
            if (column.Alias == this.rowAlias) {
                return Expression.Convert(Expression.Call(this.row, miGetValue, Expression.Constant(column.Ordinal)), column.Type);
            }
            return column;
        }

        protected override Expression VisitProjection(ProjectionExpression proj) {
            LambdaExpression subQuery = Expression.Lambda(base.VisitProjection(proj), this.row);
            Type elementType = TypeSystem.GetElementType(subQuery.Body.Type);
            MethodInfo mi = miExecuteSubQuery.MakeGenericMethod(elementType);
            return Expression.Convert(
                Expression.Call(this.row, mi, Expression.Constant(subQuery)),
                proj.Type
                );
        }
    }
}