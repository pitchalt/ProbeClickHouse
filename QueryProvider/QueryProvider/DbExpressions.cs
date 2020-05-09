using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Authentication.ExtendedProtection;
using System.Text;

namespace QueryProviderTest {

    internal enum DbExpressionType {
       Table = 1000, // make sure these don't overlap with ExpressionType
        Column,
        Select,
        Projection,
        Join,
        Aggregate,
        Subquery,
        Grouping,
        AggregateSubquery,
        IsNull
    }

    internal static class DbExpressionExtensions {
        internal static bool IsDbExpression(this ExpressionType et) {
            return ((int)et) >= 1000;
        }
    }

    internal class TableExpression : Expression {
        string alias;
        string name;
        internal TableExpression(Type type, string alias, string name)
            : base((ExpressionType)DbExpressionType.Table, type) {
            this.alias = alias;
            this.name = name;
        }
        internal string Alias {
            get { return this.alias; }
        }
        internal string Name {
            get { return this.name; }
        }
    }

   internal class ColumnExpression : Expression {
        string alias;
        string name;
        internal ColumnExpression(Type type, string alias, string name)
            : base((ExpressionType)DbExpressionType.Column, type) {
            this.alias = alias;
            this.name = name;
        }
        internal string Alias {
            get { return this.alias; }
        }
        internal string Name {
            get { return this.name; }
        }
    }

    internal class ColumnDeclaration {
        string name;
        Expression expression;
        internal ColumnDeclaration(string name, Expression expression) {
            this.name = name;
            this.expression = expression;
        }
        internal string Name {
            get { return this.name; }
        }
        internal Expression Expression {
            get { return this.expression; }
        }
    }

    
    internal class SelectExpression : Expression {
        string alias;
        ReadOnlyCollection<ColumnDeclaration> columns;
        Expression from;
        Expression where;
        ReadOnlyCollection<OrderExpression> orderBy;
        ReadOnlyCollection<Expression> groupBy;

        internal SelectExpression(
            Type type, 
            string alias, 
            IEnumerable<ColumnDeclaration> columns, 
            Expression from, 
            Expression where, 
            IEnumerable<OrderExpression> orderBy,
            IEnumerable<Expression> groupBy
            )
            : base((ExpressionType)DbExpressionType.Select, type) {
            this.alias = alias;
            this.columns = columns as ReadOnlyCollection<ColumnDeclaration>;
            if (this.columns == null) {
                this.columns = new List<ColumnDeclaration>(columns).AsReadOnly();
            }
            this.from = from;
            this.where = where;
            this.orderBy = orderBy as ReadOnlyCollection<OrderExpression>;
            if (this.orderBy == null && orderBy != null) {
                this.orderBy = new List<OrderExpression>(orderBy).AsReadOnly();
            }
            this.groupBy = groupBy as ReadOnlyCollection<Expression>;
            if (this.groupBy == null && groupBy != null) {
                this.groupBy = new List<Expression>(groupBy).AsReadOnly();
            }
        }
        internal SelectExpression(
            Type type, string alias, IEnumerable<ColumnDeclaration> columns, 
            Expression from, Expression where
            )
            : this(type, alias, columns, from, where, null, null) {
        }
        internal string Alias {
            get { return this.alias; }
        }
        internal ReadOnlyCollection<ColumnDeclaration> Columns {
            get { return this.columns; }
        }
        internal Expression From {
            get { return this.from; }
        }
        internal Expression Where {
            get { return this.where; }
        }
        internal ReadOnlyCollection<OrderExpression> OrderBy {
            get { return this.orderBy; }
        }
        internal ReadOnlyCollection<Expression> GroupBy {
            get { return this.groupBy; }
        }
    }

   internal class ProjectionExpression : Expression {
        SelectExpression source;
        Expression projector;
        LambdaExpression aggregator;
        internal ProjectionExpression(SelectExpression source, Expression projector)
            : this (source, projector, null) {
        }
        internal ProjectionExpression(SelectExpression source, Expression projector, LambdaExpression aggregator)
            : base((ExpressionType)DbExpressionType.Projection, source.Type) {
            this.source = source;
            this.projector = projector;
            this.aggregator = aggregator;
        }
        internal SelectExpression Source {
            get { return this.source; }
        }
        internal Expression Projector {
            get { return this.projector; }
        }
        internal LambdaExpression Aggregator {
            get { return this.aggregator; }
        }
    }

