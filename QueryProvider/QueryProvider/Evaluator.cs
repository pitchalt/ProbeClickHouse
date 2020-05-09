using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace QueryProviderTest {

    public static class Evaluator {
        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="fnCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
      
         public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated, TextWriter logger) {
            return SubtreeEvaluator.Eval(Nominator.Nominate(fnCanBeEvaluated, expression,logger), expression,logger);
        }

        /// <summary>
        /// Performs evaluation & replacement of independent sub-trees
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEval(Expression expression, TextWriter logger) {
            return PartialEval(expression, Evaluator.CanBeEvaluatedLocally, logger);
        }

        private static bool CanBeEvaluatedLocally(Expression expression) {
            return expression.NodeType != ExpressionType.Parameter;
        }

        /// <summary>
        /// Evaluates & replaces sub-trees when first candidate is reached (top-down)
        /// </summary>
        class SubtreeEvaluator: DbExpressionVisitor {
            HashSet<Expression> candidates;

            internal SubtreeEvaluator(HashSet<Expression> candidates, TextWriter logger): base(logger) {
                this.candidates = candidates;
            }

             internal static Expression Eval(HashSet<Expression> candidates, Expression exp, TextWriter logger) {
                return new SubtreeEvaluator(candidates,logger).Visit(exp);
            }

            protected override Expression Visit(Expression exp) {
                if (exp == null) {
                    return null;
                }
                if (this.candidates.Contains(exp)) {
                    return this.Evaluate(exp);
                }
                return base.Visit(exp);
            }

            private Expression Evaluate(Expression e) {
                if (e.NodeType == ExpressionType.Constant) {
                    return e;
                }
                LambdaExpression lambda = Expression.Lambda(e);
                Delegate fn = lambda.Compile();
                return Expression.Constant(fn.DynamicInvoke(null), e.Type);
            }
        }

        /// <summary>
        /// Performs bottom-up analysis to determine which nodes can possibly
        /// be part of an evaluated sub-tree.
        /// </summary>
        class Nominator : DbExpressionVisitor {
            Func<Expression, bool> fnCanBeEvaluated;
            HashSet<Expression> candidates;
            bool cannotBeEvaluated;

            private Nominator(Func<Expression, bool> fnCanBeEvaluated, TextWriter logger): base(logger) {
                  this.candidates = new HashSet<Expression>();
                this.fnCanBeEvaluated = fnCanBeEvaluated;
            }

            internal static HashSet<Expression> Nominate(Func<Expression, bool> fnCanBeEvaluated, Expression expression, TextWriter logger) {
               Nominator nominator = new Nominator(fnCanBeEvaluated,logger);
                nominator.Visit(expression);
                return nominator.candidates;
            }

            protected override Expression Visit(Expression expression) {
                if (expression != null) {
                    bool saveCannotBeEvaluated = this.cannotBeEvaluated;
                    this.cannotBeEvaluated = false;
                    base.Visit(expression);
                    if (!this.cannotBeEvaluated) {
                        if (this.fnCanBeEvaluated(expression)) {
                            this.candidates.Add(expression);
                        }
                        else {
                            this.cannotBeEvaluated = true;
                        }
                    }
                    this.cannotBeEvaluated |= saveCannotBeEvaluated;
                }
                return expression;
            }
        }
    }
}