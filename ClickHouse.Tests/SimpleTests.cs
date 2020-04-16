using System;
using Xunit;
using ClickHouse.Ado;
using ClickHouse.Ado.Impl.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using test;

namespace ClickHouse.Tests
{
    public class SimpleTests
    {
        ClickHouseConnectionSettings settings; 
        public SimpleTests()
        {
           settings = new ClickHouseConnectionSettings();   
           settings.Host = "localhost";
            settings.Port = 9000;           
            settings.Compress = true;
            settings.User = "default";
            settings.Password = ""; 
        }

     
        [Fact]
        public void ConnectionTest()
        {
            int res;
        using (ClickHouseConnection con = new ClickHouseConnection(settings))
            {
                con.Open();           
                ClickHouseCommand cmd = con.CreateCommand();                

                cmd.CommandText = "select 1";
                res = cmd.ExecuteNonQuery();             

                con.Close();               

            }
            Assert.Equal(res,0);
        }

        [Fact]
        public void CreateTableTest()
        {
            int res1;
            int res2;
            int res3;
        using (ClickHouseConnection con = new ClickHouseConnection(settings))
            {
                con.Open();
                
                ClickHouseCommand cmd = con.CreateCommand();                

                cmd.CommandText = "CREATE DATABASE IF NOT EXISTS `test`;";
                res1 = cmd.ExecuteNonQuery();

                cmd.CommandText = "DROP TABLE IF EXISTS test.temp_table;";
                res2 = cmd.ExecuteNonQuery();

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS test.temp_table ( OrderID Int32, Subject String, Date Date, Summ Decimal(15,2)) ENGINE = SummingMergeTree(Summ) PARTITION BY toYYYYMM(Date) ORDER BY (OrderID,Subject,Date);";
                res3 = cmd.ExecuteNonQuery();             

                con.Close();

            }

            Assert.Equal(res1 + res2 + res3, 0);           
        }
        public SummClass CreateNewObject(Int32 OrderID, String Subject, DateTime Date, Decimal Summ)
        {
            SummClass sc = new SummClass();
                sc.Date = Date;
                sc.OrderID = OrderID;
                sc.Subject = Subject;
                sc.Summ = Summ;
                return sc;
        }

       [Fact]
       public void CreateGroupedListTest()
       {
          
           List<SummClass> list = new List<SummClass>();                      

            list.Add(CreateNewObject(1,"sub1",DateTime.Parse("2019-01-01"),100)); 
            list.Add(CreateNewObject(1,"sub1",DateTime.Parse("2019-01-01"),373)); // повтор
            list.Add(CreateNewObject(1,"sub1",DateTime.Parse("2019-01-01"),345)); //повтор
            list.Add(CreateNewObject(1,"sub2",DateTime.Parse("2019-01-01"),450));
            list.Add(CreateNewObject(1,"sub3",DateTime.Parse("2019-01-01"),305));             
            list.Add(CreateNewObject(2,"sub1",DateTime.Parse("2019-01-01"),520));
            list.Add(CreateNewObject(2,"sub2",DateTime.Parse("2019-01-01"),530));
            list.Add(CreateNewObject(2,"sub2",DateTime.Parse("2019-01-01"),543)); //повтор
            list.Add(CreateNewObject(2,"sub4",DateTime.Parse("2019-01-01"),530));         
            list.Add(CreateNewObject(3,"sub2",DateTime.Parse("2019-01-01"),60));
            list.Add(CreateNewObject(3,"sub3",DateTime.Parse("2019-01-01"),560));
            list.Add(CreateNewObject(3,"sub3",DateTime.Parse("2019-01-01"),346)); //повтор


            List<SummClass> groupedList = new List<SummClass>();
            for(int i = 0; i< list.Count; i++)
            {
                var el = groupedList.Where(x => x.OrderID == list.ElementAt(i).OrderID
                                        && x.Subject == list.ElementAt(i).Subject 
                                        && x.Date == list.ElementAt(i).Date).Select(x => x);               

                  if(el.Count() == 0)
                    {                        
                        groupedList.Add(new SummClass {OrderID = list.ElementAt(i).OrderID,
                        Subject = list.ElementAt(i).Subject, Date = list.ElementAt(i).Date, 
                        Summ = list.ElementAt(i).Summ});
                    }
                    else 
                        if(el.Count() > 1)
                            throw new Exception("smth goes wrong");
                    else
                    {
                        el.ElementAt(0).Summ += list.ElementAt(i).Summ;
                    }                        
                    
                     
            }

            Assert.Equal(groupedList.Count, list.Count-4); 
         
       } 
         
