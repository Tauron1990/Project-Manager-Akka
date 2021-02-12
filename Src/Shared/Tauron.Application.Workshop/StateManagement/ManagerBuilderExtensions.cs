using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Akka.Routing;
using Autofac;
using Autofac.Core;
using Autofac.Features.ResolveAnything;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.DataFactorys;
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
                param[0] = Buildhelper.GetParam(Buildhelper.Parameters[0], context,
                    () => context.Resolve(typeof(WorkspaceSuperviser)), supplyedParameters);
                param[1] = Buildhelper.GetParam(Buildhelper.Parameters[1], context, () => configAction,
                    supplyedParameters);

                return (Activator.CreateInstance(typeof(Buildhelper), param) as Buildhelper)
                    ?.Create(context, options) ?? throw new InvalidOperationException("Build helper Creation Failed");
            }).As<IActionInvoker>().SingleInstance();

            return builder;
        }

        public static ManagerBuilder AddFromAssembly<TType>(this ManagerBuilder builder, IDataSourceFactory factory,
            IComponentContext? context = null) => AddFromAssembly(builder, typeof(TType).Assembly, factory, context);

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, Assembly assembly,
            IDataSourceFactory factory, IComponentContext? context = null, CreationMetadata? metadata = null)
        {
            new ReflectionSearchEngine(Enumerable.Repeat(assembly, 1), context).Add(builder, factory, metadata);
            return builder;
        }

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, IComponentContext? context, CreationMetadata? metadata, IEnumerable<Assembly> assembly)
        {
            new ReflectionSearchEngine(assembly, context).Add(builder, factory, metadata);
            return builder;
        }

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder,
            IDataSourceFactory factory, IComponentContext? context, CreationMetadata? metadata, params Assembly[] assembly)
        {
            new ReflectionSearchEngine(assembly, context).Add(builder, factory, metadata);
            return builder;
        }

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder,
            IDataSourceFactory factory, IComponentContext? context, params Assembly[] assembly)
            => AddFromAssembly(builder, factory, context, null, assembly);

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder,
            IDataSourceFactory factory, CreationMetadata metadata, params Assembly[] assembly)
            => AddFromAssembly(builder, factory, null, metadata, assembly);

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder,
            IDataSourceFactory factory, params Assembly[] assembly)
            => AddFromAssembly(builder, factory, null, null, assembly);

        public static ManagerBuilder AddFromAssembly<TType>(this ManagerBuilder builder, IComponentContext context)
            => AddFromAssembly(builder, typeof(TType).Assembly, context);

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, IComponentContext? context, CreationMetadata? metadata, params Type[] assembly)
            => AddFromAssembly(builder, factory, context, metadata, assembly.Select(t => t.Assembly));

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, IComponentContext? context, params Type[] assembly)
            => AddFromAssembly(builder, factory, context, null, assembly.Select(t => t.Assembly));

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata, params Type[] assembly)
            => AddFromAssembly(builder, factory, null, metadata, assembly.Select(t => t.Assembly));

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, IDataSourceFactory factory, params Type[] assembly)
            => AddFromAssembly(builder, factory, null, null, assembly.Select(t => t.Assembly));

        public static ManagerBuilder AddFromAssembly(this ManagerBuilder builder, Assembly assembly, IComponentContext context)
            => AddFromAssembly(builder, assembly,
                MergeFactory.Merge(context.Resolve<IEnumerable<IDataSourceFactory>>().Cast<AdvancedDataSourceFactory>()
                    .ToArray()), context);

        public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, IComponentContext? context, CreationMetadata? metadata, IEnumerable<Type> types)
        {
            new ReflectionSearchEngine(types, context).Add(builder, factory, metadata);
            return builder;
        }

        public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, IComponentContext? context, CreationMetadata? metadata, params Type[] types)
            => AddTypes(builder, factory, context, metadata, (IEnumerable<Type>)types);

        public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, IComponentContext? context, params Type[] types)
            => AddTypes(builder, factory, context, null, types);

        public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata metadata, params Type[] types)
            => AddTypes(builder, factory, null, metadata, types);

        public static ManagerBuilder AddTypes(this ManagerBuilder builder, IDataSourceFactory factory, params Type[] assembly)
            => AddTypes(builder, factory, null, null, assembly);

        public static IConcurrentDispatcherConfugiration WithConcurentDispatcher(this ManagerBuilder builder)
        {
            var config = new ConcurrentDispatcherConfugiration();
            builder.WithDispatcher(config.Create);

            return config;
        }

        public static IConsistentHashDispatcherPoolConfiguration WithConsistentHashDispatcher(
            this ManagerBuilder builder)
        {
            var config = new ConsistentHashDispatcherConfiguration();
            builder.WithDispatcher(config.Create);

            return config;
        }

        public static TConfig WithDefaultConfig<TConfig>(this IDispatcherPoolConfiguration<TConfig> config)
            where TConfig : IDispatcherPoolConfiguration<TConfig>
            => config
                .NrOfInstances(2)
                .WithResizer(new DefaultResizer(2, 10));

        private sealed class Buildhelper
        {
            public static readonly ParameterInfo[] Parameters =
                typeof(Buildhelper).GetConstructors().First().GetParameters();

            public Buildhelper(WorkspaceSuperviser superviser, Action<ManagerBuilder, IComponentContext> action)
            {
                Superviser = superviser;
                Action = action;
            }

            private WorkspaceSuperviser Superviser { get; }
            private Action<ManagerBuilder, IComponentContext> Action { get; }

            public static object GetParam(ParameterInfo info, IComponentContext context, Func<object> alternative,
                IEnumerable<Parameter> param)
            {
                Func<object?>? factory = null;

                foreach (var parameter in param)
                    if (parameter.CanSupplyValue(info, context, out factory))
                        break;

                factory ??= alternative;

                return factory() ?? alternative();
            }

            public RootManager Create(IComponentContext context, AutofacOptions autofacOptions)
            {
                var config = new ManagerBuilder(Superviser);
                Action(config, context);

                return config.Build(context, autofacOptions);
            }
        }
    }
}