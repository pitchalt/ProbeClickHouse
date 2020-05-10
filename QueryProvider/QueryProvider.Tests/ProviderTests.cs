using System;
using System.Data.Common;
using System.IO;
using System.Linq;

using Xunit;

using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using Microsoft.Data.Sqlite;

using Xunit.Abstractions;

using QueryProviderTest;

using System.Collections.Generic;
using System.Collections;
using System.Text;

using Xunit.Sdk;

namespace QueryProviderTest.Tests {

    public class TestData { }

    public class TestOutTextWriter : TextWriter {

        private readonly ITestOutputHelper _Output;
        private readonly StringBuilder _Builder;
        public override Encoding Encoding { get; }

        public override void Write(char value) {
            if (value != '\n')
                _Builder.Append(value);
            else {
                _Output.WriteLine(_Builder.ToString());
                _Builder.Clear();
            }
        }

        public TestOutTextWriter(ITestOutputHelper testOut) {
            _Output = testOut;
            _Builder = new StringBuilder(1024);
        }

    }

    public class QueryTests : IDisposable {

        private readonly ITestOutputHelper _testOutputHelper;

        private readonly TestOutTextWriter _TestOutWriter;
        // public static String ConnectionString { get; protected set; }

        private readonly String _DbFileName;
        private readonly String _SqLiteConnectionString;
        private readonly IDataLayer _XpoDataLayer;

        public Session CreateXpoSession() {
            return new Session(_XpoDataLayer);
        }

        public UnitOfWork CreateXpoUnitOfWork() {
            return new UnitOfWork(_XpoDataLayer);
        }

        public DbConnection CreateSqLiteConnection() {
            var con = new SqliteConnection(_SqLiteConnectionString);
            con.Open();
            return con;
        }

        public QueryTests(ITestOutputHelper testOutputHelper) {
            _testOutputHelper = testOutputHelper;
            _TestOutWriter = new TestOutTextWriter(testOutputHelper);
            _DbFileName = String.Join(String.Empty, Path.GetDirectoryName(typeof(QueryTests).Assembly.Location),
                Path.DirectorySeparatorChar.ToString(), "QueryAdapter.Tests.", Guid.NewGuid().ToString(), ".db");
            _SqLiteConnectionString = "Data Source=" + _DbFileName;
            _XpoDataLayer = XpoDefault.GetDataLayer("XpoProvider=SQLite;" + _SqLiteConnectionString,
                AutoCreateOption.DatabaseAndSchema);
            using (UnitOfWork uow = CreateXpoUnitOfWork()) {
                Customers customer = new Customers(uow);
                customer.CustomerID = "1";
                customer.City = "Moscow";
                customer.Country = "Russia";
                customer.Phone = "111";
                customer.ContactName = "Test papa";

                Customers customer2 = new Customers(uow);
                customer2.CustomerID = "2";
                customer2.City = "London";
                customer2.Country = "Greate Britan";
                customer2.Phone = "222";
                customer2.ContactName = "Test mama";

                Customers customer3 = new Customers(uow);
                customer3.CustomerID = "3";
                customer3.City = "London";
                customer3.Country = "Great Britain";
                customer3.Phone = "666";
                customer3.ContactName = "meme";

                Customers customer4 = new Customers(uow);
                customer4.CustomerID = "4";
                customer4.City = "Saratov";
                customer4.Country = "USSA";
                customer4.Phone = "662";
                customer4.ContactName = "mevk";

                Customers customer5 = new Customers(uow);
                customer5.CustomerID = "5";
                customer5.City = "Sizran";
                customer5.Country = "USSA";
                customer5.Phone = "668";
                customer5.ContactName = "mmme";


                Orders order1 = new Orders(uow);
                order1.OrderID = 1;
                order1.CustomerID = customer.CustomerID;
                order1.OrderDate = new DateTime(2019, 1, 15);

                Orders order2 = new Orders(uow);
                order2.OrderID = 2;
                order2.CustomerID = customer2.CustomerID;
                order2.OrderDate = new DateTime(2019, 1, 15);

                Orders order3 = new Orders(uow);
                order3.OrderID = 3;
                order3.CustomerID = customer3.CustomerID;
                order3.OrderDate = new DateTime(2019, 1, 15);

                Orders order4 = new Orders(uow);
                order4.OrderID = 4;
                order4.CustomerID = customer4.CustomerID;
                order4.OrderDate = new DateTime(2019, 1, 15);


                uow.CommitChanges();
            }
        }

        [Fact]
        public void Part01TestConstructors() {
            try {
                var q1 = new Query<TestData>(null);
            }
            catch (ArgumentNullException e) {
                Assert.Equal("provider", e.ParamName);
            }

            Assert.True(true);
        }

        [Fact]
        public void Part02ConstQueryFormatterTest() {
            using (DbConnection con = CreateSqLiteConnection()) {
                var provider = new DbQueryProvider(con) {
                    Log = _TestOutWriter
                };
                var query = new Query<Customers>(provider);
                var command = query.ToString();
                Assert.StartsWith("SELECT", command);
            }

            Assert.True(true);
        }

