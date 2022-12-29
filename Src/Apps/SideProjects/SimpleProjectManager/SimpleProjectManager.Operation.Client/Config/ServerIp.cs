using System.Net;
using Vogen;

namespace SimpleProjectManager.Operation.Client.Config;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct ServerIp
{
    private static Validation Validate(string value)
    {
        if(value.Contains("localhost", StringComparison.Ordinal)) return Validation.Ok;

        if(IPAddress.TryParse(value, out _)) return Validation.Ok;

        return Uri.TryCreate(value, UriKind.Absolute, out _) ? Validation.Ok : Validation.Invalid("Invalid Url or Ip Adress");

    }
}