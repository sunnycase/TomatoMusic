using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Tomato.TomatoMusic.AudioTask;
using Microsoft.HockeyApp;
using Tomato.TomatoMusic.Messages;
using Windows.ApplicationModel;

namespace Tomato.TomatoMusic.Shell
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : CaliburnApplication
    {
        private WinRTContainer _container;
        private IEventAggregator _eventAggregator;
        private Caliburn.Micro.ILog _logger;

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            LogManager.GetLog = type => new DebugLog(type);
            _logger = LogManager.GetLog(typeof(App));

            HockeyClient.Current.Configure("d6df9da09fd74e0a8df588ba0afffa56", new TelemetryConfiguration
            {
                Collectors = WindowsCollectors.UnhandledException | WindowsCollectors.Metadata |
                 WindowsCollectors.Session
            });
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            this.UnhandledException += App_UnhandledException;
            this.InitializeComponent();
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            HockeyClient.Current.TrackEvent(e.Exception.Flatten());
            _logger.Error(e.Exception);
            e.Handled = true;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            HockeyClient.Current.TrackEvent(ExceptionExtensions.Flatten(e.Exception.Flatten()));
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
                .AddLastFm(config.LastFmApiKey)
                .AddLocalLyrics();
            _eventAggregator = _container.GetInstance<IEventAggregator>();
            ViewModelBinder.ApplyConventionsByDefault = false;
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

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PrelaunchActivated) return;
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            DisplayRootViewFor<ViewModels.MainViewModel>();
            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                _eventAggregator.PublishOnUIThread(new ResumeStateMessage());
            }
        }

        protected override void OnResuming(object sender, object e)
        {
            _eventAggregator.PublishOnUIThread(new ResumeStateMessage());
        }

        protected override async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await _eventAggregator.PublishOnUIThreadAsync(new SuspendStateMessage());
            deferral.Complete();
        }

        static Config LoadConfig()
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "config.json")));
        }

        class Config
        {
            public string LastFmApiKey { get; set; }
        }

        class MyLogger : Caliburn.Micro.ILog
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
