using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Autofac;
using Autofac.Core;
using Autofac.Features.ResolveAnything;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.DataFactorys;
using Tauron.Application.Workshop.StateManagement.Dispatcher.WorkDistributor;
using Tauron.Application.Workshop.StateManagement.Internal;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public static class ManagerBuilderExtensions
    {
        public static ContainerBuilder RegisterStateManager(this ContainerBuilder builder, Action<ManagerBuilder, IComponentContext> configAction)
            => RegisterStateManager(builder, new AutofacOptions(), configAction);

        public static ContainerBuilder RegisterStateManager(this ContainerBuilder builder, bool registerWorkspaceSuperviser, Action<ManagerBuilder, IComponentContext> configAction)
            => RegisterStateManager(builder, new AutofacOptions {RegisterSuperviser = registerWorkspaceSuperviser},
                configAction);

        public static ContainerBuilder RegisterStateManager(this ContainerBuilder builder, AutofacOptions options, Action<ManagerBuilder, IComponentContext> configAction)
        {
            static bool ImplementInterface(Type target, Type interfac) => target.GetInterface(interfac.Name) != null;

            if (options.AutoRegisterInContainer)
                builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(
                    t => t.IsAssignableTo<IState>() || t.IsAssignableTo<IEffect>() ||
                         t.IsAssignableTo<IMiddleware>() || ImplementInterface(t, typeof(IReducer<>))));

            if (options.RegisterSuperviser)
                builder.Register(c => new WorkspaceSuperviser(c.Resolve<ActorSystem>(), "State_Manager_Superviser"))
                    .AsSelf().SingleInstance();

            builder.Register((context, parameters) =>
            {
                var supplyedParameters = parameters.ToArray();
                object[] param = new object[2];
                param[0] = Buildhelper.GetParam(Buildhelper.Parameters[0], context, () => context.Resolve(typeof(WorkspaceSuperviser)), supplyedParameters);
                param[1] = Buildhelper.GetParam(Buildhelper.Parameters[1], context, () => configAction, supplyedParameters);

                return (Activator.CreateInstance(typeof(Buildhelper), param) as Buildhelper)
                    ?.Create(context, options) ?? throw new InvalidOperationException("Build helper Creation Failed");
            }).As<IActionInvoker>().SingleInstance();

            return builder;
        }

        public static ManagerBuilder AddFromAssembly<TType>(this ManagerBuilder builder, IDataSourceFactory factory) 
            => AddFromAssembly(builder, typeof(TType).Assembly, factory);

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, Assembly assembly, IDataSourceFactory factory, CreationMetadata? metadata = null)
        {
            new ReflectionSearchEngine(Enumerable.Repeat(assembly, 1)).Add(builder, factory, metadata);
            return builder;
        }

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, IEnumerable<Assembly> assembly)
        {
            new ReflectionSearchEngine(assembly).Add(builder, factory, metadata);
            return builder;
        }
        

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, params Assembly[] assembly)
            => AddFromAssembly(builder, factory, metadata, assembly.AsEnumerable());

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, params Assembly[] assembly)
            => AddFromAssembly(builder, factory, null, assembly);

        public static ManagerBuilder AddFromAssembly<TType>(this ManagerBuilder builder, IComponentContext context)
            => AddFromAssembly(builder, typeof(TType).Assembly, context);

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, params Type[] assembly)
            => AddFromAssembly(builder, factory, metadata, assembly.Select(t => t.Assembly));

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, params Type[] assembly)
            => AddFromAssembly(builder, factory, null, assembly.Select(t => t.Assembly));


        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, Assembly assembly, IComponentContext context)
            => AddFromAssembly(builder, assembly,
                MergeFactory.Merge(context.Resolve<IEnumerable<IDataSourceFactory>>().Cast<AdvancedDataSourceFactory>()
                    .ToArray()));

        public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, IEnumerable<Type> types)
        {
            new ReflectionSearchEngine(types).Add(builder, factory, metadata);
            return builder;
        }

        public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, params Type[] types)
            => AddTypes(builder, factory, metadata, (IEnumerable<Type>)types);

        public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, params Type[] types)
            => AddTypes(builder, factory, null, types);

        public static IConcurrentDispatcherConfugiration WithConcurentDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder) 
            where TReturn : IDispatcherConfigurable<TReturn>
        {
            var config = new ConcurrentDispatcherConfugiration();
            builder.WithDispatcher(config.Create);

            return config;
        }

        public static IConsistentHashDispatcherPoolConfiguration WithConsistentHashDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder)
            where TReturn : IDispatcherConfigurable<TReturn>
        {
            var config = new ConsistentHashDispatcherConfiguration();
            builder.WithDispatcher(config.Create);

            return config;
        }

        public static TType WithWorkDistributorDispatcher<TType>(this TType source, TimeSpan? timeout = null)
            where TType : IDispatcherConfigurable<TType>
        {
            timeout ??= TimeSpan.FromMinutes(1);

            return source.WithDispatcher(() => new WorkDistributorConfigurator(timeout.Value));
        }

        public static IConsistentHashDispatcherPoolConfiguration WithConsistentHashDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder, string name)
            where TReturn : IDispatcherConfigurable<TReturn>
        {
            var config = new ConsistentHashDispatcherConfiguration();
            builder.WithDispatcher(name, config.Create);

            return config;
        }

        public static TType WithWorkDistributorDispatcher<TType>(this TType source, string name, TimeSpan? timeout = null)
            where TType : IDispatcherConfigurable<TType>
        {
            timeout ??= TimeSpan.FromMinutes(1);

            return source.WithDispatcher(name, () => new WorkDistributorConfigurator(timeout.Value));
        }


        public static IConcurrentDispatcherConfugiration WithConcurentDispatcher<TReturn>(this IDispatcherConfigurable<TReturn> builder, string name)
            where TReturn : IDispatcherConfigurable<TReturn>
        {
            var config = new ConcurrentDispatcherConfugiration();
            builder.WithDispatcher(name, config.Create);

            return config;
        }

        private sealed class Buildhelper
        {
            internal static readonly ParameterInfo[] Parameters =
                typeof(Buildhelper).GetConstructors().First().GetParameters();

            internal Buildhelper(WorkspaceSuperviser superviser, Action<ManagerBuilder, IComponentContext> action)
            {
                Superviser = superviser;
                Action = action;
            }

            private WorkspaceSuperviser Superviser { get; }
            private Action<ManagerBuilder, IComponentContext> Action { get; }

            internal static object GetParam(ParameterInfo info, IComponentContext context, Func<object> alternative,
                IEnumerable<Parameter> param)
            {
                Func<object?>? factory = null;

                foreach (var parameter in param)
                    if (parameter.CanSupplyValue(info, context, out factory))
                        break;

                factory ??= alternative;

                return factory() ?? alternative();
            }

            internal RootManager Create(IComponentContext context, AutofacOptions autofacOptions)
            {
                var config = new ManagerBuilder(Superviser)
                             {
                                 ComponentContext = context
                             };

                Action(config, context);

                return config.Build(autofacOptions);
            }
        }
    }
}