       [Fact]
       public void EqualsTwoListWithoutSummTest()
       {
           int res = 0;

           // списки с уникальными записями
         List<SummClass> list1 = new List<SummClass>();                      

            list1.Add(CreateNewObject(1,"sub1",DateTime.Parse("2019-01-01"),100)); 
            list1.Add(CreateNewObject(1,"sub2",DateTime.Parse("2019-01-01"),400));
            list1.Add(CreateNewObject(1,"sub3",DateTime.Parse("2019-01-01"),200));             
            list1.Add(CreateNewObject(2,"sub1",DateTime.Parse("2019-01-01"),600));
            list1.Add(CreateNewObject(2,"sub2",DateTime.Parse("2019-01-01"),530));
            list1.Add(CreateNewObject(2,"sub4",DateTime.Parse("2019-01-01"),300));         
            list1.Add(CreateNewObject(3,"sub2",DateTime.Parse("2019-01-01"),60));
            list1.Add(CreateNewObject(3,"sub3",DateTime.Parse("2019-01-01"),560));

            List<SummClass> list2 = new List<SummClass>();                      

            list2.Add(CreateNewObject(1,"sub5",DateTime.Parse("2019-01-01"),100));
            list2.Add(CreateNewObject(1,"sub2",DateTime.Parse("2019-01-01"),400));
            list2.Add(CreateNewObject(1,"sub3",DateTime.Parse("2019-01-01"),200));

            list2.Add(CreateNewObject(2,"sub1",DateTime.Parse("2019-01-01"),600));
            list2.Add(CreateNewObject(2,"sub3",DateTime.Parse("2019-01-01"),150));
            list2.Add(CreateNewObject(2,"sub4",DateTime.Parse("2019-01-01"),300));
                
          
            // Общbq kbcn
            List<SummClass> listBoth = new List<SummClass>();  
            foreach(var el1 in list1)
            {
                foreach (var el2 in list2)
                {
                if (el1.OrderID == el2.OrderID && el1.Subject == el2.Subject  
                    && el1.Date == el2.Date)
                    {
                        if (el1.Summ != el2.Summ)
                        res++;
                    }
                }
            }  
            
            Assert.Equal(res, 0);  

       } 

       [Fact]
       public void InsertDataIntoTempTableTest()
       {
           int res = 0;
           List<SummClass> list = new List<SummClass>();                      

            list.Add(CreateNewObject(1,"sub1",DateTime.Parse("2019-01-01"),100)); 
            list.Add(CreateNewObject(1,"sub2",DateTime.Parse("2019-01-01"),450));
            list.Add(CreateNewObject(1,"sub3",DateTime.Parse("2019-01-01"),305));             
            list.Add(CreateNewObject(2,"sub1",DateTime.Parse("2019-01-01"),520));
            list.Add(CreateNewObject(2,"sub2",DateTime.Parse("2019-01-01"),530));
            list.Add(CreateNewObject(2,"sub4",DateTime.Parse("2019-01-01"),530));         
            list.Add(CreateNewObject(3,"sub2",DateTime.Parse("2019-01-01"),60));
            list.Add(CreateNewObject(3,"sub3",DateTime.Parse("2019-01-01"),560));

            using (ClickHouseConnection con = new ClickHouseConnection(settings))
            {
                con.Open();

                ClickHouseCommand cmd = con.CreateCommand();              


                cmd.CommandText = "CREATE TEMPORARY TABLE temp (OrderID Int32, Subject String, Date Date, Summ Decimal(15,2)) ENGINE = Memory()";
                res += cmd.ExecuteNonQuery(); 
                         
                var cmdInsert = con.CreateCommand("insert into temp values @bulk");
                cmdInsert.Parameters.Add(new ClickHouseParameter{ParameterName = "bulk", Value = list});
                res += cmdInsert.ExecuteNonQuery();

                cmd.CommandText = "drop table temp";
                res += cmd.ExecuteNonQuery();


                con.Close();

            }

          Assert.Equal(res, 0);
       } 
         
         [Fact]
       public void FillListFromDatabaseTest()
       {         

           List<SummClass> list = new List<SummClass>();                      

            list.Add(CreateNewObject(1,"sub1",DateTime.Parse("2019-01-01"),100)); 
            list.Add(CreateNewObject(1,"sub2",DateTime.Parse("2019-01-01"),450));
            list.Add(CreateNewObject(1,"sub3",DateTime.Parse("2019-01-01"),305));             
            list.Add(CreateNewObject(2,"sub1",DateTime.Parse("2019-01-01"),520));
            list.Add(CreateNewObject(2,"sub2",DateTime.Parse("2019-01-01"),530));
            list.Add(CreateNewObject(2,"sub4",DateTime.Parse("2019-01-01"),530));         
            list.Add(CreateNewObject(3,"sub2",DateTime.Parse("2019-01-01"),60));
            list.Add(CreateNewObject(3,"sub3",DateTime.Parse("2019-01-01"),560));
            

            List<SummClass> listFromDB = new List<SummClass>();
          using (ClickHouseConnection con = new ClickHouseConnection(settings))
            {
                con.Open();

                ClickHouseCommand cmd = con.CreateCommand(); 
                cmd.CommandText = "CREATE TEMPORARY TABLE temp (OrderID Int32, Subject String, Date Date, Summ Decimal(15,2)) ENGINE = Memory()";
                cmd.ExecuteNonQuery(); 
                         
                var cmdInsert = con.CreateCommand("insert into temp values @bulk");
                cmdInsert.Parameters.Add(new ClickHouseParameter{ParameterName = "bulk", Value = list});
                cmdInsert.ExecuteNonQuery();

                var reader = con.CreateCommand($"select OrderID,Subject,Date, toDecimal64(SUM(Summ),2) from temp group by OrderID,Subject,Date").ExecuteReader();
                    
                do              
               {                                    
                   while(reader.Read())
                   {
                     
                    SummClass sc= new SummClass();

                        sc.OrderID = (Int32)reader.GetInt32(0);
                        sc.Subject = (String)reader.GetString(1);
                        sc.Date = (DateTime)reader.GetDateTime(2);
                        sc.Summ = (Decimal)reader.GetDecimal(3);
                    
                    listFromDB.Add(sc);

                   }
           
                    
               }
                while (reader.NextResult());

                cmd.CommandText = "drop table temp";
                cmd.ExecuteNonQuery();

                con.Close();

            }



         Assert.Equal(listFromDB.Count, list.Count);
       } 
         
        


    }
}
