using FluentValidation;

namespace SimpleProjectManager.Operation.Client.Config;

public sealed class ConfigurationValidator : AbstractValidator<OperationConfiguration>
{
    public ConfigurationValidator()
    {
        RuleFor(oc => oc.ServerIp)
           .NotEmpty().WithMessage("Es wurde Keine IP Addresse Festgelegt")
           .Must(value => Uri.TryCreate($"http://{value}", UriKind.RelativeOrAbsolute, out _)).WithMessage("Die angegeben IP ist keine valide URL");

        RuleFor(oc => oc.AkkaPort.Value).InclusiveBetween(0, 65_535).WithMessage("der Akka Port muss zwische 0 und 65 535 liegen");

        RuleFor(oc => oc.ServerPort.Value)
           .InclusiveBetween(0, 65_535).WithMessage("der Server Port muss zwische 0 und 65 535 liegen")
           .Must((config, value) => value != config.AkkaPort).WithMessage("Der Server und Akka port müssen verschieden sein");

        RuleFor(oc => oc.Name.Value).NotEmpty().WithMessage("Das Gerät benötigt einen Idenifizierbaren Namen");

        RuleFor(oc => oc.Device)
           .Must((_, value) => !value.Active || !string.IsNullOrWhiteSpace(value.MachineInterface.Value))
           .WithMessage("Es wurde Kein Maschienen Interface Angegeben");

        RuleFor(oc => oc.Editor)
           .Must((_, value) => !value.Active || !string.IsNullOrWhiteSpace(value.Path.Value))
           .WithMessage("Es wurde Kein Pfad zu einem Bild Editor Angegeben")
           .Must((_, value) => !value.Active || !Path.HasExtension(value.Path.Value)).WithMessage("Der Pfad muss eine Datei sein");

        /*RuleFor(oc => oc.ServerUrl).CustomAsync(
            async (value, context, _) =>
            {
                try
                {
                    using var pinger = new HttpClient();
                    HttpResponseMessage result = await pinger.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{value}/Api/Ping")).ConfigureAwait(false);

                    if(result.StatusCode == HttpStatusCode.OK)
                        return;

                    context.AddFailure(nameof(OperationConfiguration.ServerUrl), "Der Server ist nicht erreichbar");
                }
                catch (Exception e)
                {
                    Exception? ex = e;
                    if(ex.InnerException is not null)
                        ex = e.InnerException;

                    context.AddFailure(nameof(OperationConfiguration.ServerUrl), $"Fehler beim versuch den Server zu erreichen: {ex!.Message}");
                }
            });*/
    }
}