﻿using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl.Bus;
using Akka.MGIHelper.Core.FanControl.Components;
using Akka.MGIHelper.Core.FanControl.Events;
using Tauron.Features;

namespace Akka.MGIHelper.Core.FanControl
{
    public sealed class FanControlActor : ActorFeatureBase<FanControlOptions>
    {
        private FanControlActor() { }

        public static IPreparedFeature New(FanControlOptions options)
            => Feature.Create(() => new FanControlActor(), options);

        protected override void ConfigImpl()
        {
            var options = CurrentState;
            var parent = Context.Parent;

            var messageBus = new MessageBus();

            messageBus.Subscribe(new ClockComponent(options));

            messageBus.Subscribe(new DataFetchComponent(options));

            messageBus.Subscribe(
                new TrackingEventDeliveryComponent(
                    e =>
                    {
                        parent.Tell(e);

                        return Task.CompletedTask;
                    }));

            messageBus.Subscribe(new PowerComponent());
            messageBus.Subscribe(new CoolDownComponent());
            messageBus.Subscribe(new GoStandByComponent(options));
            messageBus.Subscribe(new StartUpCoolingComponent(options));
            messageBus.Subscribe(new StandByCoolingComponent(options));

            messageBus.Subscribe(
                new FanControlComponent(
                    options,
                    e =>
                    {
                        parent.Tell(new FanStatusChange(e));

                        return Task.CompletedTask;
                    }));

            Stop.Subscribe(_ => messageBus.Dispose());

            Observ<ClockEvent>(obs => obs.SelectMany(async p => await messageBus.Publish(p).ConfigureAwait(false)));
        }
    }
}