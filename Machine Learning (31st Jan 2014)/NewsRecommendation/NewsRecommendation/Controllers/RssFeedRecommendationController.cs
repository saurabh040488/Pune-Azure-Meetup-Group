using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using NewsRecommendation.Models;


namespace NewsRecommendation.Controllers
{
    public class BlogRecommendationsController : ApiController
    {
        public ApiServices Services { get; set; }
    
        // GET api/BlogRecommendations
        [AuthorizeLevel(Microsoft.WindowsAzure.Mobile.Service.Security.AuthorizationLevel.Application)]
        public IEnumerable<BlogItem> Get(string title)
        {
            var resultsTable = GetCloudTable(CloudConfigurationManager.GetSetting("RecommendationsDataTable"));

            Services.Log.Info(CloudConfigurationManager.GetSetting("RecommendationsDataTable"));

            Services.Log.Info(title);

            var cluster =resultsTable.CreateQuery<BlogEntryResult>()
                        .Where<BlogEntryResult>(b => b.Title == title)
                        .Select<BlogEntryResult, string>
                        (r => r.Assignments)
                        .FirstOrDefault<string>();

            if (cluster != null)
            {
                return resultsTable.CreateQuery<BlogEntryResult>()
                    .Where<BlogEntryResult>(b => b.Assignments == cluster && b.Title != title)
                    .Select<BlogEntryResult, BlogItem>
                    (r =>
                        new BlogItem
                        {
                            Title = r.Title,
                            Link = r.Link,
                            PubDate = r.PubDate,
                            Source = r.Source,
                            Url = r.Title.Replace(" ", "-").Trim()
                        });
            }

            return null;
        }
        private CloudTable GetCloudTable(string name)
        {
            CloudStorageAccount cloudStorageAccount;
            CloudStorageAccount.TryParse(CloudConfigurationManager.GetSetting("StorageConnectionString"), out cloudStorageAccount);
            var tClient = cloudStorageAccount.CreateCloudTableClient();
            var tref = tClient.GetTableReference(name);
            tref.CreateIfNotExists();

            return tref;

        }

    }
}

