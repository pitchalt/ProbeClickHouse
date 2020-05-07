using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;


namespace QueryProviderTest {

    internal enum OrderType {
        Ascending,
        Descending
    }

    internal class OrderExpression {
        OrderType orderType;
        Expression expression;
        internal OrderExpression(OrderType orderType, Expression expression) {
            this.orderType = orderType;
            this.expression = expression;
        }
        internal OrderType OrderType {
            get { return this.orderType; }
        }
        internal Expression Expression {
            get { return this.expression; }
        }
    }
}