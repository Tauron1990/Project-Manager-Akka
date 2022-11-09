using Vogen;

namespace Tauron.Application.AkkaNode.Services.FileTransfer;

[ValueObject(typeof(string))]
[Instance("Empty", "")]
public readonly partial struct TransferData { }