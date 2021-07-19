using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using Akka.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Config;

namespace Tauron.Host
{
    [PublicAPI]
    public sealed class ActorApplication : IDisposable
    {
        private static ActorApplication? _actorApplication;

        internal ActorApplication(IContainer container, ActorSystem actorSystem)
        {
            _actorApplication = this;
            Container = container;
            ActorSystem = actorSystem;
        }

        public static bool IsStarted => _actorApplication != null;

        public static ActorApplication Application => Argument.NotNull(_actorApplication, nameof(Application));

        public IContainer Container { get; }
        public ActorSystem ActorSystem { get; }

        public void Dispose()
        {
            Container.Dispose();
            ActorSystem.Dispose();
        }

        public static IActorApplicationBuilder Create(IServiceCollection collection)
            => new Builder(collection).ConfigureAkka(_
                                                         => ConfigurationFactory.ParseString(
                                                             " akka { loggers =[\"Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog\"]")) // \n  scheduler { implementation = \"Tauron.Akka.TimerScheduler, Tauron.Application.Common\" } }"))
                                      .ConfigureAutoFac(cb => cb.RegisterModule<CommonModule>());

        public static IActorApplicationBuilder Create(string[]? args = null)
            => new Builder()
              .UseContentRoot(Directory.GetCurrentDirectory())
              .ConfigureAkka(_
                                 => ConfigurationFactory.ParseString(
                                     " akka { loggers =[\"Akka.Logger.NLog.NLogLogger, Akka.Logger.NLog\"]")) // \n  scheduler { implementation = \"Tauron.Akka.TimerScheduler, Tauron.Application.Common\" } }"))
              .ConfigureAutoFac(cb => cb.RegisterModule<CommonModule>())
              .Configuration(cb => { cb.AddEnvironmentVariables("DOTNET_"); })
              .ConfigureAppConfiguration((hostingContext, config) =>
                                         {
                                             IActorHostEnvironment hostEnvironment = hostingContext.HostEnvironment;
                                             var value = hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", true);
                                             config.AddJsonFile("appsettings." + hostEnvironment.EnvironmentName + ".json", true, value);
                                             config.AddEnvironmentVariables();
                                             if (args != null)
                                                 config.AddCommandLine(args);
                                         })
              .Configuration(cb => cb.AddJsonFile("appsettings.json", true, true));

        public async Task Run(bool shutdown = true)
        {
            var lifeTime = Container.Resolve<IHostLifetime>();
            var hostAppLifetime = (ApplicationLifetime) Container.Resolve<IActorApplicationLifetime>();
            await using (hostAppLifetime.ApplicationStopping.Register(() => ActorSystem.Terminate()))
            {
                await lifeTime.WaitForStartAsync(ActorSystem);
                hostAppLifetime.NotifyStarted();

                ActorSystem.RegisterOnTermination(hostAppLifetime.NotifyStopped);
                await lifeTime.ShutdownTask;
                await Task.WhenAny(ActorSystem.WhenTerminated, Task.Delay(TimeSpan.FromSeconds(60)));

                if (shutdown)
                {
                    Container.Dispose();
                    LogManager.Flush(TimeSpan.FromMinutes(1));
                    LogManager.Shutdown();
                }
            }
        }

        private sealed class Builder : IActorApplicationBuilder
        {
            private sealed class ServiceCollection : IServiceCollection
            {
                private readonly List<ServiceDescriptor> _descriptors = new();
                
                public int Count => _descriptors.Count;
                
                public bool IsReadOnly => false;
                
                public ServiceDescriptor this[int index]
                {
                    get => _descriptors[index];
                    set => _descriptors[index] = value;
                }
                
                public void Clear() => _descriptors.Clear();
                
                public bool Contains(ServiceDescriptor item) => _descriptors.Contains(item);
                
                public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _descriptors.CopyTo(array, arrayIndex);
                
                public bool Remove(ServiceDescriptor item) => _descriptors.Remove(item);

                public IEnumerator<ServiceDescriptor> GetEnumerator() => _descriptors.GetEnumerator();

                void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item) => _descriptors.Add(item);

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
                
                public int IndexOf(ServiceDescriptor item) => _descriptors.IndexOf(item);
                
                public void Insert(int index, ServiceDescriptor item) => _descriptors.Insert(index, item);
                
                public void RemoveAt(int index) => _descriptors.RemoveAt(index);
            }

            //private sealed class ChangeTokenCombine : IChangeToken
            //{
            //    private readonly IChangeToken _first;
            //    private readonly IChangeToken _secund;

            //    public ChangeTokenCombine(IChangeToken first, IChangeToken secund)
            //    {
            //        _first = first;
            //        _secund = secund;

                    
            //    }

            //    public IDisposable RegisterChangeCallback(Action<object> callback, object state)
            //    {
            //        var first = _first.RegisterChangeCallback(callback, state);
            //        var secund = _secund.RegisterChangeCallback(callback, state);

