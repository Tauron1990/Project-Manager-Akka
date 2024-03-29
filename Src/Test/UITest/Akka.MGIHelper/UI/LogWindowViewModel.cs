﻿using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using Akka.Event;
using DynamicData;
using Tauron;
using Tauron.TAkka;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;

namespace Akka.MGIHelper.UI
{
    public sealed class LogWindowViewModel : UiActor
    {
        public LogWindowViewModel(IServiceProvider lifetimeScope, IUIDispatcher dispatcher)
            : base(lifetimeScope, dispatcher)
        {
            UnhandledMessages = this.RegisterUiCollection<string>(nameof(UnhandledMessages)).BindToList(out var list);
            list.Add("Start");

            this.SubscribeToEvent<UnhandledMessage>(
                obs =>
                    obs
                       .SubscribeWithStatus(
                            obj =>
                            {
                                var builder = new StringBuilder($"Name: {obj.GetType().Name}");

                                foreach (var propertyInfo in obj.Message.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                                    builder.Append(CultureInfo.InvariantCulture, $" - {propertyInfo.GetValue(obj.Message)}");

                                list.Add(builder.ToString());
                            }));
        }

        private UICollectionProperty<string> UnhandledMessages { get; }
    }
}