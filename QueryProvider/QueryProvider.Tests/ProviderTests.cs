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

namespace QueryProviderTest.Tests
{
    public class TestData
    {
        
    }

    public class QueryTests: IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        // public static String ConnectionString { get; protected set; }

        private readonly String _DbFileName;
        private readonly String _SqLiteConnectionString;
        private readonly IDataLayer _XpoDataLayer;
        
        public Session CreateXpoSession()
        {
            return new Session(_XpoDataLayer);
        }
        public UnitOfWork CreateXpoUnitOfWork()
        {
            return new UnitOfWork(_XpoDataLayer);
        }

        public DbConnection CreateSqLiteConnection()
        {
            var con = new SqliteConnection(_SqLiteConnectionString);
            con.Open();
            return con;
        }

        public QueryTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _DbFileName = String.Join(String.Empty, Path.GetDirectoryName(typeof(QueryTests).Assembly.Location),
                Path.DirectorySeparatorChar.ToString(), "IntecoAG.XpoExt.QueryAdapter.Tests.", Guid.NewGuid().ToString(), ".db");
            _SqLiteConnectionString = "Data Source=" + _DbFileName;
            _XpoDataLayer = XpoDefault.GetDataLayer("XpoProvider=SQLite;" + _SqLiteConnectionString, AutoCreateOption.DatabaseAndSchema);
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
                customer4.Country = "USA";
                customer4.Phone = "662";
                customer4.ContactName = "mevk";

                Customers customer5 = new Customers(uow);
                customer5.CustomerID = "5";
                customer5.City = "Sizran";
                customer5.Country = "USSA";
                customer5.Phone = "668";
                customer5.ContactName = "mmme";

                uow.CommitChanges();
            }
        }

        [Fact]
        public void Part01TestConstructors()
        {
            try
            {
                var q1 = new Query<TestData>(null);
            }
            catch (ArgumentNullException e)
            {
                Assert.Equal("provider", e.ParamName);
            }
            Assert.True(true);
        }

        [Fact]
        public void Part02Query()
        {
            IList list;
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con);
                IQueryable<Customers> query =
                    db.Customers.Where(c => c.City == "London");
                _testOutputHelper.WriteLine("Query:\n{0}\n", query);
                 list = query.ToList();              
            }            
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void Part02DirectQuery()
        {
            using (var session = CreateXpoSession())
            {
                using var customers = new XPCollection<Customers>(session);
                Assert.Equal(5,customers.Count);
                foreach (var cust in customers)
                {
                    switch (cust.CustomerID)
                    {
                        case "1":
                            Assert.Equal("Moscow",cust.City);
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

            using (DbConnection con = CreateSqLiteConnection())
            {
                DbCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT * FROM (SELECT * FROM Customers) AS T WHERE (City = 'London')";
                DbDataReader reader = cmd.ExecuteReader();
                var count = 0;
                while (reader.Read())
                {
                    count++;
                }

                Assert.Equal(2, count);
            }
        }

        [Fact]
        public void Part03Query()
        {
            IList list;
            string str = "London";
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con);
                IQueryable<Customers> query =
                    db.Customers.Where(c => c.City == str);
                 list = query.ToList();              
            }            
            Assert.Equal(2, list.Count);          
        }

        [Fact]
        public void Part04Query()
        {
            IList list;
            string city = "London";
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con);
               var query = db.Customers.Where(c => c.City == city)
                            .Select(c => new {Name = c.ContactName, Phone = c.Phone});
                 list = query.ToList();              
              /*   foreach(var el in list)
                 {
                     Console.WriteLine($"{0}",el);
                 }*/
            }   
            
            Assert.Equal(2, list.Count);          
        }

        [Fact]
        public void Part05Query()
        {
            IList list;
            string city = "London";
            using (DbConnection con = CreateSqLiteConnection()) {
                Northwind db = new Northwind(con);
               var query = db.Customers.Select(c => new {
                    Name = c.ContactName,
                    Location = new {
                    City = c.City,
                    Country = c.Country
                    }
                     })
                .Where(x => x.Location.City == city);
                 list = query.ToList();              
                /* foreach(var el in list)
                 {
                     Console.WriteLine($"{0}",el);
                 }*/
            }   
            
            Assert.Equal(2, list.Count);          
        }
     
        public void Dispose()
        {
            _XpoDataLayer?.Dispose();
            if(System.IO.File.Exists(_DbFileName))
            {
                try
                {
                    System.IO.File.Delete(_DbFileName);
                }
                catch (System.IO.IOException e)
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }
            
        }
    }
}