        [Fact]
        public void Part02Query() {
            IList list;
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con, _TestOutWriter);
                IQueryable<Customers> query =
                        db.Customers.Where(c => c.City == "London");
                _testOutputHelper.WriteLine("Query:\n{0}\n", query);
                list = query.ToList();
            }

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void Part02DirectQuery() {
            using (var session = CreateXpoSession()) {
                using var customers = new XPCollection<Customers>(session);
                Assert.Equal(5, customers.Count);
                foreach (var cust in customers) {
                    switch (cust.CustomerID) {
                    case "1":
                        Assert.Equal("Moscow", cust.City);
                        break;
                    case "2":
                        Assert.Equal("London", cust.City);
                        break;
                    case "3":
                        Assert.Equal("London", cust.City);
                        break;
                    case "4":
                        Assert.Equal("Saratov", cust.City);
                        break;
                    case "5":
                        Assert.Equal("Sizran", cust.City);
                        break;

                    default:
                        throw new ArgumentException("Unknow CustId");
                    }
                }
            }

            using (DbConnection con = CreateSqLiteConnection()) {
                DbCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT * FROM (SELECT * FROM Customers) AS T WHERE (City = 'London')";
                DbDataReader reader = cmd.ExecuteReader();
                var count = 0;
                while (reader.Read()) {
                    count++;
                }

                Assert.Equal(2, count);
            }
        }

        [Fact]
        public void Part03Query() {
            IList list;
            string str = "London";
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con, _TestOutWriter);
                IQueryable<Customers> query =
                        db.Customers.Where(c => c.City == str);
                list = query.ToList();
            }

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void Part04Query() {
            IList list;
            string city = "London";
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con, _TestOutWriter);
                var query = db.Customers.Where(c => c.City == city)
                              .Select(c => new {Name = c.ContactName, Phone = c.Phone});
                list = query.ToList();
            }

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void Part05Query() {
            IList list;
            string city = "London";
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con, _TestOutWriter);
                var query = db.Customers.Select(c => new {
                                   Name = c.ContactName,
                                   Location = new {
                                       City = c.City,
                                       Country = c.Country
                                   }
                               })
                              .Where(x => x.Location.City == city);
                list = query.ToList();
            }

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void Part06Query() {
            IList list;
            string city = "London";
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con, _TestOutWriter);
                var query = from c in db.Customers
                        where c.City == city
                        select new {
                            Name = c.ContactName,
                            Ords = from o in db.Orders
                                    where o.CustomerID == c.CustomerID
                                    select o
                        };
                _testOutputHelper.WriteLine("Query:\n{0}\n", query);
                list = query.ToList();
            }

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void Part06DirectOrderQuery() {
            using (DbConnection con = CreateSqLiteConnection()) {
                DbCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT OrderId, OrderDate FROM (SELECT * FROM Orders) AS T ";
                DbDataReader reader = cmd.ExecuteReader();
                var count = 0;
                while (reader.Read()) {
                    var ct0 = reader.GetFieldType(0);
                    var ct = reader.GetFieldType(1);
//                     Int64 orderId = (Int64) reader.GetValue(0);
                    Assert.Equal(typeof(Int64), ct0);
                    Assert.Equal(typeof(String), ct);
                    //                    DateTime orderDate = (DateTime) reader.GetValue(1);
                    count++;
                }
            }
        }

        [Fact]
        public void Part07JoinQuery() {
            IList list;
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con, _TestOutWriter);
                var query = from c in db.Customers
                        where c.CustomerID == "2"
                        from o in db.Orders
                        where c.CustomerID == o.CustomerID
                        select new {c.ContactName, o.OrderDate};
                //   _testOutputHelper.WriteLine("Query:\n{0}\n", query);
                list = query.ToList();
            }

            Assert.Equal(1, list.Count);
        }

//        [Fact]
        public void Part07SelectManyQuery()
        {
            //IEnumerable en;
            IList list;
            int c=0;
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con, _TestOutWriter);
               var query = db.Customers
              .Where(c => c.CustomerID == "2")
              .SelectMany(
                 c => db.Orders.Where(o => c.CustomerID == o.CustomerID),
                 (c, o) => new { c.ContactName, o.OrderDate }
                 );
                 list = query.ToList();
            }   
            
            Assert.Equal(1, 1);          
        }

        [Fact]
        public void Part08OrderQuery() {
            IList list;
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con, _TestOutWriter);
                var query = from c in db.Customers
                        orderby c.City
                        where c.Country == "USSA"
                        select new {c.City, c.ContactName};
                _testOutputHelper.WriteLine("Query:\n{0}\n", query);
                list = query.ToList();
            }

            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void Part09RemoveQuery() {
            IList list;
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con, _TestOutWriter);
                var query = 
                        from c in db.Customers
                        join o in db.Orders on c.CustomerID equals o.CustomerID
                        let m = c.Phone
                        orderby c.City
                        where c.Country == "USSA"
                        where m != "555-5555"
                        select new {c.City, c.ContactName}
                        into x
                        where x.City == "Sizran"
                        select x;
                _testOutputHelper.WriteLine("Query:\n{0}\n", query);
                list = query.ToList();
            }

            Assert.Equal(0, list.Count);
        }

        public void Dispose() {
            _XpoDataLayer?.Dispose();
            if (System.IO.File.Exists(_DbFileName)) {
                try {
                    System.IO.File.Delete(_DbFileName);
                }
                catch (System.IO.IOException e) {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
        }

    }

}