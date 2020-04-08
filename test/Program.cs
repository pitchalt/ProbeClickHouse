using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.Odbc;
using ClickHouse.Ado.Impl.Data;
using ClickHouse.Ado;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
 
        ClickHouseConnectionSettings set = new ClickHouseConnectionSettings();
         set.Database = "default";
         set.Host = "localhost";
         set.Port = 9000;
         set.SocketTimeout = 60000000;
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
          cmd.CommandText = "select WatchID from default.hits_100m_obfuscated limit 10";
          
          Console.WriteLine(cmd.ExecuteNonQuery());
          using (ClickHouseDataReader reader = (ClickHouseDataReader)cmd.ExecuteReader())
          {
             Console.WriteLine("meme");
             //Console.WriteLine(reader.NextResult .ToString());
           //  Console.WriteLine("{0}",reader.GetInt64(0));
            while(reader.Read())
            //while(reader.NextResult())
            {
              Console.WriteLine(reader.FieldCount);

              //  Console.WriteLine(reader.GetData(0));
              //  Console.WriteLine(reader.Read());
              //  Console.WriteLine("{0}",reader.GetValue());
             //   Console.WriteLine("{0}",reader.GetInt64(0));
              //  Console.WriteLine("{0}",reader.GetName(0));
               // Console.WriteLine("{0}",reader.GetOrdinal("WatchID"));
             //   Console.WriteLine(reader.GetValue(0));
                
           
            }
           
          }

       
        }

        }
        
    }
}