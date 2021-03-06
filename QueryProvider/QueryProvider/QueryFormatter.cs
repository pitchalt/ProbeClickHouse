﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace QueryProviderTest {
    internal class QueryFormatter : DbExpressionVisitor {
        StringBuilder sb;
        int indent = 2;
        int depth;
   
        private QueryFormatter(TextWriter logger): base(logger) {
          this.sb = new StringBuilder();
        }

        internal static string Format(Expression expression,TextWriter logger) {
            QueryFormatter formatter = new QueryFormatter(logger);
            formatter.Visit(expression);
            return formatter.sb.ToString();
        }

        protected enum Indentation {
            Same,
            Inner,
            Outer
        }

        internal int IndentationWidth {
            get { return this.indent; }
            set { this.indent = value; }
        }

        private void AppendNewLine(Indentation style) {
           sb.AppendLine();
            this.Indent(style);
            for (int i = 0, n = this.depth * this.indent; i < n; i++) {
                sb.Append(" ");
            }
        }
        private void Indent(Indentation style) {
            if (style == Indentation.Inner) {
                this.depth++;
            }
            else if (style == Indentation.Outer) {
                this.depth--;
                System.Diagnostics.Debug.Assert(this.depth >= 0);
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression m) {
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u) {
            switch (u.NodeType) {
                case ExpressionType.Not:
                    sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b) {
            sb.Append("(");
            this.Visit(b.Left);
            switch (b.NodeType) {
                case ExpressionType.And:
                    sb.Append(" AND ");
                    break;
                case ExpressionType.Or:
                    sb.Append(" OR ");
                    break;
                case ExpressionType.Equal:
                    sb.Append(" = ");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(" <> ");
                    break;
                case ExpressionType.LessThan:
                    sb.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(" >= ");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }
            this.Visit(b.Right);
            sb.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c) {
            if (c.Value == null) {
                sb.Append("NULL");
            }
            else {
                switch (Type.GetTypeCode(c.Value.GetType())) {
                    case TypeCode.Boolean:
                        sb.Append(((bool)c.Value) ? 1 : 0);
                        break;
                    case TypeCode.String:
                        sb.Append("'");
                        sb.Append(c.Value);
                        sb.Append("'");
                        break;
                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));
                    default:
                        sb.Append(c.Value);
                        break;
                }
            }
            return c;
        }

        protected override Expression VisitColumn(ColumnExpression column) {
            if (!string.IsNullOrEmpty(column.Alias)) {
                sb.Append(column.Alias);
                sb.Append(".");
            }
            sb.Append(column.Name);
            return column;
        }

        protected override Expression VisitSelect(SelectExpression select) {
            sb.Append("SELECT ");
            for (int i = 0, n = select.Columns.Count; i < n; i++) {
                ColumnDeclaration column = select.Columns[i];
                if (i > 0) {
                    sb.Append(", ");
                }
                ColumnExpression c = this.Visit(column.Expression) as ColumnExpression;
                if (!string.IsNullOrEmpty(column.Name) && (c == null || c.Name != column.Name)) {
                    sb.Append(" AS ");
                    sb.Append(column.Name);
                }
            }
            if (select.From != null) {
                this.AppendNewLine(Indentation.Same);
                sb.Append("FROM ");
                this.VisitSource(select.From);
            }
            if (select.Where != null) {
                this.AppendNewLine(Indentation.Same);
                sb.Append("WHERE ");
                this.Visit(select.Where);
            }
            if (select.OrderBy != null && select.OrderBy.Count > 0) {
                this.AppendNewLine(Indentation.Same);
                sb.Append("ORDER BY ");
                for (int i = 0, n = select.OrderBy.Count; i < n; i++) {
                    OrderExpression exp = select.OrderBy[i];
                    if (i > 0) {
                        sb.Append(", ");
                    }
                    this.Visit(exp.Expression);
                    if (exp.OrderType != OrderType.Ascending) {
                        sb.Append(" DESC");
                    }
                }
            }
            if (select.GroupBy != null && select.GroupBy.Count > 0) {
                this.AppendNewLine(Indentation.Same);
                sb.Append("GROUP BY ");
                for (int i = 0, n = select.GroupBy.Count; i < n; i++) {
                    if (i > 0) {
                        sb.Append(", ");
                    }
                    this.Visit(select.GroupBy[i]);
                }
            }
            return select;
        }

        protected override Expression VisitSource(Expression source) {
           switch ((DbExpressionType)source.NodeType) {
                case DbExpressionType.Table:
                    TableExpression table = (TableExpression)source;
                    sb.Append(table.Name);
                    sb.Append(" AS ");
                    sb.Append(table.Alias);
                    break;
                case DbExpressionType.Select:
                    SelectExpression select = (SelectExpression)source;
                    sb.Append("(");
                    this.AppendNewLine(Indentation.Inner);
                    this.Visit(select);
                    this.AppendNewLine(Indentation.Same);
                    sb.Append(")");
                    sb.Append(" AS ");
                    sb.Append(select.Alias);
                    this.Indent(Indentation.Outer);
                    break;
                case DbExpressionType.Join:
                    this.VisitJoin((JoinExpression)source);
                    break;
                default:
                    throw new InvalidOperationException("Select source is not valid type");
            }
            return source;
        }

        protected override Expression VisitJoin(JoinExpression join) {
            this.VisitSource(join.Left);
            this.AppendNewLine(Indentation.Same);
            switch (join.Join) {
                case JoinType.CrossJoin:
                    sb.Append("CROSS JOIN ");
                    break;
                case JoinType.InnerJoin:
                    sb.Append("INNER JOIN ");
                    break;
                case JoinType.CrossApply:
                    sb.Append("CROSS APPLY ");
                    break;
            }
            this.VisitSource(join.Right);
            if (join.Condition != null) {
                this.AppendNewLine(Indentation.Inner);
                sb.Append("ON ");
                this.Visit(join.Condition);
                this.AppendNewLine(Indentation.Outer);
            }
            return join;
        }

         private string GetAggregateName(AggregateType aggregateType) {
            switch (aggregateType) {
                case AggregateType.Count: return "COUNT";
                case AggregateType.Min: return "MIN";
                case AggregateType.Max: return "MAX";
                case AggregateType.Sum: return "SUM";
                case AggregateType.Average: return "AVG";
                default: throw new Exception(string.Format("Unknown aggregate type: {0}", aggregateType));
            }
        }

        private bool RequiresAsteriskWhenNoArgument(AggregateType aggregateType) {
            return aggregateType == AggregateType.Count;
        }

        protected override Expression VisitAggregate(AggregateExpression aggregate) {
            sb.Append(GetAggregateName(aggregate.AggregateType));
            sb.Append("(");
            if (aggregate.Argument != null) {
                this.Visit(aggregate.Argument);
            }
            else if (RequiresAsteriskWhenNoArgument(aggregate.AggregateType)) {
                sb.Append("*");
            }
            sb.Append(")");
            return aggregate;
        }

        protected override Expression VisitIsNull(IsNullExpression isnull) {
            this.Visit(isnull.Expression);
            sb.Append(" IS NULL");
            return isnull;
        }

        protected override Expression VisitSubquery(SubqueryExpression subquery) {
            sb.Append("(");
            this.AppendNewLine(Indentation.Inner);
            this.Visit(subquery.Select);
            this.AppendNewLine(Indentation.Same);
            sb.Append(")");
            this.Indent(Indentation.Outer);
            return subquery;
        }
    }
}
