using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Trip.Services
{
    public static class WebView2Behaviors
    {
        public static readonly DependencyProperty NavigateUriProperty =
            DependencyProperty.RegisterAttached(
                "NavigateUri",
                typeof(Uri),
                typeof(WebView2Behaviors),
                new PropertyMetadata(null, OnNavigateUriChanged));

        public static void SetNavigateUri(DependencyObject element, Uri? value) => element.SetValue(NavigateUriProperty, value);
        public static Uri? GetNavigateUri(DependencyObject element) => (Uri?)element.GetValue(NavigateUriProperty);

        private static async void OnNavigateUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Microsoft.Web.WebView2.Wpf.WebView2 web)
            {
                if (web.CoreWebView2 == null)
                {
                    await web.EnsureCoreWebView2Async();
                }
                var newUri = e.NewValue as Uri;
                if (newUri != null)
                    web.Source = newUri;   // Embed URL 로드
            }
        }

        public static readonly DependencyProperty NavigateHtmlProperty =
        DependencyProperty.RegisterAttached(
            "NavigateHtml",
            typeof(string),
            typeof(WebView2Behaviors),
            new PropertyMetadata(null, OnNavigateHtmlChanged));

        public static void SetNavigateHtml(DependencyObject element, string? value)
            => element.SetValue(NavigateHtmlProperty, value);

        public static string? GetNavigateHtml(DependencyObject element)
            => (string?)element.GetValue(NavigateHtmlProperty);

        private static async void OnNavigateHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Microsoft.Web.WebView2.Wpf.WebView2 web)
            {
                if (web.CoreWebView2 == null)
                    await web.EnsureCoreWebView2Async();

                var html = e.NewValue as string;
                if (!string.IsNullOrWhiteSpace(html))
                    web.NavigateToString(html);
                else
                    web.NavigateToString("<html><body style='margin:0'></body></html>");
            }
        }
    }
}
