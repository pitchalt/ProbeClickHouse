using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;


namespace QueryProviderTest {
    internal enum JoinType {
        CrossJoin,
        InnerJoin,
        CrossApply,
    }

    internal class JoinExpression : Expression {
        JoinType joinType;
        Expression left;
        Expression right;
        Expression condition;
        internal JoinExpression(Type type, JoinType joinType, Expression left, Expression right, Expression condition)
            : base((ExpressionType)DbExpressionType.Join, type) {
            this.joinType = joinType;
            this.left = left;
            this.right = right;
            this.condition = condition;
        }
        internal JoinType Join {
            get { return this.joinType; }
        }
        internal Expression Left {
            get { return this.left; }
        }
        internal Expression Right {
            get { return this.right; }
        }
        internal new Expression Condition {
            get { return this.condition; }
        }
    }
}
