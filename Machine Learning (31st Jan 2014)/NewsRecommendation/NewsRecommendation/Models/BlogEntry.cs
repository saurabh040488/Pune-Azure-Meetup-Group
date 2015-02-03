using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace NewsRecommendation.Models
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/sample".

    public class BlogEntry : TableEntity
    {
        public string Source { get; set; }
        public string Text { get; set; }
        public DateTime PubDate { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
    }

    public class BlogEntryResult : TableEntity
    {
        public string Source { get; set; }
        public string Assignments { get; set; }
        public DateTime PubDate { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
    }

    public class BlogItem
    {
        public string Source { get; set; }
        public string Text { get; set; }
        public DateTime PubDate { get; set; }
        public string Link { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }

}