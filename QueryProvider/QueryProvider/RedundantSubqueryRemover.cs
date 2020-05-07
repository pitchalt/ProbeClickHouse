using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QueryProviderTest {
    internal class RedundantSubqueryRemover : DbExpressionVisitor
    {
        TextWriter _logger;
        internal RedundantSubqueryRemover(TextWriter logger): base(logger) {
            _logger = logger;
        }

        internal Expression Remove(Expression expression)
        {
            return this.Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select,TextWriter logger)
        {
            select = (SelectExpression)base.VisitSelect(select,logger);

            // first remove all purely redundant subqueries
            List<SelectExpression> redundant = new RedundantSubqueryGatherer(logger).Gather(select.From);
            if (redundant != null)
            {
                select = (SelectExpression)new SubqueryRemover(logger).Remove(select, redundant);
            }

            // next attempt to merge subqueries

            // can only merge if subquery is a single select (not a join)
            SelectExpression fromSelect = select.From as SelectExpression;
            if (fromSelect != null)
            {
                // can only merge if subquery has simple-projection (no renames or complex expressions)
                if (HasSimpleProjection(fromSelect))
                {
                    // remove the redundant subquery
                    select = (SelectExpression)new SubqueryRemover(logger).Remove(select, fromSelect);
                    // merge where expressions 
                    Expression where = select.Where;
                    if (fromSelect.Where != null)
                    {
                        if (where != null)
                        {
                            where = Expression.And(fromSelect.Where, where);
                        }
                        else
                        {
                            where = fromSelect.Where;
                        }
                    }
                    if (where != select.Where)
                    {
                        return new SelectExpression(select.Type, select.Alias, select.Columns, select.From, where, select.OrderBy);
                    }
                }
            }

            return select;
        }

        private static bool IsRedudantSubquery(SelectExpression select)
        {
            return HasSimpleProjection(select)
                && select.Where == null
                && (select.OrderBy == null || select.OrderBy.Count == 0);
        }

        private static bool HasSimpleProjection(SelectExpression select)
        {
            foreach (ColumnDeclaration decl in select.Columns)
            {
                ColumnExpression col = decl.Expression as ColumnExpression;
                if (col == null || decl.Name != col.Name)
                {
                    // column name changed or column expression is more complex than reference to another column
                    return false;
                }
            }
            return true;
        }

        class RedundantSubqueryGatherer : DbExpressionVisitor
        {
             TextWriter _logger;
        internal RedundantSubqueryGatherer(TextWriter logger): base(logger) {
            _logger = logger;
        }

            List<SelectExpression> redundant;

            internal List<SelectExpression> Gather(Expression source)
            {
                this.Visit(source);
                return this.redundant;
            }

            protected override Expression VisitSelect(SelectExpression select,TextWriter logger)
            {
                if (IsRedudantSubquery(select))
                {
                    if (this.redundant == null)
                    {
                        this.redundant = new List<SelectExpression>();
                    }
                    this.redundant.Add(select);
                }
                return select;
            }
        }
    }
}