    internal class DbExpressionVisitor : ExpressionVisitor {
        
        protected DbExpressionVisitor(TextWriter logger) : base(logger) {
            
         }

        protected override Expression Visit(Expression exp) {
            if (exp == null) {
                return null;
            }
            switch ((DbExpressionType)exp.NodeType) {
                case DbExpressionType.Table:
                    return this.VisitTable((TableExpression)exp);
                case DbExpressionType.Column:
                    return this.VisitColumn((ColumnExpression)exp);
                case DbExpressionType.Select:
                    return this.VisitSelect((SelectExpression)exp);
                case DbExpressionType.Join:
                    return this.VisitJoin((JoinExpression)exp);
                case DbExpressionType.Aggregate:
                    return this.VisitAggregate((AggregateExpression)exp);
                case DbExpressionType.Subquery:
                    return this.VisitSubquery((SubqueryExpression)exp);
                case DbExpressionType.AggregateSubquery:
                    return this.VisitAggregateSubquery((AggregateSubqueryExpression)exp);
                case DbExpressionType.IsNull:
                    return this.VisitIsNull((IsNullExpression)exp);
                case DbExpressionType.Projection:
                    return this.VisitProjection((ProjectionExpression)exp);
                default:
                    return base.Visit(exp);
            }
        }
        protected virtual Expression VisitTable(TableExpression table) {
            return table;
        }
        protected virtual Expression VisitColumn(ColumnExpression column) {
            return column;
        }
       protected virtual Expression VisitSelect(SelectExpression select) {
            Expression from = this.VisitSource(select.From);
            Expression where = this.Visit(select.Where);
            ReadOnlyCollection<ColumnDeclaration> columns = this.VisitColumnDeclarations(select.Columns);
            ReadOnlyCollection<OrderExpression> orderBy = this.VisitOrderBy(select.OrderBy);
               ReadOnlyCollection<Expression> groupBy = this.VisitExpressionList(select.GroupBy);

    if (from != select.From

   || where != select.Where

        || columns != select.Columns

        || orderBy != select.OrderBy

        || groupBy != select.GroupBy

        ) {

        return new SelectExpression(select.Type, select.Alias, columns, from, where, orderBy, groupBy);

    }

    return select;
        }

