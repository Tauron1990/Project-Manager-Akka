using System.Reflection;
using System.Text;
using Akka.Event;
using Autofac;
using DynamicData;
using Tauron;
using Tauron.Akka;
using Tauron.Application.CommonUI.AppCore;
using Tauron.Application.CommonUI.Model;

namespace Akka.MGIHelper.UI
{
    public sealed class LogWindowViewModel : UiActor
    {
        public LogWindowViewModel(ILifetimeScope lifetimeScope, IUIDispatcher dispatcher)
            : base(lifetimeScope, dispatcher)
        {
            UnhandledMessages = this.RegisterUiCollection<string>(nameof(UnhandledMessages)).BindToList(out var list);
            list.Add("Start");

            this.SubscribeToEvent<UnhandledMessage>(obs =>
                                                        obs
                                                           .SubscribeWithStatus(obj =>
                                                                                {
                                                                                    var builder = new StringBuilder($"Name: {obj.GetType().Name}");

                                                                                    foreach (var propertyInfo in obj.Message.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                                                                                        builder.Append($" - {propertyInfo.GetValue(obj.Message)}");

                                                                                    list.Add(builder.ToString());
                                                                                }));
        }

        private UICollectionProperty<string> UnhandledMessages { get; }
    }
}