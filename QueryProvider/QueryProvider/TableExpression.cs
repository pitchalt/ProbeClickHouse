using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq.Expressions;
using System.Reflection;
using System.Data.Common;


namespace QueryProviderTest
{
    internal enum DbExpressionType {

    Table = 1000, // make sure these don't overlap with ExpressionType

    Column,

    Select,

    Projection

}

internal class TableExpression : Expression {

  string alias;

    string name;

    internal TableExpression(Type type, string alias, string name)

        : base((ExpressionType)DbExpressionType.Table, type) {

        this.alias = alias;

        this.name = name;

    }

    internal string Alias {

     get { return this.alias; }

    }

    internal string Name {

        get { return this.name; }

    }

}

internal class ColumnExpression : Expression {

    string alias;

    string name;

    int ordinal;

    internal ColumnExpression(Type type, string alias, string name, int ordinal)

        : base((ExpressionType)DbExpressionType.Column, type) {

        this.alias = alias;

        this.name = name;

        this.ordinal = ordinal;

    }

    internal string Alias {

        get { return this.alias; }

  }

    internal string Name {

        get { return this.name; }

    }

    internal int Ordinal {

        get { return this.ordinal; }

    }

}

internal class ColumnDeclaration {

    string name;

    Expression expression;

    internal ColumnDeclaration(string name, Expression expression) {

        this.name = name;

        this.expression = expression;

    }

    internal string Name {

        get { return this.name; }

    }

    internal Expression Expression {

        get { return this.expression; }

    }

}

internal class SelectExpression : Expression {

    string alias;

    ReadOnlyCollection<ColumnDeclaration> columns;

    Expression from;

    Expression where;

    internal SelectExpression(Type type, string alias, IEnumerable<ColumnDeclaration> columns, Expression from, Expression where)

        : base((ExpressionType)DbExpressionType.Select, type) {

        this.alias = alias;

        this.columns = columns as ReadOnlyCollection<ColumnDeclaration>;

        if (this.columns == null) {

            this.columns = new List<ColumnDeclaration>(columns).AsReadOnly();

        }

        this.from = from;

        this.where = where;

    }

    internal string Alias {

        get { return this.alias; }

    }

    internal ReadOnlyCollection<ColumnDeclaration> Columns {

        get { return this.columns; }

    }

    internal Expression From {

        get { return this.from; }

    }

    internal Expression Where {

        get { return this.where; }

    }

}

internal class ProjectionExpression : Expression {

    SelectExpression source;

    Expression projector;

    internal ProjectionExpression(SelectExpression source, Expression projector)

        : base((ExpressionType)DbExpressionType.Projection, projector.Type) {

        this.source = source;

        this.projector = projector;

    }

    internal SelectExpression Source {

        get { return this.source; }

    }

    internal Expression Projector {

        get { return this.projector; }

    }

}
}