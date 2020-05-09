using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QueryProviderTest {
    internal class SubqueryRemover : DbExpressionVisitor
    {
        HashSet<SelectExpression> selectsToRemove;
        Dictionary<string, Dictionary<string, Expression>> map;
    
        private SubqueryRemover(IEnumerable<SelectExpression> selectsToRemove,TextWriter logger): base(logger) 
        {
            this.selectsToRemove = new HashSet<SelectExpression>(selectsToRemove);
            this.map = this.selectsToRemove.ToDictionary(d => d.Alias, d => d.Columns.ToDictionary(d2 => d2.Name, d2 => d2.Expression));
        }

        internal static SelectExpression Remove(SelectExpression outerSelect,TextWriter logger, params SelectExpression[] selectsToRemove)
        {
            return Remove(outerSelect, (IEnumerable<SelectExpression>)selectsToRemove,logger);
        }

        internal static SelectExpression Remove(SelectExpression outerSelect, IEnumerable<SelectExpression> selectsToRemove,TextWriter logger)
        {
            return (SelectExpression)new SubqueryRemover(selectsToRemove,logger).Visit(outerSelect);
        }

        internal static ProjectionExpression Remove(ProjectionExpression projection,TextWriter logger, params SelectExpression[] selectsToRemove)
        {
            return Remove(projection, (IEnumerable<SelectExpression>)selectsToRemove,logger);
        }

        internal static ProjectionExpression Remove(ProjectionExpression projection, IEnumerable<SelectExpression> selectsToRemove,TextWriter logger)
        {
            return (ProjectionExpression)new SubqueryRemover(selectsToRemove,logger).Visit(projection);
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            if (this.selectsToRemove.Contains(select))
            {
                return this.Visit(select.From);
            }
            else
            {
                return base.VisitSelect(select);
            }
        }

        protected override Expression VisitColumn(ColumnExpression column)
        {
            Dictionary<string, Expression> nameMap;
            if (this.map.TryGetValue(column.Alias, out nameMap))
            {
                Expression expr;
                if (nameMap.TryGetValue(column.Name, out expr))
                {
                    return this.Visit(expr);
                }
                throw new Exception("Reference to undefined column");
            }
            return column;
        }
    }
}