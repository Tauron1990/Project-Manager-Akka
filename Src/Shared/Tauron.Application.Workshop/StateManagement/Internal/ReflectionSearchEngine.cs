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
using Tauron.Application.Workshop.StateManagement.Builder;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.Workshop.StateManagement.Internal;

public class ReflectionSearchEngine
{
    private static readonly MethodInfo ConfigurateStateMethod =
        typeof(ReflectionSearchEngine).GetMethod(
            nameof(ConfigurateState),
            BindingFlags.Static | BindingFlags.NonPublic)
     ?? throw new InvalidOperationException("Method not Found");

    private readonly IEnumerable<Type> _types;

    public ReflectionSearchEngine(IEnumerable<Assembly> types) => _types = types.SelectMany(asm => asm.GetTypes());

    public ReflectionSearchEngine(IEnumerable<Type> types) => _types = types;

    public void Add(ManagerBuilder builder, IDataSourceFactory factory, CreationMetadata? metadata)
    {
        var states = new List<(Type TargetState, string? Key, Type[] DataTypes)>();
        var reducers = new GroupDictionary<Type, Type>();
        var factorys = new List<AdvancedDataSourceFactory>();
        var processors = new List<Type>();

        foreach (var type in _types)
            foreach (var customAttribute in type.GetCustomAttributes(inherit: false))
                ProcessAttribute(builder, customAttribute, states, type, reducers, factorys, processors);

        if (factorys.Count != 0)
        {
            factorys.Add((AdvancedDataSourceFactory)factory);
            factory = MergeFactory.Merge(factorys.ToArray());
        }

        foreach (var (type, key, dataType) in states.SelectMany(i => i.DataTypes.Select(e => (i.TargetState, i.Key, e))))
        {
            var actualMethod = ConfigurateStateMethod.MakeGenericMethod(dataType);
            actualMethod.Invoke(null, new object?[] { type, builder, factory, reducers, key, metadata });
        }

        foreach (var processor in processors)
            builder.Superviser.CreateAnonym(processor, $"Processor--{processor.Name}");
    }

    private void ProcessAttribute(ManagerBuilder builder, object customAttribute, ICollection<(Type TargetState, string? Key, Type[] DataTypes)> states, Type type, GroupDictionary<Type, Type> reducers, ICollection<AdvancedDataSourceFactory> factorys, ICollection<Type> processors)
    {
        switch (customAttribute)
        {
            case StateAttribute state:
                states.Add((type, state.Key, state.Types));

                break;
            case EffectAttribute:
                builder.WithEffect(CreateFactory<IEffect>(type, builder));

                break;
            case MiddlewareAttribute:
                builder.WithMiddleware(CreateFactory<IMiddleware>(type, builder));

                break;
            case BelogsToStateAttribute belogsTo:
                reducers.Add(belogsTo.StateType, type);

                break;
            case DataSourceAttribute:
                factorys.Add(
                    (AdvancedDataSourceFactory)(builder.ComponentContext?.ResolveOptional(type)
                                             ?? Activator.CreateInstance(type)
                                             ?? throw new InvalidOperationException("Data Source Creation Failed")));

                break;
            case ProcessorAttribute:
                processors.Add(type);

                break;
        }
    }

    private Func<TType> CreateFactory<TType>(Type target, ManagerBuilder builder)
    {
        if (builder.ComponentContext != null)
            return () => (TType)(builder.ComponentContext.ResolveOptional(target) ?? Activator.CreateInstance(target))!;

        return () => (TType)Activator.CreateInstance(target)!;
    }

    private static void ConfigurateState<TData>(Type target, ManagerBuilder builder, IDataSourceFactory factory, GroupDictionary<Type, Type> reducerMap, string? key, CreationMetadata? metadata)
        where TData : class, IStateEntity
    {
        if (builder.StateRegistrated<TData>(target))
            return;

        var config = builder.WithDataSource(factory.Create<TData>(metadata));

        if (!string.IsNullOrWhiteSpace(key))
            config.WithKey(key);

        config.WithStateType(target);

        var dispatcherAttrOption = target.GetCustomAttribute<DispatcherAttribute>();

        if (dispatcherAttrOption.HasValue)
        {
            var dispatcherAttr = dispatcherAttrOption.Value;
            if (string.IsNullOrWhiteSpace(dispatcherAttr.Name))
                config.WithDispatcher(dispatcherAttr.CreateConfig());
            else
                config.WithDispatcher(dispatcherAttr.Name, dispatcherAttr.CreateConfig());
        }

        if (!reducerMap.TryGetValue(target, out var reducers)) return;

        var methods = new Dictionary<Type, MethodInfo>();
        var validators = new Dictionary<Type, object>();

        foreach (var reducer in reducers)
        {
            ProcessReducerMethods(reducer, methods);
            ProcessReducerPropertys(reducer, validators);
        }

        foreach (var (actionType, reducer) in methods) 
            ConfigurateReducer(reducer, actionType, validators, config);
    }

