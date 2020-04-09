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
        
        static void Main(string[] args)
        {
            ClickHouseConnectionSettings set = new ClickHouseConnectionSettings();
            set.Database = "default";
            set.Host = "localhost";
//            set.Port = 6000;
            set.Port = 32769;
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
                cmd.CommandTimeout = 30;

                cmd.CommandText = "CREATE DATABASE IF NOT EXISTS `firstdb`;";
                Console.WriteLine(cmd.ExecuteNonQuery());

                cmd.CommandText = "DROP TABLE IF EXISTS firstdb.firsttbl;";
                Console.WriteLine(cmd.ExecuteNonQuery());

                cmd.CommandText = "CREATE TABLE IF NOT EXISTS firstdb.firsttbl ( EventDate Date, ColId UInt64, ParamOrderID String  ) ENGINE = MergeTree() PARTITION BY toYYYYMM(EventDate) ORDER BY (ColId);";
                Console.WriteLine(cmd.ExecuteNonQuery());

                con.CreateCommand(
                    "INSERT INTO firstdb.firsttbl (EventDate, ColId, ParamOrderID) VALUES ('2019-01-01', 1, 'str1')").ExecuteNonQuery();
                con.CreateCommand(
                    "INSERT INTO firstdb.firsttbl (EventDate, ColId, ParamOrderID) VALUES ('2019-01-01', 2, 'str2')").ExecuteNonQuery();
                con.CreateCommand(
                    "INSERT INTO firstdb.firsttbl (EventDate, ColId, ParamOrderID) VALUES ('2019-01-01', 3, 'str3')").ExecuteNonQuery();

//                cmd.CommandText = "SELECT * from firstdb.firsttbl;";
                PrintData(con.CreateCommand("SELECT * from firstdb.firsttbl").ExecuteReader());
                
                // using (ClickHouseDataReader reader = (ClickHouseDataReader) cmd.ExecuteReader())
                // {
                //     Console.WriteLine("meme");
                //     //Console.WriteLine(reader.NextResult .ToString());//            set.Port = 32769;
                //
                //     //  Console.WriteLine("{0}",reader.GetInt64(0));
                //     while (reader.Read())
                //         //while(reader.NextResult())
                //     {
                //         Console.WriteLine(reader.FieldCount);
                //
                //         //  Console.WriteLine(reader.GetData(0));
                //         //  Console.WriteLine(reader.Read());
                //         //  Console.WriteLine("{0}",reader.GetValue());
                //         //   Console.WriteLine("{0}",reader.GetInt64(0));
                //         //  Console.WriteLine("{0}",reader.GetName(0));
                //         // Console.WriteLine("{0}",reader.GetOrdinal("WatchID"));
                //         //   Console.WriteLine(reader.GetValue(0));
                //     }
                // }
            }
        }
    }
}