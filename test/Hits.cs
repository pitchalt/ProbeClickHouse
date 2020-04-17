using System;
using System.Collections;

namespace test
{



    public class SummClass : IEnumerable, IComparable
    {

      public SummClass(Int32 orderid, String	subject,	DateTime	date, Decimal summ)
      {
        this.Date = date;
        this.OrderID = orderid;
        this.Subject = subject;
        this.Summ = summ;
       
      }
      public SummClass()
      {}
    
        public	Int32	OrderID {get;set;}	//UInt64			
        public		String	Subject	{get;set;}				
        public		DateTime	Date	{get;set;}		
        public Decimal Summ {get;set;}

         public IEnumerator GetEnumerator()
        {
            yield return OrderID ;     
              yield return Subject ;     
                yield return Date ;  
                yield return Summ ; 
        }	

        public int CompareTo(object obj)
         {
            if(obj == null)
            return 1;

            SummClass otherClass = obj as SummClass;
            if(otherClass != null)
            {
              if(this.OrderID == otherClass.OrderID && this.Subject == otherClass.Subject 
              && this.Date == otherClass.Date)
              return 0;
              else return 1;
            }
            else
            {
              throw new Exception("object is not SummClass");
            }

         }	
    }
    public class Hits : IEnumerable
    {
    
        public	String	WatchID {get;set;}			
        public	Int16		JavaEnable		{get;set;}	
        public		String	Title	{get;set;}				
        public		DateTime	EventTime	{get;set;}			
        public		DateTime	EventDate		{get;set;}		
  
        public IEnumerator GetEnumerator()
        {
            yield return WatchID ;    
             yield return JavaEnable ;    
              yield return Title ;    
               yield return EventTime ;    
                yield return EventDate ;                                                                                                     
 
        }


    }
 
}