using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using FluentValidation;
using JetBrains.Annotations;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.StateManagement.Attributes;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.Workshop.StateManagement.Internal
{
    public class ReflectionSearchEngine
    {
        private static readonly MethodInfo ConfigurateStateMethod =
            typeof(ReflectionSearchEngine).GetMethod(nameof(ConfigurateState),
                BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Method not Found");

        private readonly IEnumerable<Type> _types;
        private readonly IComponentContext? _context;

        public ReflectionSearchEngine(IEnumerable<Assembly> types, IComponentContext? context)
        {
            _types = types.SelectMany(asm => asm.GetTypes());
            _context = context;
        }

        public ReflectionSearchEngine(IEnumerable<Type> types, IComponentContext? context)
        {
            _types = types;
            _context = context;
        }

        public void Add(ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata)
        {
            Func<TType> CreateFactory<TType>(Type target)
            {
                if (_context != null)
                    return () => (TType) (_context.ResolveOptional(target) ?? Activator.CreateInstance(target))!;
                return () => (TType) Activator.CreateInstance(target)!;
            }

            var states = new List<(Type, string?)>();
            var reducers = new GroupDictionary<Type, Type>();
            var factorys = new List<AdvancedDataSourceFactory>();
            var processors = new List<Type>();

            foreach (var type in _types)
            foreach (var customAttribute in type.GetCustomAttributes(false))
                switch (customAttribute)
                {
                    case StateAttribute state:
                        states.Add((type, state.Key));
                        break;
                    case EffectAttribute:
                        builder.WithEffect(CreateFactory<IEffect>(type));
                        break;
                    case MiddlewareAttribute:
                        builder.WithMiddleware(CreateFactory<IMiddleware>(type));
                        break;
                    case BelogsToStateAttribute belogsTo:
                        reducers.Add(belogsTo.StateType, type);
                        break;
                    case DataSourceAttribute:
                        factorys.Add((AdvancedDataSourceFactory) (_context?.ResolveOptional(type)
                                                                  ?? Activator.CreateInstance(type)
                                                                  ?? throw new InvalidOperationException(
                                                                      "Data Source Creation Failed")));
                        break;
                    case ProcessorAttribute:
                        processors.Add(type);
                        break;
                }

            if (factorys.Count != 0)
            {
                factorys.Add((AdvancedDataSourceFactory) factory);
                factory = MergeFactory.Merge(factorys.ToArray());
            }

            foreach (var (type, key) in states)
            {
                if (type == null || type.BaseType?.IsGenericType != true ||
                    type.BaseType?.GetGenericTypeDefinition() != typeof(StateBase<>))
                    continue;

                var dataType = type.BaseType.GetGenericArguments()[0];
                var actualMethod = ConfigurateStateMethod.MakeGenericMethod(dataType);
                actualMethod.Invoke(null, new object?[] {type, builder, factory, reducers, key, metadata});
            }

            foreach (var processor in processors)
                builder.Superviser.CreateAnonym(processor, $"Processor--{processor.Name}");
        }

        private static void ConfigurateState<TData>(Type target, ManagerBuilder builder, IDataSourceFactory factory,
            GroupDictionary<Type, Type> reducerMap, string? key, CreationMetadata? metadata)
            where TData : class, IStateEntity
        {
            var config = builder.WithDataSource(factory.Create<TData>(metadata));

            if (!string.IsNullOrWhiteSpace(key))
                config.WithKey(key);

            config.WithStateType(target);

            if (!reducerMap.TryGetValue(target, out var reducers)) return;

            var methods = new Dictionary<Type, MethodInfo>();
            var validators = new Dictionary<Type, object>();

            foreach (var reducer in reducers)
            {
                foreach (var method in reducer.GetMethods(BindingFlags.Public | BindingFlags.NonPublic |
                                                          BindingFlags.Static))
                {
                    if (!method.HasAttribute<ReducerAttribute>())
                        continue;

                    var parms = method.GetParameters();
                    if (parms.Length == 0)
                        continue;

                    methods[parms[1].ParameterType] = method;
                }

                foreach (var property in reducer.GetProperties(BindingFlags.Public | BindingFlags.NonPublic |
                                                               BindingFlags.Static))
                {
                    if (!property.HasAttribute<ValidatorAttribute>())
                        continue;

                    var potenialValidator = property.PropertyType;
                    if (!potenialValidator.IsAssignableTo<IValidator>())
                        continue;

                    var validatorType = potenialValidator.GetInterface(typeof(IValidator<>).Name);
                    if (validatorType == null && potenialValidator.IsGenericType &&
                        potenialValidator.GetGenericTypeDefinition() == typeof(IValidator<>))
                        validatorType = potenialValidator;
                    if (validatorType == null)
                        continue;
                    var validator = property.GetValue(null);
                    if (validator == null)
                        continue;

                    validators[validatorType.GenericTypeArguments[0]] = validator;
                }
            }

            foreach (var (actionType, reducer) in methods)
            {
                var reducerBuilder = ReducerBuilder.Create<TData>(reducer, actionType);
                if (reducerBuilder == null) continue;

                object? validator = null;
                if (validators.ContainsKey(actionType))
                    validator = validators[actionType];

                var constructedReducer = typeof(DelegateReducer<,>).MakeGenericType(actionType, typeof(TData));
                var reducerInstance = Activator.CreateInstance(constructedReducer, reducerBuilder, validator) ??
                                      throw new InvalidOperationException("Reducer Creation Failed");

                config.WithReducer(() => (IReducer<TData>) reducerInstance);
            }
        }

        private static class ReducerBuilder
        {
            private static readonly MethodInfo GenericBuilder =
                Reflex.MethodInfo(() => Create<string, string>(null!)).GetGenericMethodDefinition();

            public static ReducerBuilderBase? Create<TData>(MethodInfo info, Type actionType)
            {
                return GenericBuilder.MakeGenericMethod(typeof(TData), actionType).Invoke(null, new object[] {info}) as
                    ReducerBuilderBase;
            }

            [UsedImplicitly]
            private static ReducerBuilderBase? Create<TData, TAction>(MethodInfo info)
            {
                var returnType = info.ReturnType;
                var parms = info.GetParameterTypes().ToArray();
                if (parms.Length != 2 && parms[1] != typeof(TAction))
                    return null;

                var parm = parms[0];

                //Observable Variants
                if (parm == typeof(IObservable<MutatingContext<TData>>) &&
                    returnType == typeof(IObservable<ReducerResult<TData>>))
                    return new ReducerBuilder<TData, TAction>.ContextToResultMap(info);
                if (parm == typeof(IObservable<MutatingContext<TData>>) &&
                    returnType == typeof(IObservable<MutatingContext<TData>>))
                    return new ReducerBuilder<TData, TAction>.ContextToContextMap(info);
                if (parm == typeof(IObservable<MutatingContext<TData>>) && returnType == typeof(IObservable<TData>))
                    return new ReducerBuilder<TData, TAction>.ContextToDataMap(info);

                if (parm == typeof(IObservable<TData>) && returnType == typeof(IObservable<ReducerResult<TData>>))
                    return new ReducerBuilder<TData, TAction>.DataToResultMap(info);
                if (parm == typeof(IObservable<TData>) && returnType == typeof(IObservable<MutatingContext<TData>>))
                    return new ReducerBuilder<TData, TAction>.DataToContextMap(info);
                if (parm == typeof(IObservable<TData>) && returnType == typeof(IObservable<TData>))
                    return new ReducerBuilder<TData, TAction>.DataToDataMap(info);

                //Direct Variants
                if (parm == typeof(TData) && returnType == typeof(ReducerResult<TData>))
                    return new ReducerBuilder<TData, TAction>.DirectDataToResultMap(info);
                if (parm == typeof(TData) && returnType == typeof(MutatingContext<TData>))
                    return new ReducerBuilder<TData, TAction>.DirectDataToContextMap(info);
                if (parm == typeof(TData) && returnType == typeof(TData))
                    return new ReducerBuilder<TData, TAction>.DirectDataToDataMap(info);


                if (parm == typeof(MutatingContext<TData>) && returnType == typeof(ReducerResult<TData>))
                    return new ReducerBuilder<TData, TAction>.DirectContextToResultMap(info);
                if (parm == typeof(MutatingContext<TData>) && returnType == typeof(MutatingContext<TData>))
                    return new ReducerBuilder<TData, TAction>.DirectContextToContextMap(info);
                if (parm == typeof(MutatingContext<TData>) && returnType == typeof(TData))
                    return new ReducerBuilder<TData, TAction>.DirectContextToDataMap(info);

                //Async Variants
                if (parm == typeof(MutatingContext<TData>) && returnType == typeof(Task<ReducerResult<TData>>))
                    return new ReducerBuilder<TData, TAction>.AsyncContextToResultMap(info);
                if (parm == typeof(MutatingContext<TData>) && returnType == typeof(Task<MutatingContext<TData>>))
                    return new ReducerBuilder<TData, TAction>.AsyncContextToContextMap(info);
                if (parm == typeof(MutatingContext<TData>) && returnType == typeof(Task<TData>))
                    return new ReducerBuilder<TData, TAction>.AsyncContextToDataMap(info);

                if (parm == typeof(TData) && returnType == typeof(Task<ReducerResult<TData>>))
                    return new ReducerBuilder<TData, TAction>.AsyncDataToResultMap(info);
                if (parm == typeof(TData) && returnType == typeof(Task<MutatingContext<TData>>))
                    return new ReducerBuilder<TData, TAction>.AsyncDataToContextMap(info);
                if (parm == typeof(TData) && returnType == typeof(Task<TData>))
                    return new ReducerBuilder<TData, TAction>.AsyncDataToDataMap(info);

                return null;
            }
        }

        private abstract class ReducerBuilderBase
        {
            protected static TDelegate CreateDelegate<TDelegate>(MethodInfo method)
                where TDelegate : Delegate
                => (TDelegate) Delegate.CreateDelegate(typeof(TDelegate), method);

            protected delegate IObservable<ReducerResult<TData>> ReducerDel<TData, in TAction>(
                IObservable<MutatingContext<TData>> state, TAction action);
        }

        private abstract class ReducerBuilder<TData, TAction> : ReducerBuilderBase
        {
            private readonly MethodInfo _info;
            private ReducerDel<TData, TAction>? _reducer;

            private ReducerBuilder(MethodInfo info) => _info = info;

            protected abstract ReducerDel<TData, TAction> Build(MethodInfo info);

            public IObservable<ReducerResult<TData>> Reduce(IObservable<MutatingContext<TData>> state, TAction action)
            {
                _reducer ??= Build(_info);
                return _reducer(state, action);
            }

            public sealed class ContextToResultMap : ReducerBuilder<TData, TAction>
            {
                public ContextToResultMap(MethodInfo info)
                    : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                    => CreateDelegate<ReducerDel<TData, TAction>>(info);
            }

            public sealed class ContextToContextMap : ReducerBuilder<TData, TAction>
            {
                public ContextToContextMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del =
                        CreateDelegate<Func<IObservable<MutatingContext<TData>>, TAction,
                            IObservable<MutatingContext<TData>>>>(info);

                    return (state, action) => del(state, action).Select(ReducerResult.Sucess);
                }
            }

            public sealed class ContextToDataMap : ReducerBuilder<TData, TAction>
            {
                public ContextToDataMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del =
                        CreateDelegate<Func<IObservable<MutatingContext<TData>>, TAction, IObservable<TData>>>(info);

                    return (state, action)
                        => del(state, action).Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
                }
            }

            public sealed class DataToContextMap : ReducerBuilder<TData, TAction>
            {
                public DataToContextMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del =
                        CreateDelegate<Func<IObservable<TData>, TAction, IObservable<MutatingContext<TData>>>>(info);

                    return (state, action) => del(state.Select(c => c.Data), action).Select(ReducerResult.Sucess);
                }
            }

            public sealed class DataToResultMap : ReducerBuilder<TData, TAction>
            {
                public DataToResultMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del =
                        CreateDelegate<Func<IObservable<TData>, TAction, IObservable<ReducerResult<TData>>>>(info);

                    return (state, action) => del(state.Select(c => c.Data), action);
                }
            }

            public sealed class DataToDataMap : ReducerBuilder<TData, TAction>
            {
                public DataToDataMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<IObservable<TData>, TAction, IObservable<TData>>>(info);

                    return (state, action) => del(state.Select(c => c.Data), action)
                        .Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
                }
            }

            public sealed class DirectDataToContextMap : ReducerBuilder<TData, TAction>
            {
                public DirectDataToContextMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<TData, TAction, MutatingContext<TData>>>(info);

                    return (state, action) => state.Select(d => del(d.Data, action)).Select(ReducerResult.Sucess);
                }
            }

            public sealed class DirectDataToResultMap : ReducerBuilder<TData, TAction>
            {
                public DirectDataToResultMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<TData, TAction, ReducerResult<TData>>>(info);

                    return (state, action) => state.Select(d => del(d.Data, action));
                }
            }

            public sealed class DirectDataToDataMap : ReducerBuilder<TData, TAction>
            {
                public DirectDataToDataMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<TData, TAction, TData>>(info);

                    return (state, action) => state.Select(d => del(d.Data, action))
                        .Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
                }
            }

            public sealed class DirectContextToResultMap : ReducerBuilder<TData, TAction>
            {
                public DirectContextToResultMap(MethodInfo info)
                    : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<MutatingContext<TData>, TAction, ReducerResult<TData>>>(info);

                    return (state, action) => state.Select(d => del(d, action));
                }
            }

            public sealed class DirectContextToContextMap : ReducerBuilder<TData, TAction>
            {
                public DirectContextToContextMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<MutatingContext<TData>, TAction, MutatingContext<TData>>>(info);

                    return (state, action) => state.Select(d => del(d, action)).Select(ReducerResult.Sucess);
                }
            }

            public sealed class DirectContextToDataMap : ReducerBuilder<TData, TAction>
            {
                public DirectContextToDataMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<MutatingContext<TData>, TAction, TData>>(info);

                    return (state, action) => state.Select(d => del(d, action))
                        .Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
                }
            }

            public sealed class AsyncDataToContextMap : ReducerBuilder<TData, TAction>
            {
                public AsyncDataToContextMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<TData, TAction, Task<MutatingContext<TData>>>>(info);

                    return (state, action) => state.SelectMany(d => del(d.Data, action)).Select(ReducerResult.Sucess);
                }
            }

            public sealed class AsyncDataToResultMap : ReducerBuilder<TData, TAction>
            {
                public AsyncDataToResultMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<TData, TAction, Task<ReducerResult<TData>>>>(info);

                    return (state, action) => state.SelectMany(d => del(d.Data, action));
                }
            }

            public sealed class AsyncDataToDataMap : ReducerBuilder<TData, TAction>
            {
                public AsyncDataToDataMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<TData, TAction, Task<TData>>>(info);

                    return (state, action) => state.SelectMany(d => del(d.Data, action))
                        .Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
                }
            }

            public sealed class AsyncContextToResultMap : ReducerBuilder<TData, TAction>
            {
                public AsyncContextToResultMap(MethodInfo info)
                    : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<MutatingContext<TData>, TAction, Task<ReducerResult<TData>>>>(info);

                    return (state, action) => state.SelectMany(d => del(d, action));
                }
            }

            public sealed class AsyncContextToContextMap : ReducerBuilder<TData, TAction>
            {
                public AsyncContextToContextMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<MutatingContext<TData>, TAction, Task<MutatingContext<TData>>>>(info);

                    return (state, action) => state.SelectMany(d => del(d, action)).Select(ReducerResult.Sucess);
                }
            }

            public sealed class AsyncContextToDataMap : ReducerBuilder<TData, TAction>
            {
                public AsyncContextToDataMap([NotNull] MethodInfo info) : base(info)
                {
                }

                protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                {
                    var del = CreateDelegate<Func<MutatingContext<TData>, TAction, Task<TData>>>(info);

                    return (state, action) => state.SelectMany(d => del(d, action))
                        .Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
                }
            }
        }

        [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
        private sealed class DelegateReducer<TAction, TData> : Reducer<TAction, TData>
            where TData : IStateEntity
            where TAction : IStateAction
        {
            private readonly ReducerBuilder<TData, TAction> _builder;

            public DelegateReducer(ReducerBuilder<TData, TAction> builder, IValidator<TAction>? validation)
            {
                _builder = builder;
                Validator = validation;
            }


            public override IValidator<TAction>? Validator { get; }

            protected override IObservable<ReducerResult<TData>> Reduce(IObservable<MutatingContext<TData>> state,
                TAction action) => _builder.Reduce(state, action);
        }
    }
}