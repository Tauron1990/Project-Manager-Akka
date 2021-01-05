using System;
using Akka.Actor;
using JetBrains.Annotations;
using Serilog;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.ModelMessages;
using Tauron.Host;

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
            Logger = Log.ForContext(GetType());

            UserControl = userControl;
            UserControl.DataContext = model;
            Model = model;
            BindLogic = new ControlBindLogic(userControl, model, Logger);

            // ReSharper disable once VirtualMemberCallInConstructor
            WireUpLoaded();
            // ReSharper disable once VirtualMemberCallInConstructor
            WireUpUnloaded();
        }

        public void Register(string key, IControlBindable bindable, IUIObject affectedPart) => BindLogic.Register(key, bindable, affectedPart);

        public void CleanUp(string key) => BindLogic.CleanUp(key);
        
        public event Action? ControlUnload;

        protected virtual void WireUpLoaded() => UserControl.Loaded.Subscribe(_ => UserControlOnLoaded());

        protected virtual void WireUpUnloaded() => UserControl.Unloaded.Subscribe(_ =>  UserControlOnUnloaded());

        protected virtual void UserControlOnUnloaded()
        {
            try
            {
                Logger.Debug("Control Unloaded {Element}", UserControl.GetType());
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
            ControlUnload?.Invoke();

            if (!Model.IsInitialized)
            {
                var parent = ControlBindLogic.FindParentDatacontext(UserControl);
                if (parent != null)
                    parent.Actor.Tell(new InitParentViewModel(Model));
                else
                {
                    ViewModelSuperviser.Get(ActorApplication.Application.ActorSystem)
                       .Create(Model);
                }
            }

            Model.AwaitInit(() => Model.Actor.Tell(new InitEvent()));
        }
    }
}