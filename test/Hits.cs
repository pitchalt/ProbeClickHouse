using System;
using System.Collections;

namespace test
{



    public class SummClass : IEnumerable
    {
    
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
    }
    public class Hits : IEnumerable
    {
    
        public	String	WatchID {get;set;}	//UInt64			
        public	Int16		JavaEnable		{get;set;}	
        public		String	Title	{get;set;}			
      //  public		Int16	GoodEvent		{get;set;}		
        public		DateTime	EventTime	{get;set;}			
        public		DateTime	EventDate		{get;set;}		
       /* public		UInt32	CounterID	{get;set;}			
        public		UInt32	ClientIP		{get;set;}		
        public		UInt32	RegionID		{get;set;}		
        public		UInt64	UserID			{get;set;}	
        public		Int16	CounterClass	{get;set;}			
        public		UInt16	OS				{get;set;}
        public		UInt16	UserAgent	{get;set;}			
        public		String		URL			{get;set;}
        public		String		Referer			{get;set;}
        public		UInt16		Refresh		{get;set;}	
        public		UInt16		RefererCategoryID		{get;set;}	
        public		UInt32		RefererRegionID			{get;set;}
        public		UInt16		URLCategoryID			{get;set;}
        public		UInt32	URLRegionID				{get;set;}
        public		UInt16	ResolutionWidth				{get;set;}
        public		UInt16	ResolutionHeight			{get;set;}	
        public		UInt16	ResolutionDepth				{get;set;}
        public		UInt16	FlashMajor				{get;set;}
        public		UInt16	FlashMinor				{get;set;}
        public		String	FlashMinor2				{get;set;}
        public		UInt16	NetMajor				{get;set;}
        public		UInt16	NetMinor				{get;set;}
        public		UInt16		UserAgentMajor			{get;set;}
        public		string		UserAgentMinor			{get;set;}
        public		UInt16		CookieEnable			{get;set;}
        public		UInt16		JavascriptEnable			{get;set;}
        public		UInt16		IsMobile			{get;set;}
        public		UInt16	MobilePhone				{get;set;}
        public		String		MobilePhoneModel		{get;set;}	
        public		String		Params			{get;set;}
        public		UInt32		IPNetworkID			{get;set;}
        public		Int16		TraficSourceID			{get;set;}
        public		UInt16		SearchEngineID			{get;set;}
        public		String		SearchPhrase			{get;set;}
        public		UInt16		AdvEngineID			{get;set;}
        public		UInt16		IsArtifical			{get;set;}
        public		UInt16		WindowClientWidth			{get;set;}
        public		UInt16		WindowClientHeight			{get;set;}
        public		Int16		ClientTimeZone			{get;set;}
        public		DateTime	ClientEventTime				{get;set;}
        public		UInt16		SilverlightVersion1			{get;set;}
        public		UInt16		SilverlightVersion2			{get;set;}
        public		UInt32		SilverlightVersion3			{get;set;}
        public		UInt16		SilverlightVersion4			{get;set;}
        public		String		PageCharset			{get;set;}
        public		UInt32		CodeVersion			{get;set;}
        public		UInt16		IsLink			{get;set;}
        public		UInt16		IsDownload			{get;set;}
        public		UInt16		IsNotBounce			{get;set;}
        public		UInt64	FUniqID				{get;set;}
        public		String		OriginalURL			{get;set;}
        public		UInt32	HID				{get;set;}
        public		UInt16	IsOldCounter				{get;set;}
        public		UInt16	IsEvent				{get;set;}
        public		UInt16	IsParameter				{get;set;}
        public		UInt16		DontCountHits		{get;set;}	
        public		UInt16	WithHash				{get;set;}
        public		string HitColor					{get;set;}
        public		DateTime LocalEventTime				{get;set;}	
        public		UInt16 Age					{get;set;}
        public		UInt16	 Sex				{get;set;}
        public		UInt16		Income			{get;set;}
        public		UInt16		 Interests			{get;set;}
        public		UInt16		Robotness			{get;set;}
        public		UInt32		RemoteIP			{get;set;}
        public		Int32	WindowName				{get;set;}
        public		Int32	OpenerName				{get;set;}
        public		Int16		HistoryLength			{get;set;}
        public		String		BrowserLanguage			{get;set;}
        public		String	BrowserCountry				{get;set;}
        public		String		SocialNetwork			{get;set;}
        public		String		SocialAction			{get;set;}
        public		UInt16		HTTPError			{get;set;}
        public		UInt32		SendTiming			{get;set;}
        public		UInt32	DNSTiming				{get;set;}
        public		UInt32		ConnectTiming			{get;set;}
        public		UInt32		ResponseStartTiming			{get;set;}
        public		UInt32		ResponseEndTiming			{get;set;}
        public		UInt32		FetchTiming			{get;set;}
        public		UInt16		SocialSourceNetworkID		{get;set;}	
        public		String		SocialSourcePage			{get;set;}
        public		Int64		ParamPrice			{get;set;}
        public		String		ParamOrderID			{get;set;}
        public		String ParamCurrency				{get;set;}	
        public		UInt16	ParamCurrencyID				{get;set;}
        public		String	OpenstatServiceName			{get;set;}	
        public		String	OpenstatCampaignID			{get;set;}	
        public		String	OpenstatAdID				{get;set;}
        public		String	OpenstatSourceID			{get;set;}	
        public		String	UTMSource				{get;set;}
        public		String	UTMMedium				{get;set;}
        public		String	UTMCampaign				{get;set;}
        public		String	UTMContent				{get;set;}
        public		String	UTMTerm				{get;set;}
        public		String	FromTag				{get;set;}
        public		UInt16	HasGCLID				{get;set;}
        public		UInt64	RefererHash				{get;set;}
        public		UInt64	URLHash				{get;set;}
        public		UInt32 CLID {get;set;}
*/

