﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
/*
namespace QueryProviderTest {
    public class Customers {
        public string CustomerID;
        public string ContactName;
        public string Phone;
        public string City;
        public string Country;
    }

    public class Orders {
        public int OrderID;
        public string CustomerID;
        public DateTime OrderDate;
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

    class Program {
        static void Main(string[] args) {
            string constr = @"Data Source=.\SQLEXPRESS;AttachDbFilename=C:\data\Northwind.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True;MultipleActiveResultSets=true";
            using (SqlConnection con = new SqlConnection(constr)) {
                con.Open();
                Northwind db = new Northwind(con);

                string city = "London";
                var query = from c in db.Customers
                            where c.City == city
                            select new {
                                Name = c.ContactName,
                                Orders = from o in db.Orders
                                         where o.CustomerID == c.CustomerID
                                         select o
                            };


                foreach (var item in query) {
                    Console.WriteLine("{0}", item.Name);
                    foreach (var ord in item.Orders) {
                        Console.WriteLine("\tOrder: {0}", ord.OrderID);
                    }
                }

                Console.ReadLine();
            }
        }
    }
}
*/