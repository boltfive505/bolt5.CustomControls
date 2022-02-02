using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CefSharp.JavascriptBinding;
using CefSharp.Wpf;

namespace bolt5.CustomHtmlCefEditor
{
    [TemplatePart(Name = "PART_CefWebBrowser", Type = typeof(WebBrowser))]
    public class HtmlEditor : Control
    {
        private const string ELEMENT_CEFWEBBROWSER = "PART_CefWebBrowser";
        private ChromiumWebBrowser _cefWebBrowser;
        private ObjectForScriptingHelper _objectForScripting;
        private bool _isEditingFlag = false;
        private bool _isLoaded = false;

        public static readonly DependencyProperty HtmlContentProperty = DependencyProperty.Register(nameof(HtmlContent), typeof(string), typeof(HtmlEditor), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHtmlContentPropertyChanged)));
        public string HtmlContent
        {
            get { return (string)GetValue(HtmlContentProperty); }
            set { SetValue(HtmlContentProperty, value); }
        }

        public static readonly DependencyProperty IsPreviewProperty = DependencyProperty.Register(nameof(IsPreview), typeof(bool), typeof(HtmlEditor), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsPreviewPropertyChanged)));
        public bool IsPreview
        {
            get { return (bool)GetValue(IsPreviewProperty); }
            set { SetValue(IsPreviewProperty, value); }
        }

        public bool IsContentEmpty
        {
            get { return IsEditorAvailable() ? Convert.ToBoolean(InvokeScript("is_content_empty")) : true; }
        }

        public HtmlEditor()
        {
        }

        static HtmlEditor()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlEditor), new FrameworkPropertyMetadata(typeof(HtmlEditor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _cefWebBrowser = this.GetTemplateChild(ELEMENT_CEFWEBBROWSER) as ChromiumWebBrowser;
            string htmlFile = HtmlHelpers.ExtractWysiwygEditorFiles();
            LoadHtmlFile(htmlFile);
        }

        protected virtual void LoadHtmlFile(string htmlFile)
        {
            _objectForScripting = new ObjectForScriptingHelper();
            try
            {
                _cefWebBrowser.JavascriptObjectRepository.Register("boundAsync", _objectForScripting, true, null);
            }
            catch
            { }            
            _objectForScripting.ContentChange += _objectForScripting_ContentChange;
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
                Dispatcher.BeginInvoke(new Action(() => 
                {
                    _cefWebBrowser.ExecuteScriptAsyncWhenPageLoaded(@"(async function() {await CefSharp.BindObjectAsync('boundAsync');})();");
                    SetText(HtmlContent);
                    SetPreview(IsPreview);
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void JavascriptObjectRepository_ResolveObject(object sender, CefSharp.Event.JavascriptBindingEventArgs e)
        {
            if (e.ObjectName == "boundAsync")
            {
                BindingOptions bindingOptions = BindingOptions.DefaultBinder;
                e.ObjectRepository.Register("boundAsync", _objectForScripting, true, bindingOptions);
            }
        }

        private void _objectForScripting_ContentChange()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _isEditingFlag = true;
                string txt = GetText();
                SetValue(HtmlContentProperty, txt);
                _isEditingFlag = false;
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private static void OnHtmlContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is HtmlEditor)
            {
                HtmlEditor obj = sender as HtmlEditor;
                if (!obj._isEditingFlag && obj.IsEditorAvailable())
                {
                    obj.SetText((string)e.NewValue);
                }
            }
        }

        private static void OnIsPreviewPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is HtmlEditor)
            {
                HtmlEditor obj = sender as HtmlEditor;
                if (obj.IsEditorAvailable())
                {
                    obj.SetPreview((bool)e.NewValue);
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
            return Convert.ToString(InvokeScript("get_content_code"));
        }

        private void SetText(string html)
        {
            if (!IsEditorAvailable()) return;
            InvokeScript("set_content_code", html);
        }

        private void SetPreview(bool preview)
        {
            if (!IsEditorAvailable()) return;
            InvokeScript("show_toolbar", !preview);
            InvokeScript("set_enabled", !preview);
        }
    }
}
