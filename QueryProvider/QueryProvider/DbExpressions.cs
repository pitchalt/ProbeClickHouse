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
        Join
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
        int ordinal;
        internal ColumnExpression(Type type, string alias, string name, int ordinal)
            : base((ExpressionType)DbExpressionType.Column, type) {
            this.alias = alias;
            this.name = name;
            this.ordinal = ordinal;
        }
        internal string Alias {
            get { return this.alias; }
        }
        internal string Name {
            get { return this.name; }
        }
        internal int Ordinal {
            get { return this.ordinal; }
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
         internal SelectExpression(
            Type type, string alias, IEnumerable<ColumnDeclaration> columns, 
            Expression from, Expression where, IEnumerable<OrderExpression> orderBy)
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
        }
        internal SelectExpression(
            Type type, string alias, IEnumerable<ColumnDeclaration> columns, 
            Expression from, Expression where)
            : this(type, alias, columns, from, where, null) {
        }
         internal ReadOnlyCollection<OrderExpression> OrderBy {
            get { return this.orderBy; }
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
    }

    internal class ProjectionExpression : Expression {
        SelectExpression source;
        Expression projector;
        internal ProjectionExpression(SelectExpression source, Expression projector)
            : base((ExpressionType)DbExpressionType.Projection, source.Type) {
            this.source = source;
            this.projector = projector;
        }
        internal SelectExpression Source {
            get { return this.source; }
        }
        internal Expression Projector {
            get { return this.projector; }
        }
    }

    internal class DbExpressionVisitor : ExpressionVisitor {
        protected DbExpressionVisitor(TextWriter logger) : base(logger) { }

        protected override Expression Visit(Expression exp) {
            if (exp == null) {
                return null;
            }
            VisitLog("DbExp", exp);
            var nodeType = (DbExpressionType) exp.NodeType;
            switch (nodeType) {
                case DbExpressionType.Table:
                    return this.VisitTable((TableExpression)exp);
                case DbExpressionType.Column:
                    return this.VisitColumn((ColumnExpression)exp);
                case DbExpressionType.Select:
                    return this.VisitSelect((SelectExpression)exp);
                case DbExpressionType.Join:
                    return this.VisitJoin((JoinExpression)exp);
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
            if (from != select.From || where != select.Where || columns != select.Columns || orderBy != select.OrderBy) {
                return new SelectExpression(select.Type, select.Alias, columns, from, where, orderBy);
            }
            return select;
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
}
