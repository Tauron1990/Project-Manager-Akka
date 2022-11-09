﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using JetBrains.Annotations;
using NLog;
using Stl;

namespace Tauron.Application.CommonUI.Helper;

public abstract class DataContextPromise
{
    public abstract void OnUnload(Action unload);

    public abstract void OnContext(Action<IViewModel, IView> modelAction);

    public abstract void OnNoContext(Action action);
}

public sealed class RootedDataContextPromise : DataContextPromise
{
    private readonly IUIElement _element;

    private Action? _noContext;

    public RootedDataContextPromise(IUIElement element) => _element = element;

    public override void OnUnload(Action unload)
    {
        if(_element is IView view)
            view.ControlUnload += unload;
    }

    public override void OnContext(Action<IViewModel, IView> modelAction)
    {
        if(_element is IView view)
        {
            if(_element.DataContext is IViewModel model)
            {
                modelAction(model, view);

                return;
            }

            void OnElementOnDataContextChanged(object newValue)
            {
                if(newValue is IViewModel localModel)
                    modelAction(localModel, view);
                else
                    _noContext?.Invoke();
            }

            _element.DataContextChanged.Take(1).Subscribe(OnElementOnDataContextChanged);
        }
        else
        {
            _noContext?.Invoke();
        }
    }

    public override void OnNoContext(Action action)
    {
        _noContext = action;
    }
}

public sealed class DisconnectedDataContextRoot : DataContextPromise
{
    private Action<IViewModel, IView>? _modelAction;
    private Action? _noContext;
    private Action? _unload;

    public DisconnectedDataContextRoot(IUIElement elementBase)
    {
        void OnLoad()
        {
            (bool hasValue, IBinderControllable value) = ControlBindLogic.FindRoot(elementBase.AsOption<IUIObject>());
            if(hasValue && value is IView control and IUIElement { DataContext: IViewModel model })
            {
                _modelAction?.Invoke(model, control);

                if(_unload != null)
                    control.ControlUnload += _unload;
            }
            else
            {
                _noContext?.Invoke();
            }
        }

        elementBase.Loaded.Take(1).Subscribe(_ => OnLoad());
    }

    public override void OnUnload(Action unload) => _unload = unload;

    public override void OnContext(Action<IViewModel, IView> modelAction) => _modelAction = modelAction;

    public override void OnNoContext(Action noContext) => _noContext = noContext;
}

