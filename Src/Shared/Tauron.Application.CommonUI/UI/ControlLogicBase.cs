using System;
using Akka.Actor;
using JetBrains.Annotations;
using NLog;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;
using Tauron.AkkaHost;
using Tauron.ObservableExt;

namespace Tauron.Application.CommonUI.UI
{
    [PublicAPI]
    public abstract class ControlLogicBase<TControl> : IView
        where TControl : IUIElement, IView
    {
        protected readonly ControlBindLogic BindLogic;

        protected readonly ILogger Logger;
        protected readonly IViewModel Model;
        protected readonly TControl UserControl;

        protected ControlLogicBase(TControl userControl, IViewModel model)
        {
            Logger = LogManager.GetCurrentClassLogger();

            UserControl = userControl;
            UserControl.DataContext = model;
            Model = model;
            BindLogic = new ControlBindLogic(userControl, model);

            // ReSharper disable once VirtualMemberCallInConstructor
            WireUpLoaded();
            // ReSharper disable once VirtualMemberCallInConstructor
            WireUpUnloaded();
        }

        public void Register(string key, IControlBindable bindable, IUIObject affectedPart) 
            => BindLogic.Register(key, bindable, affectedPart);

        public void CleanUp(string key) 
            => BindLogic.CleanUp(key);

        public event Action? ControlUnload;

        protected virtual void WireUpLoaded() 
            => UserControl.Loaded.Subscribe(_ => UserControlOnLoaded());

        protected virtual void WireUpUnloaded() 
            => UserControl.Unloaded.Subscribe(_ => UserControlOnUnloaded());

        protected virtual void UserControlOnUnloaded()
        {
            try
            {
                Logger.Debug("Control Unloaded {Element}", UserControl.GetType());
				ControlUnload?.Invoke();
                BindLogic.CleanUp();
                Model.Actor.Tell(new UnloadEvent());
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error On Unload User Control");
            }
        }

        protected virtual void UserControlOnLoaded()
        {
            Logger.Debug("Control Loaded {Element}", UserControl.GetType());

            if (!Model.IsInitialized)
            {
                var parentOption = ControlBindLogic.FindParentDatacontext(UserControl);
                parentOption.Run(
                    parent => parent.Actor.Tell(new InitParentViewModel(Model)),
                    () => ViewModelSuperviser.Get(ActorApplication.ActorSystem)
                                             .Create(Model));
            }

            Model.AwaitInit(() => Model.Actor.Tell(new InitEvent()));
        }
    }
}