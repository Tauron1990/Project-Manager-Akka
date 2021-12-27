using System.Globalization;
using Akka.Actor;
using Akka.Util;
using JetBrains.Annotations;
using Tauron.Localization.Actor;
using Tauron.TAkka;

namespace Tauron.Localization.Extension;

[PublicAPI]
public sealed class LocExtensionAdaptor
{
    private readonly LocExtension _extension;
    private readonly ActorSystem _system;

    public LocExtensionAdaptor(LocExtension extension, ActorSystem system)
    {
        _system = system;
        _extension = extension;
    }

    public void Request(string name, Action<Option<object>> valueResponse, CultureInfo? info = null)
    {
        var hook = EventActor.CreateSelfKilling(_system, null);
        hook.Register(HookEvent.Create<LocCoordinator.ResponseLocValue>(res => valueResponse(res.Result))).Ignore();
        hook.Send(
            _extension.LocCoordinator,
            new LocCoordinator.RequestLocValue(name, info ?? CultureInfo.CurrentUICulture));
    }

    #pragma warning disable AV1551
    public Option<object> Request(string name, CultureInfo? info = null)
        #pragma warning restore AV1551
        => _extension.LocCoordinator
           .Ask<LocCoordinator.ResponseLocValue>(new LocCoordinator.RequestLocValue(name, info ?? CultureInfo.CurrentUICulture))
           .Result.Result;

    #pragma warning disable AV1755
    public async Task<Option<object>> RequestTask(string name, CultureInfo? info = null)
        #pragma warning restore AV1755
        => (await _extension.LocCoordinator.Ask<LocCoordinator.ResponseLocValue>(new LocCoordinator.RequestLocValue(name, info ?? CultureInfo.CurrentUICulture))).Result;

    #pragma warning disable AV1551
    public Option<string> RequestString(string name, CultureInfo? info = null)
        #pragma warning restore AV1551
        => Request(name, info).Select(value => value.ToString() ?? string.Empty);

    public void RequestString(string name, Action<Option<string>> valueResponse, CultureInfo? info = null)
        => Request(name, option => valueResponse(option.Select(oo => oo.ToString() ?? string.Empty)), info);
}