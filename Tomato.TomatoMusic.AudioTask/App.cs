using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Tomato.TomatoMusic.AudioTask
{
    class App : MinimalApplication
    {
        private WinRTContainer _container;
        private IEventAggregator _eventAggregator;

        public static App Current { get; private set; }

        public App()
        {
            LogManager.GetLog = type => new DebugLog(type);
        }

        public static void Startup()
        {
            var app = new App();
            Current = app;
            app.Initialize();
        }

        protected override void Configure()
        {
            _container = new WinRTContainer();
            _container.RegisterWinRTServices();

            _container.UseAudioTask();
            _container.UsePlugins();

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
    }
}
