using System.Buffers;
using System.Diagnostics;
using System.Text;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public class LogParser : IDisposable
{
    private static readonly string[] Separator =  { "<!C>" };

    private static readonly byte[] Beginmsg = "<!M>"u8.ToArray();

    private static readonly byte[] Endmsg =  "<M!>"u8.ToArray();

    private readonly MessageBuffer<string> _dataBuffer = new(
        new StringFormatter(),
        MemoryPool<byte>.Shared);

    public LogInfo? Push(IMemoryOwner<byte> msg)
    {
	    string? newMessage = _dataBuffer.AddBuffer(msg);

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
					    stateObject.Application = array2[2];

					    break;
				    case Command.Save:
					    LoggerAsync.Log(LogInfo.CreateSave());
					    LoggerAsync.Save("S");

					    break;
				    case Command.Disconnect:
					    stateObject.StopHeartBeat();
					    clientStates.Remove(stateObject);
					    connectedClients--;
					    workSocket.Close();
					    allDone.Set();
					    LoggerAsync.Log(LogInfo.CreateError("Client " + stateObject.Application + " asked to close socket"));
					    flag = true;

					    break;
				    case Command.ShowConsole:
				    {
					    bool num2 = bool.Parse(array2[2]);
					    if(num2 && !consoleShown)
					    {
						    ShowConsole();
					    }
					    if(!num2 && consoleShown)
					    {
						    HideConsole();
					    }

					    break;
				    }
				    case CommandType.SaveBdd:
					    idInfo = int.Parse(array2[2]);
					    information = array2[3];
					    Task.Factory.StartNew(
						    delegate
						    {
							    try
							    {
								    LoggerAsync.Log(new LogInfo(LogInfo.getTime(), "KernelLogger", "Bdd", toConsole: true, permanent: true, "Sent " + idInfo + ", " + information + " to DataBase+"));
							    }
							    catch (Exception ex3)
							    {
								    LoggerAsync.Log(LogInfo.CreateError(ex3.Message));
							    }
						    });

					    break;
			    }

			    break;
		    case MessageType.Log:
			    break;
		    default:
			    throw new UnreachableException("Invalid MessageType for Log");
	    }
    }

    private enum MessageType
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