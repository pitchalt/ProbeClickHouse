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

    private static MethodInfo miGetValue;

    internal ProjectionBuilder() {

        if (miGetValue == null) {

            miGetValue = typeof(ProjectionRow).GetMethod("GetValue");

        }

    }

    internal LambdaExpression Build(Expression expression) {

        this.row = Expression.Parameter(typeof(ProjectionRow), "row");

        Expression body = this.Visit(expression);

        return Expression.Lambda(body, this.row);

    }

    protected override Expression VisitColumn(ColumnExpression column) {

        return Expression.Convert(Expression.Call(this.row, miGetValue, Expression.Constant(column.Ordinal)), column.Type);

    }

}
}