    private static void ConfigurateReducer<TData>(MethodInfo reducer, Type actionType, IReadOnlyDictionary<Type, object> validators, IStateBuilder<TData> config) where TData : class, IStateEntity
    {
        var reducerBuilder = ReducerBuilder.Create<TData>(reducer, actionType);

        if (reducerBuilder is null) return;

        object? validator = null;
        if (validators.ContainsKey(actionType))
            validator = validators[actionType];

        var constructedReducer = typeof(DelegateReducer<,>).MakeGenericType(actionType, typeof(TData));
        var reducerInstance = Activator.CreateInstance(constructedReducer, reducerBuilder, validator) ??
                              throw new InvalidOperationException("Reducer Creation Failed");

        config.WithReducer(() => (IReducer<TData>)reducerInstance);
    }

    private static void ProcessReducerPropertys(IReflect reducer, IDictionary<Type, object> validators)
    {
        foreach (var (property, potenialValidator) in from prop in reducer.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                                                      where prop.HasAttribute<ValidatorAttribute>()
                                                      let potentialValidator = prop.PropertyType
                                                      where potentialValidator.IsAssignableTo<IValidator>()
                                                      select (prop, potentialValidator))
        {
            var validatorType = potenialValidator.GetInterface(typeof(IValidator<>).Name);
            if (validatorType is null && potenialValidator.IsGenericType &&
                potenialValidator.GetGenericTypeDefinition() == typeof(IValidator<>))
                validatorType = potenialValidator;

            if (validatorType is null)
                continue;

            var validator = property.GetValue(null);

            if (validator is null)
                continue;

            validators[validatorType.GenericTypeArguments[0]] = validator;
        }
    }

    private static void ProcessReducerMethods(Type reducer, Dictionary<Type, MethodInfo> methods)
    {
        foreach (var method in reducer.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (!method.HasAttribute<ReducerAttribute>())
                continue;

            var parms = method.GetParameters();

            if (parms.Length != 2)
                continue;

            methods[parms[1].ParameterType] = method;
        }
    }

    private static class ReducerBuilder
    {
        private static readonly MethodInfo GenericBuilder = Reflex.MethodInfo(() => Create<string, string>(null!)).GetGenericMethodDefinition();

        internal static ReducerBuilderBase? Create<TData>(MethodInfo info, Type actionType)
            => GenericBuilder.MakeGenericMethod(typeof(TData), actionType).Invoke(null, new object[] { info }) as ReducerBuilderBase;

        [UsedImplicitly]
        // ReSharper disable once CognitiveComplexity
        // No Better Way?
        private static ReducerBuilderBase? Create<TData, TAction>(MethodInfo info)
        {
            var returnType = info.ReturnType;
            var parms = info.GetParameterTypes().ToArray();

            if (parms.Length != 2 && parms[1] != typeof(TAction))
                return null;

            var parm = parms[0];

            //Observable Variants
            if (parm == typeof(IObservable<MutatingContext<TData>>) && returnType == typeof(IObservable<ReducerResult<TData>>))
                return new ReducerBuilder<TData, TAction>.ContextToResultMap(info);
            if (parm == typeof(IObservable<MutatingContext<TData>>) && returnType == typeof(IObservable<MutatingContext<TData>>))
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
            => (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), method);

        protected delegate IObservable<ReducerResult<TData>> ReducerDel<TData, in TAction>(
            IObservable<MutatingContext<TData>> state, TAction action);
    }

    private abstract class ReducerBuilder<TData, TAction> : ReducerBuilderBase
    {
        private readonly MethodInfo _info;
        private ReducerDel<TData, TAction>? _reducer;

        private ReducerBuilder(MethodInfo info) => _info = info;

        protected abstract ReducerDel<TData, TAction> Build(MethodInfo info);

        internal IObservable<ReducerResult<TData>> Reduce(IObservable<MutatingContext<TData>> state, TAction action)
        {
            _reducer ??= Build(_info);

            return _reducer(state, action);
        }

        internal sealed class ContextToResultMap : ReducerBuilder<TData, TAction>
        {
            internal ContextToResultMap(MethodInfo info)
                : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
                => CreateDelegate<ReducerDel<TData, TAction>>(info);
        }

        internal sealed class ContextToContextMap : ReducerBuilder<TData, TAction>
        {
            internal ContextToContextMap(MethodInfo info)
                : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del =
                    CreateDelegate<Func<IObservable<MutatingContext<TData>>, TAction,
                        IObservable<MutatingContext<TData>>>>(info);

                return (state, action) => del(state, action).Select(ReducerResult.Sucess);
            }
        }

        internal sealed class ContextToDataMap : ReducerBuilder<TData, TAction>
        {
            internal ContextToDataMap(MethodInfo info)
                : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del =
                    CreateDelegate<Func<IObservable<MutatingContext<TData>>, TAction, IObservable<TData>>>(info);