 protected virtual Expression VisitAggregate(AggregateExpression aggregate) {
            Expression arg = this.Visit(aggregate.Argument);
            if (arg != aggregate.Argument) {
                return new AggregateExpression(aggregate.Type, aggregate.AggregateType, arg);
            }
            return aggregate;
        }
        protected virtual Expression VisitIsNull(IsNullExpression isnull) {
            Expression expr = this.Visit(isnull.Expression);
            if (expr != isnull.Expression) {
                return new IsNullExpression(expr);
            }
            return isnull;
        }
        protected virtual Expression VisitSubquery(SubqueryExpression subquery) {
            SelectExpression select = (SelectExpression)this.Visit(subquery.Select);
            if (select != subquery.Select) {
                return new SubqueryExpression(subquery.Type, select);
            }
            return subquery;
        }
        protected virtual Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate) {
            Expression e = this.Visit(aggregate.AggregateAsSubquery);
            System.Diagnostics.Debug.Assert(e is SubqueryExpression);
            SubqueryExpression subquery = (SubqueryExpression)e;
            if (subquery != aggregate.AggregateAsSubquery) {
                return new AggregateSubqueryExpression(aggregate.GroupByAlias, aggregate.AggregateInGroupSelect, subquery);
            }
            return aggregate;
        }
        protected ReadOnlyCollection<OrderExpression> VisitOrderBy(ReadOnlyCollection<OrderExpression> expressions) {
            if (expressions != null) {
                List<OrderExpression> alternate = null;
                for (int i = 0, n = expressions.Count; i < n; i++) {
                    OrderExpression expr = expressions[i];
                    Expression e = this.Visit(expr.Expression);
                    if (alternate == null && e != expr.Expression) {
                        alternate = expressions.Take(i).ToList();
                    }
                    if (alternate != null) {
                        alternate.Add(new OrderExpression(expr.OrderType, e));
                    }
                }
                if (alternate != null) {
                    return alternate.AsReadOnly();
                }
            }
            return expressions;
        }
        protected virtual Expression VisitJoin(JoinExpression join) {
            Expression left = this.Visit(join.Left);
            Expression right = this.Visit(join.Right);
            Expression condition = this.Visit(join.Condition);
            if (left != join.Left || right != join.Right || condition != join.Condition) {
                return new JoinExpression(join.Type, join.Join, left, right, condition);
            }
            return join;
        }
        protected virtual Expression VisitSource(Expression source) {
            return this.Visit(source);
        }
        protected virtual Expression VisitProjection(ProjectionExpression proj) {
            SelectExpression source = (SelectExpression)this.Visit(proj.Source);
            Expression projector = this.Visit(proj.Projector);
            if (source != proj.Source || projector != proj.Projector) {
                return new ProjectionExpression(source, projector);
            }
            return proj;
        }
        protected ReadOnlyCollection<ColumnDeclaration> VisitColumnDeclarations(ReadOnlyCollection<ColumnDeclaration> columns) {
            List<ColumnDeclaration> alternate = null;
            for (int i = 0, n = columns.Count; i < n; i++) {
                ColumnDeclaration column = columns[i];
                Expression e = this.Visit(column.Expression);
                if (alternate == null && e != column.Expression) {
                    alternate = columns.Take(i).ToList();
                }
                if (alternate != null) {
                    alternate.Add(new ColumnDeclaration(column.Name, e));
                }
            }
            if (alternate != null) {
                return alternate.AsReadOnly();
            }
            return columns;
        }
    }

    internal class IsNullExpression : Expression {
        Expression expression;
        internal IsNullExpression(Expression expression) 
            : base((ExpressionType)DbExpressionType.IsNull, typeof(bool)) {
            this.expression = expression;
        }
        internal Expression Expression {
            get { return this.expression; }
        }
    }
    internal enum AggregateType {

        Count,

        Min,

        Max,

        Sum,

        Average

    }

    internal class AggregateExpression : Expression {

        AggregateType aggType;

        Expression argument;

        internal AggregateExpression(Type type, AggregateType aggType, Expression argument)

            : base((ExpressionType)DbExpressionType.Aggregate, type) {

  this.aggType = aggType;

            this.argument = argument;

        }

        internal AggregateType AggregateType {

            get { return this.aggType; }

        }

        internal Expression Argument {

            get { return this.argument; }

        }

    }


     internal class SubqueryExpression : Expression {

        SelectExpression select;

        internal SubqueryExpression(Type type, SelectExpression select)

            : base((ExpressionType)DbExpressionType.Subquery, type) {

            this.select = select;

        }

        internal SelectExpression Select {

            get { return this.select; }

        }

    }

    internal class AggregateSubqueryExpression : Expression {

        string groupByAlias;

        Expression aggregateInGroupSelect;

        SubqueryExpression aggregateAsSubquery;

        internal AggregateSubqueryExpression(string groupByAlias, Expression aggregateInGroupSelect, SubqueryExpression aggregateAsSubquery)

            : base((ExpressionType)DbExpressionType.AggregateSubquery, aggregateAsSubquery.Type)

        {

            this.aggregateInGroupSelect = aggregateInGroupSelect;

            this.groupByAlias = groupByAlias;

            this.aggregateAsSubquery = aggregateAsSubquery;

        }

        internal string GroupByAlias { get { return this.groupByAlias; } }

        internal Expression AggregateInGroupSelect { get { return this.aggregateInGroupSelect; } }

        internal SubqueryExpression AggregateAsSubquery { get { return this.aggregateAsSubquery; } }

    }
}