            //        return Disposable.Create(() =>
            //                                 {
            //                                     first.Dispose();
            //                                     secund.Dispose();
            //                                 });
            //    }

            //    public bool HasChanged => _first.HasChanged || _secund.HasChanged;

            //    public bool ActiveChangeCallbacks => _first.ActiveChangeCallbacks || _secund.ActiveChangeCallbacks;
                
            //}

            //private sealed class LazyConfig
            //{
            //    private IConfiguration? _configuration;

            //    public IConfiguration Value
            //    {
            //        get
            //        {
            //            if (_configuration != null) return _configuration;
            //            if (!CanGet)
            //                throw new InvalidOperationException();

            //            return _configuration ??= Application.Container.Resolve<IEnumerable<IConfiguration>>().First();
            //        }
            //    }

            //    public bool CanGet => _actorApplication != null;
            //}

            //private sealed class ConfigurationWrapper : IConfiguration
            //{
            //    private readonly IConfiguration _first;
            //    private readonly LazyConfig _secund;

            //    public ConfigurationWrapper(IConfiguration first)
            //    {
            //        _first = first;
            //        _secund = new LazyConfig();
            //    }

            //    public IConfigurationSection GetSection(string key)
            //    {
            //        var temp = _first.GetSection(key);
            //        return string.IsNullOrWhiteSpace(temp.Value) && _secund.CanGet ? _secund.Value.GetSection(key) : temp;
            //    }

            //    public IEnumerable<IConfigurationSection> GetChildren() 
            //        => _secund.CanGet ? _first.GetChildren().Concat(_secund.Value.GetChildren()) : _first.GetChildren();

            //    public IChangeToken GetReloadToken() 
            //        => _secund.CanGet ? new ChangeTokenCombine(_first.GetReloadToken(), _secund.Value.GetReloadToken()) : _first.GetReloadToken();

            //    public string this[string key]
            //    {
            //        get
            //        {
            //            var temp = _first[key];
            //            return string.IsNullOrWhiteSpace(temp) && _secund.CanGet ? _secund.Value[key] : temp;
            //        }
            //        set
            //        {
            //            _first[key] = value;
            //            _secund.Value[key] = value;
            //        }
            //    }
            //}

            private readonly List<Action<HostBuilderContext, ActorSystem>> _actorSystemConfig = new();
            private readonly List<Func<HostBuilderContext, Config>> _akkaConfig = new();
            private readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _appConfigs = new();
            private readonly List<Action<IConfigurationBuilder>> _configurationBuilders = new();
            private readonly List<Action<ContainerBuilder>> _containerBuilder = new();
            private readonly List<Action<IServiceCollection>> _servicesList = new();
            private readonly List<Action<HostBuilderContext, ISetupBuilder>> _logger = new();
            private readonly List<Func<IActorHostEnvironment, IActorHostEnvironment>> _hostEnviromentUpdater = new();
            private readonly IServiceCollection _serviceCollection;

            public Builder(IServiceCollection serviceCollection) => _serviceCollection = serviceCollection;

            public Builder() => _serviceCollection = new ServiceCollection();

            public IActorApplicationBuilder ConfigureLogging(Action<HostBuilderContext, ISetupBuilder> config)
            {
                _logger.Add(config);
                return this;
            }

            public IActorApplicationBuilder Configuration(Action<IConfigurationBuilder> config)
            {
                _configurationBuilders.Add(config);
                return this;
            }

            public IActorApplicationBuilder ConfigureAutoFac(Action<ContainerBuilder> config)
            {
                _containerBuilder.Add(config);
                return this;
            }

            public IActorApplicationBuilder ConfigureServices(Action<IServiceCollection> config)
            {
                _servicesList.Add(config);
                return this;
            }

            public IActorApplicationBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> config)
            {
                _appConfigs.Add(config);
                return this;
            }

