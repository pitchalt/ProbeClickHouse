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
        
internal class AggregateRewriter : DbExpressionVisitor {
        ILookup<string, AggregateSubqueryExpression> lookup;
        Dictionary<AggregateSubqueryExpression, Expression> map;
      

        private AggregateRewriter(Expression expr, TextWriter logger) : base (logger) {
            this.map = new Dictionary<AggregateSubqueryExpression, Expression>();
            this.lookup = AggregateGatherer.Gather(expr,logger).ToLookup(a => a.GroupByAlias);
           
        }


        internal static Expression Rewrite(Expression expr, TextWriter logger) {
            return new AggregateRewriter(expr,logger).Visit(expr);
        }

        protected override Expression VisitSelect(SelectExpression select) {
            select = (SelectExpression)base.VisitSelect(select);
            if (lookup.Contains(select.Alias)) {
                List<ColumnDeclaration> aggColumns = new List<ColumnDeclaration>(select.Columns);
                foreach (AggregateSubqueryExpression ae in lookup[select.Alias]) {
                    string name = "agg" + aggColumns.Count;
                    ColumnDeclaration cd = new ColumnDeclaration(name, ae.AggregateInGroupSelect);
                    this.map.Add(ae, new ColumnExpression(ae.Type, ae.GroupByAlias, name));
                    aggColumns.Add(cd);
                }
                return new SelectExpression(select.Type, select.Alias, aggColumns, select.From, select.Where, select.OrderBy, select.GroupBy);
            }
            return select;
        }

        protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate) {
            Expression mapped;
            if (this.map.TryGetValue(aggregate, out mapped)) {
                return mapped;
            }
            return this.Visit(aggregate.AggregateAsSubquery);
        }

        class AggregateGatherer : DbExpressionVisitor {
            List<AggregateSubqueryExpression> aggregates = new List<AggregateSubqueryExpression>();
           
            private AggregateGatherer(TextWriter logger) : base (logger)  {
              
            }

            internal static List<AggregateSubqueryExpression> Gather(Expression expression,TextWriter logger) {
                AggregateGatherer gatherer = new AggregateGatherer(logger);
                gatherer.Visit(expression);
                return gatherer.aggregates;
            }

            protected override Expression VisitAggregateSubquery(AggregateSubqueryExpression aggregate) {
                this.aggregates.Add(aggregate);
                return base.VisitAggregateSubquery(aggregate);
            }
        }
    }


       
    
}