                return (state, action)
                           => del(state, action).Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
            }
        }

        internal sealed class DataToContextMap : ReducerBuilder<TData, TAction>
        {
            internal DataToContextMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del =
                    CreateDelegate<Func<IObservable<TData>, TAction, IObservable<MutatingContext<TData>>>>(info);

                return (state, action) => del(state.Select(c => c.Data), action).Select(ReducerResult.Sucess);
            }
        }

        internal sealed class DataToResultMap : ReducerBuilder<TData, TAction>
        {
            internal DataToResultMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del =
                    CreateDelegate<Func<IObservable<TData>, TAction, IObservable<ReducerResult<TData>>>>(info);

                return (state, action) => del(state.Select(c => c.Data), action);
            }
        }

        internal sealed class DataToDataMap : ReducerBuilder<TData, TAction>
        {
            internal DataToDataMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<IObservable<TData>, TAction, IObservable<TData>>>(info);

                return (state, action) => del(state.Select(c => c.Data), action)
                          .Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
            }
        }

        internal sealed class DirectDataToContextMap : ReducerBuilder<TData, TAction>
        {
            internal DirectDataToContextMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<TData, TAction, MutatingContext<TData>>>(info);

                return (state, action) => state.Select(d => del(d.Data, action)).Select(ReducerResult.Sucess);
            }
        }

        internal sealed class DirectDataToResultMap : ReducerBuilder<TData, TAction>
        {
            internal DirectDataToResultMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<TData, TAction, ReducerResult<TData>>>(info);

                return (state, action) => state.Select(d => del(d.Data, action));
            }
        }

        internal sealed class DirectDataToDataMap : ReducerBuilder<TData, TAction>
        {
            internal DirectDataToDataMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<TData, TAction, TData>>(info);

                return (state, action) => state.Select(d => del(d.Data, action))
                          .Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
            }
        }

        internal sealed class DirectContextToResultMap : ReducerBuilder<TData, TAction>
        {
            internal DirectContextToResultMap(MethodInfo info)
                : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<MutatingContext<TData>, TAction, ReducerResult<TData>>>(info);

                return (state, action) => state.Select(d => del(d, action));
            }
        }

        internal sealed class DirectContextToContextMap : ReducerBuilder<TData, TAction>
        {
            internal DirectContextToContextMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<MutatingContext<TData>, TAction, MutatingContext<TData>>>(info);

                return (state, action) => state.Select(d => del(d, action)).Select(ReducerResult.Sucess);
            }
        }

        internal sealed class DirectContextToDataMap : ReducerBuilder<TData, TAction>
        {
            internal DirectContextToDataMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<MutatingContext<TData>, TAction, TData>>(info);

                return (state, action) => state.Select(d => del(d, action))
                          .Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
            }
        }

        internal sealed class AsyncDataToContextMap : ReducerBuilder<TData, TAction>
        {
            internal AsyncDataToContextMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<TData, TAction, Task<MutatingContext<TData>>>>(info);

                return (state, action) => state.SelectMany(d => del(d.Data, action)).Select(ReducerResult.Sucess);
            }
        }

        internal sealed class AsyncDataToResultMap : ReducerBuilder<TData, TAction>
        {
            internal AsyncDataToResultMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<TData, TAction, Task<ReducerResult<TData>>>>(info);

                return (state, action) => state.SelectMany(d => del(d.Data, action));
            }
        }

        internal sealed class AsyncDataToDataMap : ReducerBuilder<TData, TAction>
        {
            internal AsyncDataToDataMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<TData, TAction, Task<TData>>>(info);

                return (state, action) => state.SelectMany(d => del(d.Data, action))
                          .Select(d => ReducerResult.Sucess(MutatingContext<TData>.New(d)));
            }
        }

        internal sealed class AsyncContextToResultMap : ReducerBuilder<TData, TAction>
        {
            internal AsyncContextToResultMap(MethodInfo info)
                : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<MutatingContext<TData>, TAction, Task<ReducerResult<TData>>>>(info);

                return (state, action) => state.SelectMany(d => del(d, action));
            }
        }

        internal sealed class AsyncContextToContextMap : ReducerBuilder<TData, TAction>
        {
            internal AsyncContextToContextMap(MethodInfo info) : base(info) { }

            protected override ReducerDel<TData, TAction> Build(MethodInfo info)
            {
                var del = CreateDelegate<Func<MutatingContext<TData>, TAction, Task<MutatingContext<TData>>>>(info);

                return (state, action) => state.SelectMany(d => del(d, action)).Select(ReducerResult.Sucess);
            }
        }

        internal sealed class AsyncContextToDataMap : ReducerBuilder<TData, TAction>
        {
            internal AsyncContextToDataMap(MethodInfo info) : base(info) { }

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

        internal DelegateReducer(ReducerBuilder<TData, TAction> builder, IValidator<TAction>? validation)
        {
            _builder = builder;
            Validator = validation;
        }


        public override IValidator<TAction>? Validator { get; }

        protected override IObservable<ReducerResult<TData>> Reduce(
            IObservable<MutatingContext<TData>> state,
            TAction action) => _builder.Reduce(state, action);
    }
}