            public IActorApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config)
            {
                _akkaConfig.Add(config);
                return this;
            }

            public IActorApplicationBuilder ConfigurateAkkaSystem(Action<HostBuilderContext, ActorSystem> system)
            {
                _actorSystemConfig.Add(system);
                return this;
            }

            public IActorApplicationBuilder UpdateEnviroment(Func<IActorHostEnvironment, IActorHostEnvironment> updater)
            {
                _hostEnviromentUpdater.Add(updater);
                return this;
            }

            public ActorApplication Build(ConfigurationOptions configuration = ConfigurationOptions.None)
            {
                var config = CreateHostConfiguration();
                var hostingEnwiroment = CreateHostingEnvironment(config);
                var context = CreateHostBuilderContext(hostingEnwiroment, config);
                ConfigureLogging(context);
                config = BuildAppConfiguration(hostingEnwiroment, config, context);
                context.Configuration = config;
                var akkaConfig = CreateAkkaConfig(context);

                var provider = new ActorSystemProvider();
                var continer = CreateServiceProvider(hostingEnwiroment, context, config, configuration, provider.Get);

                var system = ActorSystem.Create(GetActorSystemName(context.Configuration, context.HostEnvironment), 
                    ActorSystemSetup.Create(
                        BootstrapSetup.Create().WithConfig(akkaConfig),
                        DependencyResolverSetup.Create(new AutofacServiceProvider(continer))));
                system.RegisterExtension(new DependencyResolverExtension());


                provider.System = system;

                foreach (var action in _actorSystemConfig)
                    action(context, system);

                return new ActorApplication(continer, system);
            }

            private static string GetActorSystemName(IConfiguration config, IActorHostEnvironment environment)
            {
                var name = config["actorsystem"];
                return !string.IsNullOrWhiteSpace(name)
                    ? name
                    : environment.ApplicationName.Replace('.', '-');
            }

            private void ConfigureLogging(HostBuilderContext context)
            {
                var config = LogManager.Setup();

                foreach (var action in _logger)
                    action(context, config);

                LogManager.ReconfigExistingLoggers();
            }

            private IConfiguration CreateHostConfiguration()
            {
                var builder = new ConfigurationBuilder().AddInMemoryCollection();
                foreach (var action in _configurationBuilders)
                    action(builder);

                return builder.Build();
            }

            private IActorHostEnvironment CreateHostingEnvironment(IConfiguration hostConfiguration)
            {
                IActorHostEnvironment hostingEnvironment = new ActorHostEnviroment
                {
                    ApplicationName = hostConfiguration[HostDefaults.ApplicationKey],
                    EnvironmentName = hostConfiguration[HostDefaults.EnvironmentKey] ?? Environments.Production,
                    ContentRootPath = ResolveContentRootPath(hostConfiguration[HostDefaults.ContentRootKey], AppContext.BaseDirectory)
                };

                hostingEnvironment = _hostEnviromentUpdater.Aggregate(hostingEnvironment, (current, updater) => updater(current));

                if (string.IsNullOrEmpty(hostingEnvironment.ApplicationName))
                    hostingEnvironment.ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;

                return hostingEnvironment;
            }

            private IConfiguration BuildAppConfiguration(IActorHostEnvironment hostEnvironment, IConfiguration hostConfiguration, HostBuilderContext hostBuilderContext)
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(hostEnvironment.ContentRootPath)
                    .AddConfiguration(hostConfiguration, true);
                foreach (Action<HostBuilderContext, IConfigurationBuilder> configureAppConfigAction in _appConfigs)
                    configureAppConfigAction(hostBuilderContext, configurationBuilder);
                return configurationBuilder.Build();
            }

            private static HostBuilderContext CreateHostBuilderContext(IActorHostEnvironment environment,
                IConfiguration configuration) => new(new Dictionary<object, object>(), configuration, environment);

            private Config CreateAkkaConfig(HostBuilderContext context)
            {
                return _akkaConfig.Aggregate(Config.Empty, (current, func) => current.WithFallback(func(context)));
            }

            private IContainer CreateServiceProvider(IActorHostEnvironment hostEnvironment, HostBuilderContext hostBuilderContext, IConfiguration appConfiguration, 
                ConfigurationOptions configurationOptions, Func<ActorSystem> actorSystem)
            {
                var containerBuilder = new ContainerBuilder();

                if (_servicesList.Count > 0 || _serviceCollection.Count > 0)
                {
                    foreach (var action in _servicesList)
                        action(_serviceCollection);

                    containerBuilder.Populate(_serviceCollection);
                }

                containerBuilder.Register(_ => actorSystem()).SingleInstance();
                containerBuilder.RegisterInstance(hostEnvironment);
                containerBuilder.RegisterInstance(hostBuilderContext);
                if (configurationOptions == ConfigurationOptions.None)
                    containerBuilder.RegisterInstance(appConfiguration);
                else
                    containerBuilder.RegisterDecorator<IConfiguration>((_, _, configuration)
                                                                           => new ConfigurationBuilder()
                                                                             .AddConfiguration(configuration, true)
                                                                             .AddConfiguration(appConfiguration, true)
                                                                             .Build());
                containerBuilder.RegisterType<ApplicationLifetime>().As<IActorApplicationLifetime, IApplicationLifetime>().SingleInstance();
                containerBuilder.RegisterType<CommonLifetime>().As<IHostLifetime>().SingleInstance();

                foreach (var action in _containerBuilder)
                    action(containerBuilder);

                return containerBuilder.Build();
            }

            private static string ResolveContentRootPath(string contentRootPath, string basePath)
            {
                if (string.IsNullOrEmpty(contentRootPath)) return basePath;
                return Path.IsPathRooted(contentRootPath)
                    ? contentRootPath
                    : Path.Combine(Path.GetFullPath(basePath), contentRootPath);
            }

            private sealed class ActorSystemProvider
            {
                public ActorSystem? System { get; set; }

                public ActorSystem Get()
                {
                    if (System == null)
                        throw new InvalidOperationException("No ActorSystem Configured");

                    return System;
                }
            }
        }
    }
}