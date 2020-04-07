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
        //    using (var sr = new StreamReader("/home/natalia/meme/fact.txt"))
           //  {                   
               
                // string allProperty = sr.ReadToEnd();
              
                // FactLS fact = new FactLS();
                /*  while( sr.Peek() >= 0)
                  {
                    line = sr.;
                    allProperty += line;
                    
                    if( line == "/n")
                    {
                      factList.Add(fact);
                      fact = new FactLS();
                      i = 0;
                      allProperty = "";
                    }
                      if(line == " ")
                      {
                        if(i == 0)
                        {
                          fact.ID = Int32.Parse(allProperty);
                          i++ ;
                        }
                        else 
                        if(i == 1)
                        {
                          fact.Summ = Decimal.Parse(allProperty);
                          i++;
                        }
                        else 
                        if(i == 2)
                        {
                          fact.Article = new Article();
                          fact.Article.Code = allProperty;                         
                        }

                      }*/
                 // }
          //    }  

       
       // foreach(var fact in factList)
        //  {
         //   Console.WriteLine(fact.ID.ToString() + " " + fact.Summ.ToString() + " " + fact.Article.Code.ToString());
            
        //  }


         
        using (ClickHouseConnection con = new ClickHouseConnection("Host=127.0.0.1;Port=9000;User=default;Password=default;Compress=True;CheckCompressedHash=False;SocketTimeout=60000000;Compressor=lz4"))
        {
          
          var cmd = con.CreateCommand();
          cmd.CommandText = "select count() from tutorial.hits_v1";
          con.Open();

          using (var reader = cmd.ExecuteReader())
          {
           // Console.WriteLine(reader.NextResult().ToString());
          }

       
        }

        }
        
    }
}