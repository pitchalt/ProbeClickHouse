using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
          //  Console.WriteLine("Hello World!");
            List<FactLS> factList = new List<FactLS>();
            List<Article> aricleList = new List<Article>();
            //Console.WriteLine("mr");
            using (var sr = new StreamReader("/home/natalia/meme/fact.txt"))
             {  
                 //Console.WriteLine("mr");
                 char line = Char.Parse(sr.Read().ToString());
                // string allProperty = sr.ReadToEnd();
                 int i = 0;
                 Console.WriteLine(line);
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
              }  

        Console.WriteLine(factList.Count);
       // foreach(var fact in factList)
        //  {
         //   Console.WriteLine(fact.ID.ToString() + " " + fact.Summ.ToString() + " " + fact.Article.Code.ToString());
            
        //  }


/*
 string connectionString =
            ConsoleApplication1.Properties.Settings.Default.ConnectionString;
        //
        // In a using statement, acquire the SqlConnection as a resource.
        //
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            //
            // Open the SqlConnection.
            //
            con.Open();
            //
            // This code uses an SqlCommand based on the SqlConnection.
            //
            using (SqlCommand command = new SqlCommand("SELECT TOP 2 * FROM Dogs1", con))
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine("{0} {1} {2}",
                        reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                }
            }
        }

*/
        }
        
    }
}