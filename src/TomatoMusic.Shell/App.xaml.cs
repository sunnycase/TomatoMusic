using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Caliburn.Micro;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TomatoMusic.Shell.Infrastructure;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

namespace TomatoMusic.Shell
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// 获取服务提供程序
        /// </summary>
        public IServiceProvider ServiceProvider { get; private set; }

        private IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            Configuration = LoadConfiguration();
            InitializeComponent();
        }

        private IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Package.Current.InstalledLocation.Path)
                .AddJsonFile("config.json", optional: true)
                .Build();
        }

        /// <inheritdoc/>
        protected override void Configure()
        {
            var services = new ServiceCollection();
            ServiceProvider = ConfigureServices(services);
            ConfigureCaliburnLogging();

            var t = ViewLocator.GetOrCreateViewType;
        }

        private void ConfigureCaliburnLogging()
        {
            LogManager.GetLog = type =>
            {
                var logger = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(type);
                return new CaliburnLogger(logger);
            };
        }

        private IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddSingleton(ConfigureLogging());
            services.AddLogging();
            services.AddCaliburn();

            var container = new ContainerBuilder();
            container.Populate(services);
            container.RegisterAssemblyModules(AssemblySource.Instance.ToArray());
            _container = container.Build();
            return new AutofacServiceProvider(_container);
        }

        /// <inheritdoc/>
        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new List<Assembly>()
                .AddApplicationPartShell()
                .AddApplicationPartPlayer()
                .AddApplicationPartPlaylist();
        }

        private static ILoggerFactory ConfigureLogging()
        {
            var factory = new LoggerFactory();
            factory.AddDebug();

            return factory;
        }

        /// <inheritdoc/>
        protected override object GetInstance(Type service, string key)
        {
            return key == null ? _container.Resolve(service) : _container.ResolveNamed(key, service);
        }

        /// <inheritdoc/>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(service);

            return _container.Resolve(type) as IEnumerable<object>;
        }

        /// <inheritdoc/>
        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }

        /// <inheritdoc/>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
                return;

            DisplayRootViewFor<ViewModels.ShellViewModel>();
        }
    }
}