[PublicAPI]
public sealed class ControlBindLogic
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<string, (IDisposable Disposer, IControlBindable Binder)> _binderList = new();
    private readonly object? _dataContext;

    private readonly IUIObject _target;

    public ControlBindLogic(IUIObject target, object dataContext)
    {
        _target = target;
        _dataContext = dataContext;
    }

    //public void NewDataContext(object? dataContext)
    //{
    //    _dataContext = dataContext;

    //    foreach (var (key, (disposer, binder)) in _binderList.ToArray())
    //    {
    //        disposer.Dispose();
    //        if (dataContext != null)
    //            _binderList[key] = (binder.NewContext(dataContext), binder);
    //    }
    //}

    public void CleanUp()
    {
        Log.Debug("Clean Up Bind Control");

        foreach (var pair in _binderList)
            pair.Value.Disposer.Dispose();

        _binderList.Clear();
    }

    public void Register(string key, IControlBindable bindable, IUIObject affectedPart)
    {
        Log.Debug(
            "Register Bind Element {Name} -- {LinkElement} -- {Part}",
            key,
            bindable.GetType(),
            affectedPart.GetType());

        if(_dataContext == null)
            return;

        IDisposable disposer = bindable.Bind(_target, affectedPart, _dataContext);

        if(affectedPart is IUIElement element)
        {
            void OnElementOnUnloaded()
            {
                disposer.Dispose();
                _binderList.Remove(key);
            }

            element.Unloaded.Take(1).Subscribe(_ => OnElementOnUnloaded());
        }

        _binderList[key] = (disposer, bindable);
    }

    public void CleanUp(string key)
    {
        Log.Debug("Clean Up Element {Name}", key);

        if(_binderList.TryGetValue(key, out (IDisposable Disposer, IControlBindable Binder) pair))
            pair.Disposer.Dispose();

        _binderList.Remove(key);
    }

    public static Option<IBinderControllable> FindRoot(Option<IUIObject> affectedOption)
        => affectedOption
           .FlatSelect(
                root =>
                {
                    Log.Debug("Try Find Root for {Element}", root.GetType());

                    IUIObject? affected = root;

                    do
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        // ReSharper disable once ConstantConditionalAccessQualifier
                        if(affected?.Object is IBinderControllable binder)
                        {
                            Log.Debug("Root Found for {Element}", affected.GetType());

                            return binder.OptionNotNull();
                        }

                        affected = affected?.GetPerent();
                    } while (affected != null);

                    Log.Debug("Root not Found for {Element}", affected?.GetType());

                    return Option<IBinderControllable>.None;
                });

    public static Option<IView> FindParentView(Option<IUIObject> affectedOption)
        => affectedOption
           .FlatSelect(
                root =>
                {
                    Log.Debug("Try Find View for {Element}", root.GetType());
                    IUIObject? affected = root;

                    do
                    {
                        // ReSharper disable once ConstantConditionalAccessQualifier
                        affected = affected?.GetPerent();

                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if(affected?.Object is not IView binder) continue;

                        Log.Debug("View Found for {Element}", affected.GetType());

                        return binder.AsOption();
                    } while (affected != null);

                    Log.Debug("View Not Found for {Element}", affected?.GetType());

                    return Option<IView>.None;
                });

    public static Option<IViewModel> FindParentDatacontext(Option<IUIObject> affectedOption)
        => affectedOption
           .FlatSelect(
                root =>
                {
                    Log.Debug("Try Find DataContext for {Element}", root.GetType());
                    IUIObject? affected = root;

                    do
                    {
                        // ReSharper disable once ConstantConditionalAccessQualifier
                        affected = affected?.GetPerent();
                        switch (affected?.Object)
                        {
                            case IUIElement { DataContext: IViewModel model }:
                                Log.Debug("DataContext Found for {Element}", affected.GetType());

                                return model.AsOption();
                        }
                    } while (affected != null);

                    Log.Debug("DataContext Not Found for {Element}", affected?.GetType());

                    return Option<IViewModel>.None;
                });

    public static bool FindDataContext(Option<IUIObject> affected, [NotNullWhen(true)] out DataContextPromise? promise)
    {
        promise = null;
        var root = FindRoot(affected);
        if(root is { HasValue: true } and { Value: IUIElement element })
            promise = new RootedDataContextPromise(element);
        else if(affected.HasValue && affected.Value is IUIElement affectedElement)
            promise = new DisconnectedDataContextRoot(affectedElement);


        return promise != null;
    }

    public static void MakeLazy(IUIElement target, string? newValue, string? oldValue, Action<string?, string?, IBinderControllable, IUIObject> runner)
    {
        var temp = new LazyHelper(target, newValue, oldValue, runner);
        target.Loaded.Take(1).Subscribe(_ => temp.ElementOnLoaded());
    }

    private class LazyHelper
    {
        private readonly string? _newValue;
        private readonly string? _oldValue;
        private readonly Action<string?, string?, IBinderControllable, IUIObject> _runner;
        private readonly IUIElement _target;

        internal LazyHelper(
            IUIElement target, string? newValue, string? oldValue,
            Action<string?, string?, IBinderControllable, IUIObject> runner)
        {
            _target = target;
            _newValue = newValue;
            _oldValue = oldValue;
            _runner = runner;
        }

        internal void ElementOnLoaded()
        {
            var root = FindRoot(_target.AsOption<IUIObject>());

            if(!root.HasValue) return;

            _runner(_oldValue, _newValue, root.Value, _target);
        }
    }
}