using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Brain.Utils;

namespace Boxed.Common.Services
{
    public class NewsFeedService
    {
        public const string FeedUri = "http://0brain.com/Feed.xml";

        public async Task<List<NewsItem>> GetFeed()
        {
            try
            {
                var newsItems = new List<NewsItem>();

                string feed;
                using (var client = new HttpClient())
                {
                    feed = await client.GetStringAsync(new Uri(FeedUri));
                }

                Debug.WriteLine("News Feed: " + FeedUri);
                Debug.WriteLine(feed);

                // Could have done this, but that would be too easy
                // var xDoc = XDocument.Load(FeedUri);
                var xDoc = XDocument.Parse(feed);

                var items = xDoc.Descendants("item");
                foreach (var itemElement in items)
                {
                    var item = new NewsItem
                    {
                        Title = itemElement.Element("title").Value,
                        Description = itemElement.Element("description").Value,
                        PubDate = itemElement.Element("pubDate").Value,
                        Link = itemElement.Element("link").Value,
                        Author = itemElement.Element("author").Value
                    };
                    newsItems.Add(item);
                }

                return newsItems;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return new List<NewsItem>();
            }
        }
    }
}
