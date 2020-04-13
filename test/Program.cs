using System;
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

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS test.sum_table ( OrderID Int16, SubjectID Int16, Subject String, EventDate Date, Summ Decimal32(2)) ENGINE = SummingMergeTree(Summ) PARTITION BY toYYYYMM(EventDate) ORDER BY (OrderID,SubjectID,Subject,EventDate);";
                Console.WriteLine(cmd.ExecuteNonQuery());
              

                cmd.CommandText = "insert into test.sum_table values (1,1,'sub1','2021-01-01',1000)"; //(1,1,'sub1', '2020-01-01', 100.0)
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into test.sum_table values (2,2,'sub2','2022-01-01',2000)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into test.sum_table values (3,3,'sub3','2023-01-01',3000)";
                cmd.ExecuteNonQuery();
                
                
                //select OrderID, SubjectID,Subject,EventDate, sum(Summ) from test.sum_table group by OrderID, SubjectID,Subject,EventDate
            
            cmd.CommandText = "insert into test.sum_table values (1,1,'sub1','2021-01-01',-1000)"; //(1,1,'sub1', '2020-01-01', 100.0)
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into test.sum_table values (2,2,'sub2','2022-01-01',-2200)";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into test.sum_table values (3,3,'sub3','2023-01-01',-3300)";
                cmd.ExecuteNonQuery();
            
            }
                
                }
    
    
    public static void TemporaryTableTest()
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

            for (int i = 0; i < 5; i++)
            {
                Hits hit = new Hits();
                hit.EventDate = DateTime.Now;
                hit.EventTime = DateTime.Now;
                hit.WatchID = i.ToString();
                hit.Title = "str" + i.ToString();
               // hit.GoodEvent = (Int16)i;
                hit.JavaEnable = (Int16)(2 * i);

                list.Add(hit);
            }


            using (ClickHouseConnection con = new ClickHouseConnection(set))
            {
                con.Open();

                //var cmd = con.CreateCommand();
                ClickHouseCommand cmd = con.CreateCommand();
                

                cmd.CommandText = "CREATE DATABASE IF NOT EXISTS `test`;";
                Console.WriteLine(cmd.ExecuteNonQuery());

                cmd.CommandText = "DROP TABLE IF EXISTS test.temp_table;";
                Console.WriteLine(cmd.ExecuteNonQuery());

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS test.temp_table ( WatchID String, JavaEnable Int16, Title String, EventTime DateTime, EventDate Date) ENGINE = MergeTree() PARTITION BY toYYYYMM(EventDate) ORDER BY (WatchID);";
                Console.WriteLine(cmd.ExecuteNonQuery());



                cmd.CommandText = "CREATE TEMPORARY TABLE temp ( WatchID String, JavaEnable Int16, Title String, EventTime DateTime, EventDate Date) ENGINE = Memory()";
                Console.WriteLine(cmd.ExecuteNonQuery()); 
                
                var cmdInsert = con.CreateCommand("insert into temp values @bulk");
                cmdInsert.Parameters.Add(new ClickHouseParameter{ParameterName = "bulk", Value = list});
                cmdInsert.ExecuteNonQuery();

               cmd.CommandText = "insert into temp (WatchID,JavaEnable,Title,EventDate) values (1,1,'sub1','2021-01-01')"; //(1,1,'sub1', '2020-01-01', 100.0)
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into temp (WatchID,JavaEnable,Title,EventDate) values (2,2,'sub2','2022-01-01')";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "insert into temp (WatchID,JavaEnable,Title,EventDate) values (3,3,'sub3','2023-01-01')";
                cmd.ExecuteNonQuery();


                // var reader = con.CreateCommand("SELECT * from temp").ExecuteReader();
               //var reader = con.CreateCommand("SELECT JavaEnable, Title from default.hits_100m_obfuscated limit 10").ExecuteReader();
               //PrintData(reader);

            cmd.CommandText = "insert into test.temp_table select * from temp";
                cmd.ExecuteNonQuery();

           //  var reader = con.CreateCommand("SELECT * from test.temp_table").ExecuteReader();
              //  PrintData(reader);

                cmd.CommandText = "drop table temp";
                cmd.ExecuteNonQuery();

            }

    }


    static void Main(string[] args)
        {
           // SummingTest();
           TemporaryTableTest();
             
        }




    
    }


    
}