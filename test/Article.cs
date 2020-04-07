using System;

namespace test
{

    public class Article
    {
        public int ID{get;set;}
        public Article ParentArticle {get;set;}
        public string Code {get;set;}
    }
 
}