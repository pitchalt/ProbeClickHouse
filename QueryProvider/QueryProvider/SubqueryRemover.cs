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

         TextWriter _logger;
        
        internal SubqueryRemover(TextWriter logger): base(logger) {
             _logger = logger;
        }
        public Expression Remove(SelectExpression outerSelect, params SelectExpression[] selectsToRemove)
        {
            return Remove(outerSelect, (IEnumerable<SelectExpression>)selectsToRemove);
        }

        public Expression Remove(SelectExpression outerSelect, IEnumerable<SelectExpression> selectsToRemove)
        {
            this.selectsToRemove = new HashSet<SelectExpression>(selectsToRemove);
            this.map = selectsToRemove.ToDictionary(d => d.Alias, d => d.Columns.ToDictionary(d2 => d2.Name, d2 => d2.Expression));
            return this.Visit(outerSelect);
        }

        protected override Expression VisitSelect(SelectExpression select,TextWriter logger)
        {
            if (this.selectsToRemove.Contains(select))
            {
                return this.Visit(select.From);
            }
            else
            {
                return base.VisitSelect(select, logger);
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