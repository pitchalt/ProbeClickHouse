using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace QueryProviderTest {

    public class DbQueryProvider : QueryProvider {
        DbConnection connection;
        TextWriter log;

        public DbQueryProvider(DbConnection connection) {
            this.connection = connection;
        }

        public TextWriter Log {
            get { return this.log; }
            set { this.log = value; }
        }

        public override string GetQueryText(Expression expression) {
            return this.Translate(expression).CommandText;
        }

        public override object Execute(Expression expression) {
            return this.Execute(this.Translate(expression));
        }

        private object Execute(TranslateResult query) {
            Delegate projector = query.Projector.Compile();

            if (this.log != null) {
                this.log.WriteLine(query.CommandText);
                this.log.WriteLine();
            }

            DbCommand cmd = this.connection.CreateCommand();
            cmd.CommandText = query.CommandText;
            DbDataReader reader = cmd.ExecuteReader();

            Type elementType = TypeSystem.GetElementType(query.Projector.Body.Type);
            return Activator.CreateInstance(
                typeof(ProjectionReader<>).MakeGenericType(elementType),
                BindingFlags.Instance | BindingFlags.NonPublic, null,
                new object[] { reader, projector, this, Log},
                null
                );
        }

        internal class TranslateResult {
            internal string CommandText;
            internal LambdaExpression Projector;
        }

        private TranslateResult Translate(Expression expression) {
            ProjectionExpression projection = expression as ProjectionExpression;
            if (projection == null) {
                expression = Evaluator.PartialEval(expression, CanBeEvaluatedLocally, log);
                expression = new QueryBinder(this, log).Bind(expression);
                expression = new OrderByRewriter(log).Rewrite(expression);
                projection = (ProjectionExpression)expression;
            }
            string commandText = new QueryFormatter(log).Format(projection.Source);
            LambdaExpression projector = new ProjectionBuilder(log).Build(projection.Projector, projection.Source.Alias);
            return new TranslateResult { CommandText = commandText, Projector = projector };
        }


        private static bool CanBeEvaluatedLocally(Expression expression) {
            return expression.NodeType != ExpressionType.Parameter &&
                   expression.NodeType != ExpressionType.Lambda;
        }

    } 
}
