using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QueryProviderTest {

    public abstract class ProjectionRow {
        public abstract object GetValue(int index);

        public object GetValueDateTime(int index) {
            var str_value = (String) GetValue(index);
            return DateTime.Parse(str_value);
        }
        public abstract IEnumerable<E> ExecuteSubQuery<E>(LambdaExpression query);
    }

    internal class ProjectionBuilder : DbExpressionVisitor {
        ParameterExpression row;
        string rowAlias;
        IList<string> columns;
        static MethodInfo miGetValue;
        static MethodInfo miGetValueDateTime;
        static MethodInfo miExecuteSubQuery;
        
        internal ProjectionBuilder(string rowAlias, IList<string> columns,TextWriter logger): base(logger) {
            this.rowAlias = rowAlias;
            this.columns = columns;
            this.row = Expression.Parameter(typeof(ProjectionRow), "row");
            if (miGetValue == null) {
                miGetValue = typeof(ProjectionRow).GetMethod("GetValue");
                miExecuteSubQuery = typeof(ProjectionRow).GetMethod("ExecuteSubQuery");
            }
        }

        internal static LambdaExpression Build(Expression expression, string alias, IList<string> columns,TextWriter logger ) {
                 ProjectionBuilder builder = new ProjectionBuilder(alias, columns,logger);
            Expression body = builder.Visit(expression);
            return Expression.Lambda(body, builder.row);
        }

        protected override Expression VisitColumn(ColumnExpression column) {
             if (column.Alias == this.rowAlias) {
                int iOrdinal = this.columns.IndexOf(column.Name);
                return Expression.Convert(
                    Expression.Call(typeof(System.Convert), "ChangeType", null,
                        Expression.Call(this.row, miGetValue, Expression.Constant(iOrdinal)),
                        Expression.Constant(column.Type)
                        ),                        
                        column.Type
                    );
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

    internal class ProjectionReader<T> : IEnumerable<T>, IEnumerable {
        Enumerator enumerator;

        protected readonly TextWriter Logger;
        internal ProjectionReader(DbDataReader reader, Func<ProjectionRow, T> projector, IQueryProvider provider, TextWriter logger) {
            Logger = logger;
            this.enumerator = new Enumerator(reader, projector, provider, logger);
        }

        public IEnumerator<T> GetEnumerator() {
            Enumerator e = this.enumerator;
            if (e == null) {
                throw new InvalidOperationException("Cannot enumerate more than once");
            }
            this.enumerator = null;
            return e;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        class Enumerator : ProjectionRow, IEnumerator<T>, IEnumerator, IDisposable {
            DbDataReader reader;
            T current;
            Func<ProjectionRow, T> projector;
            IQueryProvider provider;

            protected readonly TextWriter Logger;
            internal Enumerator(DbDataReader reader, Func<ProjectionRow, T> projector, IQueryProvider provider, TextWriter logger) {
                this.reader = reader;
                this.projector = projector;
                this.provider = provider;
                Logger = logger;
            }

            public override object GetValue(int index) {
                if (index >= 0) {
                    if (this.reader.IsDBNull(index)) {
                        return null;
                    }
                    else {
                        return this.reader.GetValue(index);
                    }
                }
                throw new IndexOutOfRangeException();
            }
        public override IEnumerable<E> ExecuteSubQuery<E>(LambdaExpression query) {
                ProjectionExpression projection = (ProjectionExpression) Replacer.Replace(query.Body, query.Parameters[0], Expression.Constant(this), Logger);
                projection = (ProjectionExpression) Evaluator.PartialEval(projection, CanEvaluateLocally, Logger);
                IEnumerable<E> result = (IEnumerable<E>)this.provider.Execute(projection);
                List<E> list = new List<E>(result);
                if (typeof(IQueryable<E>).IsAssignableFrom(query.Body.Type)) {
                    return list.AsQueryable();
                }
                return list;
            }
            private static bool CanEvaluateLocally(Expression expression) {
                if (expression.NodeType == ExpressionType.Parameter ||
                    expression.NodeType.IsDbExpression()) {
                    return false;
                }
                return true;
            }

            public T Current {
                get { return this.current; }
            }

            object IEnumerator.Current {
                get { return this.current; }
            }

            public bool MoveNext() {
                if (this.reader.Read()) {
                    this.current = this.projector(this);
                    return true;
                }
                return false;
            }

            public void Reset() {
            }

            public void Dispose() {
                this.reader.Dispose();
            }
        }
    }
}