        public IEnumerator GetEnumerator()
        {
            yield return WatchID ;    
             yield return JavaEnable ;    
              yield return Title ;    
               yield return EventTime ;    
                yield return EventDate ;                                                                                                     
 //JavaEnable                                                                                                    
 //Title                                                                                                        
// GoodEvent                                                                                                     
 //EventTime                                                                                                  
 //EventDate
 /*                                                                                                       
 CounterID                                                                                                    
 ClientIP                                                                                                     
 RegionID                                                                                                     
 UserID                                                                                                       
 CounterClass                                                                                                   
 OS                                                                                                            
 UserAgent                                                                                                     
 URL                                                                                                          
 Referer                                                                                                      
 Refresh                                                                                                       
 RefererCategoryID                                                                                            
 RefererRegionID                                                                                              
 URLCategoryID                                                                                                
 URLRegionID                                                                                                  
 ResolutionWidth                                                                                              
 ResolutionHeight                                                                                             
 ResolutionDepth                                                                                               
 FlashMajor                                                                                                    
 FlashMinor                                                                                                    
 FlashMinor2                                                                                                  
 NetMajor                                                                                                      
 NetMinor                                                                                                      
 UserAgentMajor                                                                                               
 UserAgentMinor                                                                                       
 CookieEnable                                                                                                  
 JavascriptEnable                                                                                              
 IsMobile                                                                                                      
 MobilePhone                                                                                                   
 MobilePhoneModel                                                                                             
 Params                                                                                                       
 IPNetworkID                                                                                                  
 TraficSourceID                                                                                                 
 SearchEngineID                                                                                               
 SearchPhrase                                                                                                 
 AdvEngineID                                                                                                   
 IsArtifical                                                                                                   
 WindowClientWidth                                                                                            
 WindowClientHeight                                                                                           
 ClientTimeZone                                                                                                
 ClientEventTime                                                                                            
 SilverlightVersion1                                                                                           
 SilverlightVersion2                                                                                           
 SilverlightVersion3                                                                                          
 SilverlightVersion4                                                                                          
 PageCharset                                                                                                  
 CodeVersion                                                                                                  
 IsLink                                                                                                        
 IsDownload                                                                                                    
 IsNotBounce                                                                                                   
 FUniqID                                                                                                      
 OriginalURL                                                                                                  
 HID                                                                                                          
 IsOldCounter                                                                                                  
 IsEvent                                                                                                       
 IsParameter                                                                                                   
 DontCountHits                                                                                                 
 WithHash                                                                                                      
 HitColor                                                                                            
 LocalEventTime                                                                                             
 Age                                                                                                           
 Sex                                                                                                         
 Income                                                                                                        
 Interests                                                                                                      
 Robotness                                                                                                     
 RemoteIP                                                                                                     
 WindowName                                                                                                    
 OpenerName             
 HistoryLength                                                                                                 
 BrowserLanguage                                                                                    
 BrowserCountry                                                                                       
 SocialNetwork                                                                                                
 SocialAction                                                                                                 
 HTTPError                                                                                                    
 SendTiming                                                                                                   
 DNSTiming                                                                                                    
 ConnectTiming                                                                                                
 ResponseStartTiming                                                                                          
 ResponseEndTiming                                                                                            
 FetchTiming                                                                                                  
 SocialSourceNetworkID                                                                                         
 SocialSourcePage       
 ParamPrice                                                                                                    
 ParamOrderID                                                                                                 
 ParamCurrency                                                                                        
 ParamCurrencyID                                                                                              
 OpenstatServiceName                                                                                          
 OpenstatCampaignID                                                                                           
 OpenstatAdID                                                                                                 
 OpenstatSourceID                                                                                             
 UTMSource                                                                                                    
 UTMMedium                                                                                                    
 UTMCampaign                                                                                                  
 UTMContent                                                                                                   
 UTMTerm                                                                                                      
 FromTag                                                                                                      
 HasGCLID                                                                                                      
 RefererHash                                                                                                  
 URLHash                                                                                                      
 CLID 
 */
        }


    }
 
}