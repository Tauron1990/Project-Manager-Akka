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

        internal ActorApplication(IContainer continer, ActorSystem actorSystem)
        {
            _actorApplication = this;
            Continer = continer;
            ActorSystem = actorSystem;
        }

        public static bool IsStarted => _actorApplication != null;

        public static ActorApplication Application => Argument.NotNull(_actorApplication, nameof(Application));

        public IContainer Continer { get; }
        public ActorSystem ActorSystem { get; }

        public void Dispose()
        {
            Continer.Dispose();
            ActorSystem.Dispose();
        }

        public static IApplicationBuilder Create(string[]? args = null)
        {
            var builder = new Builder();
            builder.UseContentRoot(Directory.GetCurrentDirectory());
            builder
                .ConfigureAkka(_
                    => ConfigurationFactory.ParseString(
                        " akka { loggers =[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]"))// \n  scheduler { implementation = \"Tauron.Akka.TimerScheduler, Tauron.Application.Common\" } }"))
                .ConfigureAutoFac(cb => cb.RegisterModule<CommonModule>())
                .Configuration(cb => { cb.AddEnvironmentVariables("DOTNET_"); })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    IHostEnvironment hostEnvironment = hostingContext.HostEnvironment;
                    var value = hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", true);
                    config.AddJsonFile("appsettings." + hostEnvironment.EnvironmentName + ".json", true, value);
                    config.AddEnvironmentVariables();
                    if (args != null)
                        config.AddCommandLine(args);
                })
                .Configuration(cb => cb.AddJsonFile("appsettings.json", true, true));

            return builder;
        }

        public async Task Run()
        {
            var lifeTime = Continer.Resolve<IHostLifetime>();
            var hostAppLifetime = (ApplicationLifetime) Continer.Resolve<IHostApplicationLifetime>();
            await using (hostAppLifetime.ApplicationStopping.Register(() => ActorSystem.Terminate()))
            {
                await lifeTime.WaitForStartAsync(ActorSystem);
                hostAppLifetime.NotifyStarted();

                ActorSystem.RegisterOnTermination(hostAppLifetime.NotifyStopped);
                await lifeTime.ShutdownTask;
                await Task.WhenAny(ActorSystem.WhenTerminated, Task.Delay(TimeSpan.FromSeconds(60)));
                Continer.Dispose();
                LogManager.Flush(TimeSpan.FromMinutes(1));
                LogManager.Shutdown();
            }
        }

        private sealed class Builder : IApplicationBuilder
        {
            private class ServiceCollection : IServiceCollection
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

            private readonly List<Action<HostBuilderContext, ActorSystem>> _actorSystemConfig = new();
            private readonly List<Func<HostBuilderContext, Config>> _akkaConfig = new();
            private readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _appConfigs = new();
            private readonly List<Action<IConfigurationBuilder>> _configurationBuilders = new();
            private readonly List<Action<ContainerBuilder>> _containerBuilder = new();
            private readonly List<Action<IServiceCollection>> _servicesList = new();
            private readonly List<Action<HostBuilderContext, ISetupBuilder>> _logger = new();

            public IApplicationBuilder ConfigureLogging(Action<HostBuilderContext, ISetupBuilder> config)
            {
                _logger.Add(config);
                return this;
            }

            public IApplicationBuilder Configuration(Action<IConfigurationBuilder> config)
            {
                _configurationBuilders.Add(config);
                return this;
            }

            public IApplicationBuilder ConfigureAutoFac(Action<ContainerBuilder> config)
            {
                _containerBuilder.Add(config);
                return this;
            }

            public IApplicationBuilder ConfigureServices(Action<IServiceCollection> config)
            {
                _servicesList.Add(config);
                return this;
            }

            public IApplicationBuilder ConfigureAppConfiguration(
                Action<HostBuilderContext, IConfigurationBuilder> config)
            {
                _appConfigs.Add(config);
                return this;
            }

            public IApplicationBuilder ConfigureAkka(Func<HostBuilderContext, Config> config)
            {
                _akkaConfig.Add(config);
                return this;
            }

            public IApplicationBuilder ConfigurateAkkaSystem(Action<HostBuilderContext, ActorSystem> system)
            {
                _actorSystemConfig.Add(system);
                return this;
            }

            public ActorApplication Build()
            {
                var config = CreateHostConfiguration();
                var hostingEnwiroment = CreateHostingEnvironment(config);
                var context = CreateHostBuilderContext(hostingEnwiroment, config);
                ConfigureLogging(context);
                config = BuildAppConfiguration(hostingEnwiroment, config, context);
                context.Configuration = config;
                var akkaConfig = CreateAkkaConfig(context);

                var provider = new ActorSystemProvider();
                var continer = CreateServiceProvider(hostingEnwiroment, context, config, provider.Get);

                var system = ActorSystem.Create(GetActorSystemName(context.Configuration, context.HostEnvironment), 
                    ActorSystemSetup.Create(
                        BootstrapSetup.Create().WithConfig(akkaConfig),
                        ServiceProviderSetup.Create(new AutofacServiceProvider(continer))));
                system.RegisterExtension(new ServiceProviderExtension());


                provider.System = system;

                foreach (var action in _actorSystemConfig)
                    action(context, system);

                return new ActorApplication(continer, system);
            }

            private static string GetActorSystemName(IConfiguration config, IHostEnvironment environment)
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

            private static IHostEnvironment CreateHostingEnvironment(IConfiguration hostConfiguration)
            {
                var hostingEnvironment = new HostEnviroment
                {
                    ApplicationName = hostConfiguration[HostDefaults.ApplicationKey],
                    EnvironmentName = hostConfiguration[HostDefaults.EnvironmentKey] ?? Environments.Production,
                    ContentRootPath = ResolveContentRootPath(hostConfiguration[HostDefaults.ContentRootKey],
                        AppContext.BaseDirectory)
                };
                if (string.IsNullOrEmpty(hostingEnvironment.ApplicationName))
                    hostingEnvironment.ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;

                return hostingEnvironment;
            }

            private IConfiguration BuildAppConfiguration(IHostEnvironment hostEnvironment,
                IConfiguration hostConfiguration, HostBuilderContext hostBuilderContext)
            {
                IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                    .SetBasePath(hostEnvironment.ContentRootPath)
                    .AddConfiguration(hostConfiguration, true);
                foreach (Action<HostBuilderContext, IConfigurationBuilder> configureAppConfigAction in _appConfigs)
                    configureAppConfigAction(hostBuilderContext, configurationBuilder);
                return configurationBuilder.Build();
            }

            private static HostBuilderContext CreateHostBuilderContext(IHostEnvironment environment,
                IConfiguration configuration) => new(new Dictionary<object, object>(), configuration, environment);

            private Config CreateAkkaConfig(HostBuilderContext context)
            {
                return _akkaConfig.Aggregate(Config.Empty, (current, func) => current.WithFallback(func(context)));
            }

            private IContainer CreateServiceProvider(IHostEnvironment hostEnvironment, HostBuilderContext hostBuilderContext, IConfiguration appConfiguration, Func<ActorSystem> actorSystem)
            {
                var containerBuilder = new ContainerBuilder();

                containerBuilder.Register(_ => actorSystem()).SingleInstance();
                containerBuilder.RegisterInstance(hostEnvironment);
                containerBuilder.RegisterInstance(hostBuilderContext);
                containerBuilder.RegisterInstance(appConfiguration);
                containerBuilder.RegisterType<ApplicationLifetime>().As<IHostApplicationLifetime, IApplicationLifetime>().SingleInstance();
                containerBuilder.RegisterType<CommonLifetime>().As<IHostLifetime>().SingleInstance();

                if (_servicesList.Count > 0)
                {
                    var serviceCollection = new ServiceCollection();
                    foreach (var action in _servicesList)
                        action(serviceCollection);

                    containerBuilder.Populate(serviceCollection);
                }

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