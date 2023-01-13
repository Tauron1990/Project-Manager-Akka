using JetBrains.Annotations;

namespace Tauron.Application.Files.Json;

[PublicAPI]
public enum JsonNodeType
{
    Array = 1,
    Object = 2,
    String = 3,
    Number = 4,
    NullValue = 5,
    Boolean = 6,
    None = 7,
    Custom = 0xFF,
}