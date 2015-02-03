using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NewsRecommendation.Models;

namespace NewsRecommendation.Controllers
{
    public class GetNewsController : ApiController
    {
        public ApiServices Services { get; set; }

        // GET api/GetNews
        public IEnumerable<BlogItem> Get()
        {
            Services.Log.Info("Hello from custom controller!");

            var resultsTable = GetCloudTable(CloudConfigurationManager.GetSetting("SourceDataTable"));


            Services.Log.Info(CloudConfigurationManager.GetSetting("SourceDataTable"));


            var cluster = resultsTable.CreateQuery<BlogEntry>().ToList().Take(50);

            Services.Log.Info("Got the news");

            return cluster.Select(blogEntry =>
            {
                var text = blogEntry.Text.ToString(CultureInfo.InvariantCulture);
                text = text.Substring(0, text.Length > 200 ? 200 : text.Length);
                return
                    new BlogItem
                    {
                        Link = blogEntry.Link,
                        PubDate = blogEntry.PubDate,
                        Source = blogEntry.Source,
                        Text = text,
                        Title = blogEntry.Title,
                        Url = blogEntry.Title.Replace(" ", "-").Trim(),

                    };
            });
        }

        public BlogItem Get(string title)
        {
            Services.Log.Info("Hello get a paricular value!");

            var resultsTable = GetCloudTable(CloudConfigurationManager.GetSetting("SourceDataTable"));

            Services.Log.Info(CloudConfigurationManager.GetSetting("SourceDataTable"));

            var cluster =
                resultsTable.CreateQuery<BlogEntry>()
                    .Where<BlogEntry>(x => x.Title.Equals(title))
                    .Select<BlogEntry, BlogItem>(
                        x =>
                            new BlogItem
                            {
                                Source = x.Source,
                                Text = x.Text,
                                PubDate = x.PubDate,
                                Link = x.Link,
                                Title = x.Title,
                                Url = x.Title.Replace(" ", "-").Trim(),
                            }).FirstOrDefault();                    
            return cluster;

        }

        private CloudTable GetCloudTable(string name)
        {
            CloudStorageAccount cloudStorageAccount;
            CloudStorageAccount.TryParse(CloudConfigurationManager.GetSetting("StorageConnectionString"),
                out cloudStorageAccount);
            var tClient = cloudStorageAccount.CreateCloudTableClient();
            var tref = tClient.GetTableReference(name);
            tref.CreateIfNotExists();

            return tref;

        }

    }
}
