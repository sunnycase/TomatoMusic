using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using Tomato.TomatoMusic.Shell.Views;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Tomato.TomatoMusic.Shell
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : CaliburnApplication
    {
        private WinRTContainer _container;
        private IEventAggregator _eventAggregator;
        private ILog _logger;

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            LogManager.GetLog = type => new DebugLog(type);
            _logger = LogManager.GetLog(typeof(App));

            Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
                Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
                Microsoft.ApplicationInsights.WindowsCollectors.Session |
                Microsoft.ApplicationInsights.WindowsCollectors.UnhandledException);
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            this.UnhandledException += App_UnhandledException;
            this.InitializeComponent();
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            _logger.Error(e.Exception);
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var client = new Microsoft.ApplicationInsights.TelemetryClient();
            client.TrackException(e.Exception.Flatten());
            _logger.Error(e.Exception);
            e.SetObserved();
        }

        protected override void Configure()
        {
            var config = LoadConfig();
            _container = new WinRTContainer();
            _container.RegisterWinRTServices();

            _container.UseShell();
            _container.UseAudio();
            _container.UsePlaylist();
            _container.UsePlugins()
                .AddLastFm(config.LastFmApiKey);
            _eventAggregator = _container.GetInstance<IEventAggregator>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            _container.RegisterNavigationService(rootFrame);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            DisplayRootView<MainView>();
            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //_eventAggregator.PublishOnUIThread(new ResumeStateMessage());
            }
        }

        static Config LoadConfig()
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "config.json")));
        }

        class Config
        {
            public string LastFmApiKey { get; set; }
        }

        class MyLogger : ILog
        {
            private readonly DebugLog _debug;
            private static StreamWriter _file = new StreamWriter(new FileStream(Path.Combine(Windows.Storage.ApplicationData.Current.LocalCacheFolder.Path,
                    $"log{DateTime.Today.ToString("yyyy-MM-dd")}.log"), FileMode.Append))
            {
                AutoFlush = true
            };

            public MyLogger(Type type)
            {
                _debug = new DebugLog(type);
            }

            public void Error(Exception exception)
            {
                _debug.Error(exception);
                _file.WriteLine(exception.Flatten());
                _file.WriteLine(exception.Source);
                _file.WriteLine(exception.StackTrace);
            }

            public void Info(string format, params object[] args)
            {
                _debug.Info(format, args);
            }

            public void Warn(string format, params object[] args)
            {
                _debug.Warn(format, args);
            }
        }
    }
}
