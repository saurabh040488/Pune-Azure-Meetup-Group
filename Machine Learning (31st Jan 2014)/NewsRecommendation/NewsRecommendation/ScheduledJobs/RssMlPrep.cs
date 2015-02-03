using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml.XPath;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NewsRecommendation.Models;

namespace NewsRecommendation.ScheduledJobs
{
    public class RssMlPrepJob : ScheduledJob
    {
        public override async Task ExecuteAsync()
        {
            Services.Log.Info("Hello from scheduled job!");

            using (var httpClient = new HttpClient())
            {
                var rssfeedSources = new Dictionary<string, string>
                {
                    {"IbnLive", "http://ibnlive.in.com/ibnrss/top.xml"},
                    {"Google", "https://news.google.co.in/news?pz=1&cf=all&ned=in&hl=en&output=rss"},
                    {"HindustanTimes", "http://feeds.hindustantimes.com/HT-HomePage-TopStories?format=xml"},                    
                    {"Ndtv", "http://feeds.feedburner.com/NdtvNews-TopStories?format=xml"},
                    {"ZeeNews", "http://zeenews.india.com/rss/india-national-news.xml"},
                    {"IndiaTv","http://www.indiatvnews.com/rssfeed/topstory_news.xml"},
                    {"AbpNews","http://www.abplive.in/india/?widgetName=rssfeed&widgetContentId=101313&getXmlFeed=true"},
                    {"NewsNation","http://www.newsnation.in/newsnation-rss/64"},
                    {"Newsx","http://www.newsx.com/index.php?option=com_obrss&task=feed&id=2:rss"}
                };

                var table = GetCloudTable();

                Services.Log.Info("got the table!!!");

                foreach (var rssfeedSource in rssfeedSources)
                {
                    Services.Log.Info(string.Format("getting feed from source : {0}",rssfeedSource.Key));

                    var response =
                        await
                            httpClient.GetAsync(new Uri(rssfeedSource.Value));
                    var data = await response.Content.ReadAsStringAsync();
                    var xnodes = new XPathDocument(new MemoryStream(Encoding.UTF8.GetBytes(data)))
                        .CreateNavigator()
                        .Select("//item");
                    Services.Log.Info("got the feed!!!");
                 
                    foreach (XPathNavigator node in xnodes)
                    {
                        var selectSingleNode = node.SelectSingleNode("title");
                        if (selectSingleNode == null) continue;
                        var title = selectSingleNode.ToString();

                        //Include the title in the text and prepare text...
                        var xPathNavigator = node.SelectSingleNode("description");
                        if (xPathNavigator == null) continue;
                        var text =
                            PrepareText(string.Format("{0} {1}", title, xPathNavigator));

                        //hash the title to remove invalid chars and use it as the key
                        var rowKey = ComputeSH1Hash(title);

                        var pathNavigator = node.SelectSingleNode("link");
                        if (pathNavigator == null) continue;
                        var tableOp = TableOperation.InsertOrMerge(new BlogEntry
                        {
                            Source = rssfeedSource.Key,
                            Text = text,
                            PubDate = DateTime.Now,
                            Link = pathNavigator.ToString(),
                            RowKey = rowKey,
                            PartitionKey = "BlogEntries",
                            Title = title.Trim()
                        });

                        Services.Log.Info("populated the table!!!");

                        await table.ExecuteAsync(tableOp);
                    }

                    Services.Log.Info(string.Format("Completed Source {0}",rssfeedSource.Key));
                }
            }
        }

        private string ComputeSH1Hash(string data)
        {
            string retdata;

            using (var sha = SHA1.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
                var builder = new StringBuilder();

                // Loop through each byte of the hashed data  
                // and format each one as a hexadecimal string. 
                foreach (var t in bytes)
                {
                    builder.Append(t.ToString("x2"));
                }

                retdata = builder.ToString();

            }

            return retdata;
        }

        private CloudTable GetCloudTable()
        {
            CloudStorageAccount cloudStorageAccount;
            CloudStorageAccount.TryParse(CloudConfigurationManager.GetSetting("StorageConnectionString"),
                out cloudStorageAccount);
            var tClient = cloudStorageAccount.CreateCloudTableClient();
            var tref = tClient.GetTableReference(CloudConfigurationManager.GetSetting("SourceDataTable"));
       
            tref.CreateIfNotExists();

            return tref;

        }

