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
using DevExpress.Xpo;

namespace QueryProviderTest
{
public class Customers: XPLiteObject {
        [Key]
        public string CustomerID;
        public string ContactName;
        public string Phone;
        public string City;
        public string Country;

        public Customers() { }
        public Customers(Session session): base(session) { }
    }
    public class Orders: XPLiteObject {
        [Key]
        public int OrderID;
        public string CustomerID;
        public DateTime OrderDate;

        public Orders() { }
        public Orders(Session session) : base(session) { }
    }

 public class Northwind {
        public Query<Customers> Customers;
        public Query<Orders> Orders;

        private DbQueryProvider provider;
        public Northwind(DbConnection connection) {
            this.provider = new DbQueryProvider(connection);
            this.Customers = new Query<Customers>(this.provider);
            this.Orders = new Query<Orders>(this.provider);
        }

        public TextWriter Log {
            get { return this.provider.Log; }
            set { this.provider.Log = value; }
        }
    }

}