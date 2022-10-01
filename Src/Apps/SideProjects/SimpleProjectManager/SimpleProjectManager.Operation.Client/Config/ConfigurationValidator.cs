using System.Net;
using System.Net.NetworkInformation;
using FluentValidation;

namespace SimpleProjectManager.Operation.Client.Config;

public sealed class ConfigurationValidator : AbstractValidator<OperationConfiguration>
{
    public ConfigurationValidator()
    {
        RuleFor(oc => oc.ServerIp)
           .NotEmpty().WithMessage("Es wurde Keine IP Addresse Festgelegt")
           .Must(value => Uri.TryCreate($"http://{value}", UriKind.RelativeOrAbsolute, out _)).WithMessage("Die angegeben IP ist keine valide URL");
        
        RuleFor(oc => oc.AkkaPort).InclusiveBetween(0, 65_535).WithMessage("der Akka Port muss zwische 0 und 65 535 liegen");
        
        RuleFor(oc => oc.ServerPort)
           .InclusiveBetween(0, 65_535).WithMessage("der Server Port muss zwische 0 und 65 535 liegen")
           .Must((config, value) => value != config.AkkaPort).WithMessage("Der Server und Akka port müssen verschieden sein");

        RuleFor(oc => oc.Name).NotEmpty().WithMessage("Das Gerät benötigt einen Idenifizierbaren Namen");

        RuleFor(oc => oc.MachineInterface)
           .Must((config, value) => !config.Device || !string.IsNullOrWhiteSpace(value))
           .WithMessage("Es wurde Kein Maschienen Interface Angegeben");
        
        RuleFor(oc => oc.Path)
           .Must((config, value) => !config.ImageEditor || !string.IsNullOrWhiteSpace(value))
           .WithMessage("Es wurde Kein Pfad zu einem Bild Editor Angegeben")
           .Must((config, value) => !config.ImageEditor || !Path.HasExtension(value)).WithMessage("Der Pfad muss eine Datei sein");

        RuleFor(oc => oc.ServerUrl).CustomAsync(
            async (value, context, _) =>
            {
                try
                {
                    using var pinger = new HttpClient();
                    var result = await pinger.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"{value}/Api/Ping"));
                    if(result.StatusCode == HttpStatusCode.OK)
                        return;
                
                    context.AddFailure(nameof(OperationConfiguration.ServerUrl), "Der Server ist nicht erreichbar");
                }
                catch (Exception e)
                {
                    var ex = e;
                    if(ex.InnerException is not null)
                        ex = e.InnerException;
                    
                    context.AddFailure(nameof(OperationConfiguration.ServerUrl), $"Fehler beim versuch den Server zu erreichen: {ex!.Message}");
                }
            });
    }
}