        private string PrepareText(string htmlData)
        {
            //lower case all the data
            var data = htmlData.ToLower();
            data = data.Replace(Environment.NewLine, " ");

            //remove CSS ...
            data = Regex.Replace(data, @"<style[^><]*>.*</style>", string.Empty, RegexOptions.Singleline);

            //remove HTML ...
            data = Regex.Replace(data, "<.*?>", string.Empty);
            data = HttpUtility.HtmlDecode(data);

            //Replace apostrophe with single quote ...            
            data = data.Replace("’", "'");

            //remove punctuation characters other than ' and -            
            data = Regex.Replace(data, @"[^\w\'\s\-]", " ");

            //stop words...
            var sws = new[]
            {
                "a's", "able", "about", "above", "according", "accordingly", "across", "actually", "after", "afterwards",
                "again", "against", "ain't", "all", "allow", "allows", "almost", "alone", "along", "already", "also",
                "although", "always", "am", "among", "amongst", "an", "and", "another", "any", "anybody", "anyhow",
                "anyone", "anything", "anyway", "anyways", "anywhere", "apart", "appear", "appreciate", "appropriate",
                "are", "aren't", "around", "as", "aside", "ask", "asking", "associated", "at", "available", "away",
                "awfully", "be", "became", "because", "become", "becomes", "becoming", "been", "before", "beforehand",
                "behind", "being", "believe", "below", "beside", "besides", "best", "better", "between", "beyond",
                "both", "brief", "but", "by", "c'mon", "c's", "came", "can", "can't", "cannot", "cant", "cause",
                "causes", "certain", "certainly", "changes", "clearly", "co", "com", "come", "comes", "concerning",
                "consequently", "consider", "considering", "contain", "containing", "contains", "corresponding", "could",
                "couldn't", "course", "currently", "definitely", "described", "despite", "did", "didn't", "different",
                "do", "does", "doesn't", "doing", "don't", "done", "down", "downwards", "during", "each", "edu", "eg",
                "eight", "either", "else", "elsewhere", "enough", "entirely", "especially", "et", "etc", "even", "ever",
                "every", "everybody", "everyone", "everything", "everywhere", "ex", "exactly", "example", "except",
                "far", "few", "fifth", "first", "five", "followed", "following", "follows", "for", "former", "formerly",
                "forth", "four", "from", "further", "furthermore", "get", "gets", "getting", "given", "gives", "go",
                "goes", "going", "gone", "got", "gotten", "greetings", "had", "hadn't", "happens", "hardly", "has",
                "hasn't", "have", "haven't", "having", "he", "he's", "hello", "help", "hence", "her", "here", "here's",
                "hereafter", "hereby", "herein", "hereupon", "hers", "herself", "hi", "him", "himself", "his", "hither",
                "hopefully", "how", "howbeit", "however", "i'd", "i'll", "i'm", "i've", "ie", "if", "ignored",
                "immediate", "in", "inasmuch", "inc", "indeed", "indicate", "indicated", "indicates", "inner", "insofar",
                "instead", "into", "inward", "is", "isn't", "it", "it'd", "it'll", "it's", "its", "itself", "just",
                "keep", "keeps", "kept", "know", "known", "knows", "last", "lately", "later", "latter", "latterly",
                "least", "less", "lest", "let", "let's", "like", "liked", "likely", "little", "look", "looking", "looks",
                "ltd", "mainly", "many", "may", "maybe", "me", "mean", "meanwhile", "merely", "might", "more",
                "moreover", "most", "mostly", "much", "must", "my", "myself", "name", "namely", "nd", "near", "nearly",
                "necessary", "need", "needs", "neither", "never", "nevertheless", "new", "next", "nine", "no", "nobody",
                "non", "none", "noone", "nor", "normally", "not", "nothing", "novel", "now", "nowhere", "obviously",
                "of", "off", "often", "oh", "ok", "okay", "old", "on", "once", "one", "ones", "only", "onto", "or",
                "other", "others", "otherwise", "ought", "our", "ours", "ourselves", "out", "outside", "over", "overall",
                "own", "particular", "particularly", "per", "perhaps", "placed", "please", "plus", "possible",
                "presumably", "probably", "provides", "que", "quite", "qv", "rather", "rd", "re", "really", "reasonably",
                "regarding", "regardless", "regards", "relatively", "respectively", "right", "said", "same", "saw",
                "say", "saying", "says", "second", "secondly", "see", "seeing", "seem", "seemed", "seeming", "seems",
                "seen", "self", "selves", "sensible", "sent", "serious", "seriously", "seven", "several", "shall", "she",
                "should", "shouldn't", "since", "six", "so", "some", "somebody", "somehow", "someone", "something",
                "sometime", "sometimes", "somewhat", "somewhere", "soon", "sorry", "specified", "specify", "specifying",
                "still", "sub", "such", "sup", "sure", "t's", "take", "taken", "tell", "tends", "th", "than", "thank",
                "thanks", "thanx", "that", "that's", "thats", "the", "their", "theirs", "them", "themselves", "then",
                "thence", "there", "there's", "thereafter", "thereby", "therefore", "therein", "theres", "thereupon",
                "these", "they", "they'd", "they'll", "they're", "they've", "think", "third", "this", "thorough",
                "thoroughly", "those", "though", "three", "through", "throughout", "thru", "thus", "to", "together",
                "too", "took", "toward", "towards", "tried", "tries", "truly", "try", "trying", "twice", "two", "un",
                "under", "unfortunately", "unless", "unlikely", "until", "unto", "up", "upon", "us", "use", "used",
                "useful", "uses", "using", "usually", "value", "various", "very", "via", "viz", "vs", "want", "wants",
                "was", "wasn't", "way", "we", "we'd", "we'll", "we're", "we've", "welcome", "well", "went", "were",
                "weren't", "what", "what's", "whatever", "when", "whence", "whenever", "where", "where's", "whereafter",
                "whereas", "whereby", "wherein", "whereupon", "wherever", "whether", "which", "while", "whither", "who",
                "who's", "whoever", "whole", "whom", "whose", "why", "will", "willing", "wish", "with", "within",
                "without", "won't", "wonder", "would", "wouldn't", "yes", "yet", "you", "you'd", "you'll", "you're",
                "you've", "your", "yours", "yourself", "yourselves", "zero"
            };

            data = sws.Aggregate(data, (current, w) => current.Replace(string.Format(" {0} ", w), " "));

            data = data.Replace("  ", " ");

            return data;
        }
    }
}