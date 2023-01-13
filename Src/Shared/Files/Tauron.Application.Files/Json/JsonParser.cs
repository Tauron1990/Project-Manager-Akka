using JetBrains.Annotations;

namespace Tauron.Application.Files.Json;

[PublicAPI]
public static class JsonParser
{
    public static JsonNode Parse(string aJson) => JsonNode.Parse(aJson);
}