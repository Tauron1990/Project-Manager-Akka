using Vogen;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
#pragma warning disable MA0097
public readonly partial struct TransferData { }
#pragma warning restore MA0097
