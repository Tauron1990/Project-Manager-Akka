using System.Reactive.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl;
using Akka.MGIHelper.Core.FanControl.Events;
using Autofac;
using Tauron;
using Tauron.Akka;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;
using Tauron.Application.CommonUI.ModelMessages;
using Tauron.Features;

namespace Akka.MGIHelper.UI.FanControl
{
    public class AutoFanControlModel : UiActor
    {
        public AutoFanControlModel(ILifetimeScope scope, IUIDispatcher dispatcher, FanControlOptions options)
            : base(scope, dispatcher)
        {
            Options = RegisterProperty<FanControlOptions>(nameof(Options)).WithDefaultValue(options);
            Error = RegisterProperty<bool>(nameof(Error));
            Reason = RegisterProperty<string>(nameof(Reason));
            Power = RegisterProperty<int>(nameof(Power));
            State = RegisterProperty<State>(nameof(State));
            Pidout = RegisterProperty<double>(nameof(Pidout));
            PidSetValue = RegisterProperty<int>(nameof(PidSetValue));
            Pt1000 = RegisterProperty<int>(nameof(Pt1000));
            FanRunning = RegisterProperty<bool>(nameof(FanRunning)).WithDefaultValue(false);

            Context.ActorOf("Fan-Control", Core.FanControl.FanControl.New(options));

            Receive<TrackingEvent>(obs => obs.SubscribeWithStatus(evt =>
                                                                  {
                                                                      Error += evt.Error;
                                                                      Reason += evt.Reason;
                                                                      Power += evt.Power;
                                                                      State += evt.State;
                                                                      Pidout += evt.Pidout;
                                                                      PidSetValue += evt.PidSetValue;
                                                                      Pt1000 += evt.Pt1000;
                                                                  }));
            Receive<FanStatusChange>(obs => obs.SubscribeWithStatus(evt => FanRunning += evt.Running));
        }

        private UIProperty<bool> Error { get; set; }

        private UIProperty<string> Reason { get; set; }

        private UIProperty<int> Power { get; set; }

        private UIProperty<State> State { get; set; }

        private UIProperty<double> Pidout { get; set; }

        private UIProperty<int> PidSetValue { get; set; }

        private UIProperty<int> Pt1000 { get; set; }

        private UIProperty<bool> FanRunning { get; set; }

        private UIProperty<FanControlOptions> Options { get; }

        protected override void Initialize(InitEvent evt) 
            => Context.Child("Fan-Control").Tell(new ClockEvent(ClockState.Start));
    }
}