using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public class LogParser : IDisposable
{
    internal static readonly string[] Separator =  { "<!C>" };

    internal const string BeginmsgStr = "<!M>";

    internal const string EndmsgStr =  "<M!>";
    
    private static readonly byte[] Beginmsg = "<!M>"u8.ToArray();

    private static readonly byte[] Endmsg =  "<M!>"u8.ToArray();

    private readonly MessageBuffer<string> _dataBuffer = new(
        new StringFormatter(),
        MemoryPool<byte>.Shared);

    public LogInfo? Push(IMemoryOwner<byte> msg, int msgLenght)
    {
	    try
	    {
		    string? newMessage = _dataBuffer.AddBuffer(msg, msgLenght);

		    if(string.IsNullOrWhiteSpace(newMessage)) return null;

		    string[] messageSegments = newMessage.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

		    if(messageSegments.Length == 0) return null;

		    switch (Enum.Parse<MessageType>(messageSegments[0]))
		    {
			    case MessageType.Commnd:
				    var commandType = Enum.Parse<Command>(messageSegments[1]);
				    switch (commandType)
				    {
					    case Command.SetApp:
						    string name = messageSegments[2];

						    return LogInfo.CreateCommad(commandType, name);
					    case Command.Save:
						    return LogInfo.CreateSave();
					    case Command.Disconnect:
						    return LogInfo.CreateCommad(commandType, "Socked Close");
					    case Command.ShowConsole:
						    return null;
					    case Command.SaveBdd:
						    int idInfo = int.Parse(messageSegments[2], NumberStyles.Any, CultureInfo.InvariantCulture);
						    string information = messageSegments[3];
						    #pragma warning disable MA0076
						    var message = $"Sent {idInfo}, {information} to DataBase+";
						    #pragma warning restore MA0076

						    return new LogInfo(DateTime.Now, "KernelLogger-Proxy", "Bdd", message, commandType);
				    }

				    break;
			    case MessageType.Log:
				    return new LogInfo(
					    LogInfo.ToDateTime(messageSegments[1]), 
					    "", 
					    messageSegments[2], 
					    messageSegments[messageSegments.Length < 6 ? 4 : 5], 
					    Command.Log);
			    default:
				    #pragma warning disable EX006
				    throw new UnreachableException("Invalid MessageType for Log");
			    #pragma warning restore EX006
		    }
	    }
	    catch (Exception e)
	    {
		    return LogInfo.CreateError(e.ToStringDemystified());
	    }

	    return null;
    }

    internal enum MessageType
    {
        Commnd,
        Log,
    }
    
    private sealed class StringFormatter : INetworkMessageFormatter<string>
    {
        public bool HasHeader(Memory<byte> buffer)
        {
            var pos = 0;

            return NetworkMessageFormatter.CheckPresence(buffer.Span, Beginmsg, ref pos);
        }

        public bool HasTail(Memory<byte> buffer)
        {
            if(buffer.Length < Endmsg.Length)
                return false;

            int pos = buffer.Length - Endmsg.Length;

            return NetworkMessageFormatter.CheckPresence(buffer.Span, Endmsg, ref pos);
        }

        public string ReadMessage(Memory<byte> bufferMemory)
            => Encoding.UTF8.GetString(bufferMemory[Beginmsg.Length..^Endmsg.Length].Span);
    }

    public void Dispose()
        => _dataBuffer.Dispose();
}