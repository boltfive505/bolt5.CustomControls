using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.Wpf;

namespace bolt5.CustomHtmlCefEditor
{
    [TemplatePart(Name = "PART_CefWebBrowser", Type = typeof(WebBrowser))]
    public class HtmlPreview : Control
    {
        private const string ELEMENT_CEFWEBBROWSER = "PART_CefWebBrowser";
        private ChromiumWebBrowser _cefWebBrowser;
        private bool _isLoaded = false;

        public static readonly DependencyProperty HtmlContentProperty = DependencyProperty.Register(nameof(HtmlContent), typeof(string), typeof(HtmlPreview), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHtmlContentPropertyChanged)));
        public string HtmlContent
        {
            get { return (string)GetValue(HtmlContentProperty); }
            set { SetValue(HtmlContentProperty, value); }
        }

        public bool IsContentEmpty
        {
            get { return IsEditorAvailable() ? Convert.ToBoolean(InvokeScript("is_content_empty")) : true; }
        }

        public HtmlPreview()
        {
            
        }

        static HtmlPreview()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlPreview), new FrameworkPropertyMetadata(typeof(HtmlPreview)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _cefWebBrowser = this.GetTemplateChild(ELEMENT_CEFWEBBROWSER) as ChromiumWebBrowser;
            string htmlFile = HtmlHelpers.ExtractSimplePreviewFiles();
            LoadHtmlFile(htmlFile);
        }

        protected virtual void LoadHtmlFile(string htmlFile)
        {
            _cefWebBrowser.LoadingStateChanged += _cefWebBrowser_LoadingStateChanged;
            _cefWebBrowser.Address = htmlFile;
        }

        private void _cefWebBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            if (!e.IsLoading)
            {
                //done loading page
                _cefWebBrowser.LoadingStateChanged -= _cefWebBrowser_LoadingStateChanged;
                _isLoaded = true;
                Dispatcher.BeginInvoke(new Action(() => SetText(HtmlContent)), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private static void OnHtmlContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is HtmlPreview)
            {
                HtmlPreview obj = sender as HtmlPreview;
                if(obj.IsEditorAvailable())
                {
                    obj.SetText((string)e.NewValue);
                }
            }
        }

        protected virtual bool IsEditorAvailable()
        {
            return _cefWebBrowser != null && _isLoaded;
        }

        protected virtual object InvokeScript(string methodName, params object[] args)
        {
            var t = _cefWebBrowser.EvaluateScriptAsync(methodName, args);
            t.Wait();
            return t.Result.Result;
        }

        private string GetText()
        {
            if (!IsEditorAvailable()) return string.Empty;
            return Convert.ToString(InvokeScript("get_content"));
        }

        private void SetText(string html)
        {
            if (!IsEditorAvailable()) return;
            InvokeScript("set_content", html);
        }
    }
}
