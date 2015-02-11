using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Boxed.Win.Controls
{
    public class WebViewEx
    {
        public static string GetHtml(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlProperty);
        }

        public static void SetHtml(DependencyObject obj, string html)
        {
            obj.SetValue(HtmlProperty, html);
        }

        public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(
            "Html", typeof (string), typeof (WebViewEx), new PropertyMetadata(default(string), OnHtmlChanged));

        private static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var webView = d as WebView;
            if (webView == null) return;

            webView.NavigateToString((string)e.NewValue);
        }
    }
}
