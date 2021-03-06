﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using ClickHouse.Ado.Impl.Data;
using ClickHouse.Ado;

namespace test
{
    class Program
    {
        private static void PrintData(IDataReader reader) {
            do {
                Console.Write("Fields: ");
                for (var i = 0; i < reader.FieldCount; i++) Console.Write("{0}:{1} ", reader.GetName(i), reader.GetDataTypeName(i));
                Console.WriteLine();
                while (reader.Read()) {
                    for (var i = 0; i < reader.FieldCount; i++) {
                        var val = reader.GetValue(i);
                        if (val.GetType().IsArray) {
                            Console.Write('[');
                            Console.Write(string.Join(", ", ((IEnumerable) val).Cast<object>()));
                            Console.Write(']');
                        } else {
                            Console.Write(val);
                        }

                        Console.Write(", ");
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();
            } while (reader.NextResult());
        }
        
            
    public static void firstTest()
    {
        ClickHouseConnectionSettings set = new ClickHouseConnectionSettings();
            set.Database = "default";
            set.Host = "localhost";
            set.Port = 9000;           
//            set.SocketTimeout = 60000000;
            set.Compress = true;
            set.User = "default";
            set.Password = "";

            List<Hits> list = new List<Hits>();

            //using (ClickHouseConnection con = new ClickHouseConnection("Host=127.0.0.1;Port=9000;User=default;Password=default;Compress=True;CheckCompressedHash=False;SocketTimeout=60000000;Compressor=lz4"))
            using (ClickHouseConnection con = new ClickHouseConnection(set))
            {
                con.Open();

                //var cmd = con.CreateCommand();
                ClickHouseCommand cmd = new ClickHouseCommand();
                cmd.Connection = con;
                cmd.CommandTimeout = 300;

               // cmd.CommandText = "CREATE DATABASE IF NOT EXISTS `firstdb`;";
            //   cmd.CommandText = "SELECT WatchID from default.hits_100m_obfuscated limit 10";
            //    Console.WriteLine(cmd.ExecuteNonQuery());

              //  cmd.CommandText = "DROP TABLE IF EXISTS firstdb.firsttbl;";
              //  Console.WriteLine(cmd.ExecuteNonQuery());
//
          //      cmd.CommandText = "CREATE TABLE IF NOT EXISTS firstdb.firsttbl ( EventDate Date, ColId UInt64, ParamOrderID String  ) ENGINE = MergeTree() PARTITION BY toYYYYMM(EventDate) ORDER BY (ColId);";
            //    Console.WriteLine(cmd.ExecuteNonQuery());

            //    con.CreateCommand(
         //          "INSERT INTO firstdb.firsttbl (EventDate, ColId, ParamOrderID) VALUES ('2019-01-01', 1, 'str1')").ExecuteNonQuery();
         //       con.CreateCommand(
          //          "INSERT INTO firstdb.firsttbl (EventDate, ColId, ParamOrderID) VALUES ('2019-01-01', 2, 'str2')").ExecuteNonQuery();
           //     con.CreateCommand(
            //        "INSERT INTO firstdb.firsttbl (EventDate, ColId, ParamOrderID) VALUES ('2019-01-01', 3, 'str3')").ExecuteNonQuery();

//                cmd.CommandText = "SELECT * from firstdb.firsttbl;";
              var reader = con.CreateCommand("SELECT WatchID, JavaEnable, Title, EventTime,  EventDate from default.hits_100m_obfuscated limit 10").ExecuteReader();
               //var reader = con.CreateCommand("SELECT JavaEnable, Title from default.hits_100m_obfuscated limit 10").ExecuteReader();
              // PrintData(reader);
               //for(int i = 0; i < reader.FieldCount; i++)
              // while (reader.Read())
             // int i = 0;

                
                do
              
               {
                  // Console.WriteLine(reader.GetName(i));

                    
                   while(reader.Read())
                   {
                       /*
                       Console.WriteLine($"WatchID = {reader.GetString(0)}");
                       Console.WriteLine($"JavaEnable = {reader.GetInt16(1)}");
                       Console.WriteLine($"Title = {reader.GetString(2)}");
                       Console.WriteLine($"EventTime = {reader.GetDateTime(3)}");
                       Console.WriteLine($"EventDate = {reader.GetDateTime(4)}");
                    */
                    Hits hit= new Hits();

                        hit.WatchID = (String)reader.GetString(0);
                        hit.JavaEnable = reader.GetInt16(1);
                        hit.Title = reader.GetString(2);
                        hit.EventTime = reader.GetDateTime(3);
                        hit.EventDate = reader.GetDateTime(4);
                list.Add(hit);

                      // var me = reader.GetInt16(1);
                   }
                   
                   //
                 //  object[] obj = new object[10];
                 //var rere = reader.Read();
                   //var me = ((ClickHouseDataReader)reader).GetString(2);
                   //i++;
                    
               }
                while (reader.NextResult());
              
                   // PrintData(reader);

          
         // foreach (var el in list)
        //  {
       //       Console.WriteLine(el.WatchID + "\t"+ el.JavaEnable+"\t"+ el.Title+"\t"+el.EventTime+"\t"+ el.EventDate);
      //    }


            ClickHouseConnectionSettings set2 = new ClickHouseConnectionSettings();
           // set2.Database = "tutorail";
            set2.Host = "localhost";
            set2.Port = 9000;           
//            set.SocketTimeout = 60000000;
            set2.Compress = true;
            set2.User = "default";
            set2.Password = "";

 using (ClickHouseConnection con2 = new ClickHouseConnection(set2))
            {
                con2.Open();

                //var cmd = con.CreateCommand();
                ClickHouseCommand cmd2 = con2.CreateCommand();
                

                cmd2.CommandText = "CREATE DATABASE IF NOT EXISTS `test`;";
                Console.WriteLine(cmd2.ExecuteNonQuery());

                cmd2.CommandText = "DROP TABLE IF EXISTS test.test_table;";
                Console.WriteLine(cmd2.ExecuteNonQuery());

                cmd2.CommandText = "CREATE TABLE IF NOT EXISTS test.test_table ( WatchID String, JavaEnable Int16, Title String, EventTime DateTime, EventDate Date) ENGINE = MergeTree() PARTITION BY toYYYYMM(EventDate) ORDER BY (WatchID);";
                Console.WriteLine(cmd2.ExecuteNonQuery());

                var cmdInsert = con2.CreateCommand("insert into test.test_table (WatchID, JavaEnable, Title, EventTime, EventDate) values @bulk");

                cmdInsert.Parameters.Add(new ClickHouseParameter{ParameterName = "bulk", Value = list});

                cmdInsert.ExecuteNonQuery();
                 con2.Close();
                }
            }

    }
    
    public static void SummingTest()
    {            
             ClickHouseConnectionSettings set = new ClickHouseConnectionSettings();

            set.Host = "localhost";
            set.Port = 9000;          

            set.Compress = true;
            set.User = "default";
            set.Password = ""; 
            

             using (ClickHouseConnection con = new ClickHouseConnection(set))
            {
                con.Open();

               ClickHouseCommand cmd = con.CreateCommand();
                

                cmd.CommandText = "CREATE DATABASE IF NOT EXISTS `test`;";
                Console.WriteLine(cmd.ExecuteNonQuery());

                cmd.CommandText = "DROP TABLE IF EXISTS test.sum_table;";
                Console.WriteLine(cmd.ExecuteNonQuery());

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS test.sum_table ( OrderID Int16, SubjectID Int16, Subject String, EventDate Date, Summ Decimal(13,2)) ENGINE = SummingMergeTree(Summ) PARTITION BY toYYYYMM(EventDate) ORDER BY (OrderID,SubjectID,Subject,EventDate);";
                Console.WriteLine(cmd.ExecuteNonQuery());
              

                cmd.CommandText = "insert into test.sum_table values (1,1,'sub1','2021-01-01',1000)"; //(1,1,'sub1', '2020-01-01', 100.0)
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into test.sum_table values (2,2,'sub2','2022-01-01',2000)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into test.sum_table values (3,3,'sub3','2023-01-01',3000)";
                cmd.ExecuteNonQuery();

               var reader = con.CreateCommand("select * from test.sum_table").ExecuteReader();
               PrintData(reader);
                //select OrderID, SubjectID,Subject,EventDate, sum(Summ) from test.sum_table group by OrderID, SubjectID,Subject,EventDate
            
              cmd.CommandText = "insert into test.sum_table select OrderID, SubjectID,Subject,EventDate, -1*(Summ) from test.sum_table where OrderID = 1"; //(1,1,'sub1', '2020-01-01', 100.0)
                cmd.ExecuteNonQuery();
               cmd.CommandText = "insert into test.sum_table select OrderID, SubjectID,Subject,EventDate, -1*(Summ) from test.sum_table where OrderID = 2"; //(1,1,'sub1', '2020-01-01', 100.0)
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into test.sum_table select OrderID, SubjectID,Subject,EventDate, -1*(Summ) from test.sum_table where OrderID = 3"; //(1,1,'sub1', '2020-01-01', 100.0)
                cmd.ExecuteNonQuery();

                cmd.CommandText = "optimize table test.sum_table final";
                cmd.ExecuteNonQuery();

               reader = con.CreateCommand("select * from test.sum_table").ExecuteReader();
               PrintData(reader);
             con.Close();
            }
                
                }
    

    public static void UpdateOrderData(List<SummClass> list, Int32 OrderID)
    {     

         using (ClickHouseConnection con = new ClickHouseConnection(CreateConnectionSettings()))
            {
                con.Open();

                ClickHouseCommand cmd = con.CreateCommand();                


                cmd.CommandText = "CREATE TEMPORARY TABLE temp (OrderID Int32, Subject String, Date Date, Summ Decimal(15,2)) ENGINE = Memory()";
                Console.WriteLine(cmd.ExecuteNonQuery()); 
                
                // insert into temp table reverse records
                cmd.CommandText = $"insert into temp select OrderID,Subject,Date, -1*SUM(Summ) from test.temp_table where OrderID ={OrderID} group by OrderID,Subject,Date";
                cmd.ExecuteNonQuery();

                //insert into temp table new list of records
                var cmdInsert = con.CreateCommand("insert into temp values @bulk");
                cmdInsert.Parameters.Add(new ClickHouseParameter{ParameterName = "bulk", Value = list});
                cmdInsert.ExecuteNonQuery();

                // insert everything from temp to test.temp_table
                cmd.CommandText = "insert into test.temp_table select * from temp";
                cmd.ExecuteNonQuery();


                cmd.CommandText = "drop table temp";
                cmd.ExecuteNonQuery();


               // cmd.CommandText = "optimize table test.temp_table  final";
              //  cmd.ExecuteNonQuery();

                con.Close();

            }

       
    }

     
     public static bool CheckOrderData(List<SummClass> list, Int32 OrderID)
     {

        List<SummClass> listFromDB = new List<SummClass>();
          using (ClickHouseConnection con = new ClickHouseConnection(CreateConnectionSettings()))
            {
                con.Open();
                //var cmd = con.CreateCommand();
                //cmd.CommandText = "select OrderID,Subject,Date,-1*SUM(Summ) from test.temp_table group by OrderID,Subject,Date";
                //Console.WriteLine(cmd.ExecuteNonQuery());
                var reader = con.CreateCommand($"select OrderID,Subject,Date, toDecimal64(SUM(Summ),2)  from test.temp_table where OrderID = {OrderID.ToString()} group by OrderID,Subject,Date having SUM(Summ) != 0").ExecuteReader();
                    
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

                con.Close();

            }

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


           // if(groupedList.Count() != listFromDB.Count())
          //  return false;
          //  else
            

             foreach(var el1 in listFromDB)
            {
                foreach (var el2 in groupedList)
                {
                if (el1.OrderID == el2.OrderID && el1.Subject == el2.Subject  
                    && el1.Date == el2.Date)
                    {
                        if (el1.Summ != el2.Summ)
                        return false;
                    }
                }
            }  


            return true;

     }
    
public static ClickHouseConnectionSettings CreateConnectionSettings()
{
        ClickHouseConnectionSettings set = new ClickHouseConnectionSettings();
         
            set.Host = "localhost";
            set.Port = 9000;           
            set.Compress = true;
            set.User = "default";
            set.Password = "";

            return set;

}

public static void CreateTable()
{
     using (ClickHouseConnection con = new ClickHouseConnection(CreateConnectionSettings()))
            {
                con.Open();

                //var cmd = con.CreateCommand();
                ClickHouseCommand cmd = con.CreateCommand();
                

                cmd.CommandText = "CREATE DATABASE IF NOT EXISTS `test`;";
                Console.WriteLine(cmd.ExecuteNonQuery());

                cmd.CommandText = "DROP TABLE IF EXISTS test.temp_table;";
                Console.WriteLine(cmd.ExecuteNonQuery());

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS test.temp_table ( OrderID Int32, Subject String, Date Date, Summ Decimal(15,2)) ENGINE = SummingMergeTree(Summ) PARTITION BY toYYYYMM(Date) ORDER BY (OrderID,Subject,Date);";
                Console.WriteLine(cmd.ExecuteNonQuery());
              

                con.Close();

            }
}


    public static void TemporaryTableTest()
    {
            // create begin records
            List<SummClass> listBeginOrd1 = new List<SummClass>();                      

            listBeginOrd1.Add(new SummClass(1,"sub1",new DateTime(2019,01,15),100));
            listBeginOrd1.Add(new SummClass(1,"sub1",new DateTime(2019,01,15),100));
            listBeginOrd1.Add(new SummClass(1,"sub1",new DateTime(2019,01,15),0));
            listBeginOrd1.Add(new SummClass(1,"sub2",new DateTime(2019,01,15),0));
            listBeginOrd1.Add(new SummClass(1,"sub3",new DateTime(2019,01,15),0));

            List<SummClass> listBeginOrd2 = new List<SummClass>();   
            listBeginOrd2.Add(new SummClass(2,"sub1",new DateTime(2019,01,15),0));
            listBeginOrd2.Add(new SummClass(2,"sub2",new DateTime(2019,01,15),0));
            listBeginOrd2.Add(new SummClass(2,"sub4",new DateTime(2019,01,15),0));

            List<SummClass> listBeginOrd3 = new List<SummClass>();   
            listBeginOrd3.Add(new SummClass(3,"sub2",new DateTime(2019,01,15),0));
            listBeginOrd3.Add(new SummClass(3,"sub3",new DateTime(2019,01,15),0));


           
        // create new table
        CreateTable();       

    
        UpdateOrderData(listBeginOrd1, 1);
        CheckOrderData(listBeginOrd1, 1);

        UpdateOrderData(listBeginOrd2, 2);
        CheckOrderData(listBeginOrd2, 2);

        UpdateOrderData(listBeginOrd3, 3);
        CheckOrderData(listBeginOrd3, 3);
        

      
        // create new list
        List<SummClass> newListOrd1 = new List<SummClass>();                      

            newListOrd1.Add(new SummClass(1,"sub5",new DateTime(2019,1,15),100));
            newListOrd1.Add(new SummClass(1,"sub2",new DateTime(2019,1,15),400));
            newListOrd1.Add(new SummClass(1,"sub3",new DateTime(2019,1,15),200));

        List<SummClass> newListOrd2 = new List<SummClass>();
            newListOrd2.Add(new SummClass(2,"sub1",new DateTime(2019,1,15),600));
            newListOrd2.Add(new SummClass(2,"sub3",new DateTime(2019,1,15),150));
            newListOrd2.Add(new SummClass(2,"sub4",new DateTime(2019,1,15),300));


        UpdateOrderData(newListOrd1, 1);
        Console.WriteLine(CheckOrderData(newListOrd1, 1));

        UpdateOrderData(newListOrd2, 2);
        Console.WriteLine(CheckOrderData(newListOrd2, 2));
          

    }


    static void Main(string[] args)
        {
           // SummingTest();
           TemporaryTableTest();
             
        }
    
    }

    
}