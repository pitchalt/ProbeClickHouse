using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.IO;
using System.Linq;



namespace QueryProviderTest {

     internal class OrderByRewriter : DbExpressionVisitor {
        IEnumerable<OrderExpression> gatheredOrderings;
        bool isOuterMostSelect;
        
        private OrderByRewriter(TextWriter logger) : base (logger){
             this.isOuterMostSelect = true;
        }

        internal static Expression Rewrite(Expression expression,TextWriter logger) {
            return new OrderByRewriter(logger).Visit(expression);
        }

        protected override Expression VisitSelect(SelectExpression select) {
            bool saveIsOuterMostSelect = this.isOuterMostSelect;
            try {
                this.isOuterMostSelect = false;
                select = (SelectExpression)base.VisitSelect(select);
                bool hasOrderBy = select.OrderBy != null && select.OrderBy.Count > 0;
                if (hasOrderBy) {
                    this.PrependOrderings(select.OrderBy);
                }
                bool canHaveOrderBy = saveIsOuterMostSelect;
                bool canPassOnOrderings = !saveIsOuterMostSelect && (select.GroupBy == null || select.GroupBy.Count == 0);
                IEnumerable<OrderExpression> orderings = (canHaveOrderBy) ? this.gatheredOrderings : null;
                ReadOnlyCollection<ColumnDeclaration> columns = select.Columns;
                if (this.gatheredOrderings != null) {
                    if (canPassOnOrderings) {
                        HashSet<string> producedAliases = AliasesProduced.Gather(select.From,Logger);
                        // reproject order expressions using this select's alias so the outer select will have properly formed expressions
                        BindResult project = this.RebindOrderings(this.gatheredOrderings, select.Alias, producedAliases, select.Columns);
                        this.gatheredOrderings = project.Orderings;
                        columns = project.Columns;
                    }
                    else {
                        this.gatheredOrderings = null;
                    }
                }
                if (orderings != select.OrderBy || columns != select.Columns) {
                    select = new SelectExpression(select.Type, select.Alias, columns, select.From, select.Where, orderings, select.GroupBy);
                }
                return select;
            }
            finally {
                this.isOuterMostSelect = saveIsOuterMostSelect;
            }
        }
         protected override Expression VisitSubquery(SubqueryExpression subquery) {
            var saveOrderings = this.gatheredOrderings;
            this.gatheredOrderings = null;
            var result = base.VisitSubquery(subquery);
            this.gatheredOrderings = saveOrderings;
            return result;
        }
        protected override Expression VisitJoin(JoinExpression join) {
           Expression left = this.VisitSource(join.Left);
            IEnumerable<OrderExpression> leftOrders = this.gatheredOrderings;
            this.gatheredOrderings = null; // start on the right with a clean slate
            Expression right = this.VisitSource(join.Right);
            this.PrependOrderings(leftOrders);
            Expression condition = this.Visit(join.Condition);
            if (left != join.Left || right != join.Right || condition != join.Condition) {
                return new JoinExpression(join.Type, join.Join, left, right, condition);
            }
            return join;
        }

       
        protected void PrependOrderings(IEnumerable<OrderExpression> newOrderings) {
            if (newOrderings != null) {
                if (this.gatheredOrderings == null) {
                    this.gatheredOrderings = newOrderings;
                }
                else {
                    List<OrderExpression> list = this.gatheredOrderings as List<OrderExpression>;
                    if (list == null) {
                        this.gatheredOrderings = list = new List<OrderExpression>(this.gatheredOrderings);
                    }
                    list.InsertRange(0, newOrderings);
                }
            }
        }

        protected class BindResult {
            ReadOnlyCollection<ColumnDeclaration> columns;
            ReadOnlyCollection<OrderExpression> orderings;
            public BindResult(IEnumerable<ColumnDeclaration> columns, IEnumerable<OrderExpression> orderings) {
                this.columns = columns as ReadOnlyCollection<ColumnDeclaration>;
                if (this.columns == null) {
                    this.columns = new List<ColumnDeclaration>(columns).AsReadOnly();
                }
                this.orderings = orderings as ReadOnlyCollection<OrderExpression>;
                if (this.orderings == null) {
                    this.orderings = new List<OrderExpression>(orderings).AsReadOnly();
                }
            }
            public ReadOnlyCollection<ColumnDeclaration> Columns {
                get { return this.columns; }
            }
            public ReadOnlyCollection<OrderExpression> Orderings {
                get { return this.orderings; }
            }
        }

     

         protected virtual BindResult RebindOrderings(IEnumerable<OrderExpression> orderings, string alias, HashSet<string> existingAliases, IEnumerable<ColumnDeclaration> existingColumns) {
             List<ColumnDeclaration> newColumns = null;
            List<OrderExpression> newOrderings = new List<OrderExpression>();
            foreach (OrderExpression ordering in orderings) {
                Expression expr = ordering.Expression;
                ColumnExpression column = expr as ColumnExpression;
                if (column == null || (existingAliases != null && existingAliases.Contains(column.Alias))) {
                    // check to see if a declared column already contains a similar expression
                    int iOrdinal = 0;
                    foreach (ColumnDeclaration decl in existingColumns) {
                        ColumnExpression declColumn = decl.Expression as ColumnExpression;
                        if (decl.Expression == ordering.Expression || 
                            (column != null && declColumn != null && column.Alias == declColumn.Alias && column.Name == declColumn.Name)) {
                            // found it, so make a reference to this column
                            expr = new ColumnExpression(column.Type, alias, decl.Name);
                            break;
                        }
                        iOrdinal++;
                    }
                    // if not already projected, add a new column declaration for it
                    if (expr == ordering.Expression) {
                        if (newColumns == null) {
                            newColumns = new List<ColumnDeclaration>(existingColumns);
                            existingColumns = newColumns;
                        }
                        string colName = column != null ? column.Name : "c" + iOrdinal;
                        newColumns.Add(new ColumnDeclaration(colName, ordering.Expression));
                        expr = new ColumnExpression(expr.Type, alias, colName);
                    }
                    newOrderings.Add(new OrderExpression(ordering.OrderType, expr));
                }
            }
            return new BindResult(existingColumns, newOrderings);
        }
        
    }

      internal class AliasesProduced : DbExpressionVisitor {
        HashSet<string> aliases;
   
        private AliasesProduced(TextWriter logger) : base (logger){
         this.aliases = new HashSet<string>();
        }
        internal static HashSet<string> Gather(Expression source,TextWriter logger) {
       AliasesProduced produced = new AliasesProduced(logger);
            produced.Visit(source);
            return produced.aliases;
        }

        protected override Expression VisitSelect(SelectExpression select) {
            this.aliases.Add(select.Alias);
            return select;
        }

        protected override Expression VisitTable(TableExpression table) {
            this.aliases.Add(table.Alias);
            return table;
        }
    }
}
