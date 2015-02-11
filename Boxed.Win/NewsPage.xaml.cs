using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking.Connectivity;
using Boxed.Common.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769
using Brain.Utils;
using PropertyChanged;
using System.Net.NetworkInformation;

namespace Boxed.Win
{
    [ImplementPropertyChanged]
    public sealed partial class NewsPage : SettingsFlyout
    {
        public ObservableCollection<NewsItem> News { get; set; }

        public bool Loading { get; set; }
        public string Message { get; set; }

        public NewsPage()
        {
            this.InitializeComponent();

            Loaded += OnLoaded;
            News = new ObservableCollection<NewsItem>();
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                Loading = true;
                Message = "";

                bool networkAvailable = NetworkInterface.GetIsNetworkAvailable();
                if (!networkAvailable)
                {
                    Message = "News is only available when online.  Please connect to the internet and try again.";
                    return;
                }

                var service = new NewsFeedService();
                var news = await service.GetFeed();

                News.Clear();
                foreach (var newsItem in news)
                {
                    News.Add(newsItem);
                }

                if (News.Count == 0)
                    Message = "No news feed at the moment.  Please try again later.";
            }
            catch (Exception ex)
            {
                Message = "Unable to access news feed at the moment.  Please try again later.";
            }
            finally
            {
                Loading = false;
            }
        }
    }
}
