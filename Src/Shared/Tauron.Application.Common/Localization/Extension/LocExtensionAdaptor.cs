using System;
using System.Globalization;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Localization.Actor;

namespace Tauron.Localization.Extension
{
    [PublicAPI]
    public sealed class LocExtensionAdaptor
    {
        private readonly LocExtension _extension;
        private readonly ActorSystem _system;

        public LocExtensionAdaptor(LocExtension extension, ActorSystem system)
        {
            _system = system;
            _extension = Argument.NotNull(extension, nameof(extension));
        }

        public void Request(string name, Action<Option<object>> valueResponse, CultureInfo? info = null)
        {
            var hook = EventActor.Create(_system, null, true);
            hook.Register(HookEvent.Create<LocCoordinator.ResponseLocValue>(res => valueResponse(res.Result)));
            hook.Send(_extension.LocCoordinator,
                new LocCoordinator.RequestLocValue(name, info ?? CultureInfo.CurrentUICulture));
        }

        public Option<object> Request(string name, CultureInfo? info = null) 
            => _extension.LocCoordinator
                         .Ask<LocCoordinator.ResponseLocValue>(new LocCoordinator.RequestLocValue(name, info ?? CultureInfo.CurrentUICulture))
                         .Result.Result;

        public Task<Option<object>> RequestTask(string name, CultureInfo? info = null)
            => _extension.LocCoordinator.Ask<LocCoordinator.ResponseLocValue>(new LocCoordinator.RequestLocValue(name, info ?? CultureInfo.CurrentUICulture))
                         .ContinueWith(t => t.Result.Result);

        public Option<string> RequestString(string name, CultureInfo? info = null)
            => Request(name, info).Select(s => s.ToString() ?? string.Empty);

        public void RequestString(string name, Action<Option<string>> valueResponse, CultureInfo? info = null) 
            => Request(name, o => valueResponse(o.Select(oo => oo.ToString() ?? string.Empty)), info);
    }
}