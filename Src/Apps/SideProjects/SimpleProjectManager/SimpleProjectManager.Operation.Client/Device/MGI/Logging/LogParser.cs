using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading.Channels;
using Tauron.Servicemnager.Networking.Data;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Logging;

public class LogParser : IDisposable
{
    internal static readonly string[] Separator =  { "<!C>" };

    internal const string BeginmsgStr = "<!M>";

    internal const string EndmsgStr =  "<M!>";
    
    private static readonly byte[] Beginmsg = "<!M>"u8.ToArray();

    private static readonly byte[] Endmsg =  "<M!>"u8.ToArray();

    private readonly MessageReader<string> _dataReader;

    public LogParser(IMessageStream messageStream) => _dataReader = new MessageReader<string>(messageStream, new StringFormatter());

    public async Task Run(ChannelWriter<LogInfo> logs, CancellationToken cancellationToken)
    {
	    var messages = Channel.CreateBounded<string>(new BoundedChannelOptions(1000)
	                                                 {
		                                                 FullMode = BoundedChannelFullMode.DropOldest,
		                                                 SingleReader = true,
		                                                 SingleWriter = true,
	                                                 } );
	    try
	    {
		    // ReSharper disable once MethodSupportsCancellation
		    Task runner = Task.Run(() => _dataReader.ReadAsync(messages.Writer, cancellationToken));

		    await foreach (string newMessage in messages.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
		    {
			    try
			    {
				    string[] messageSegments = newMessage.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

				    if(messageSegments.Length == 0) continue;

				    LogInfo? log = CreateLogInfo(messageSegments);

				    if(log is null) continue;

				    await logs.WriteAsync(log, cancellationToken).ConfigureAwait(false);

			    }
			    catch (Exception e)
			    {
				    await logs.WriteAsync(LogInfo.CreateError(e.ToStringDemystified()), cancellationToken).ConfigureAwait(false);
			    }
		    }

		    await runner.ConfigureAwait(false);
	    }
	    catch(OperationCanceledException)
	    {}
	    finally
	    {
		    logs.TryComplete();
	    }
    }

    private static LogInfo? CreateLogInfo(IReadOnlyList<string> messageSegments)
    {
	    LogInfo? log;
	    switch (Enum.Parse<MessageType>(messageSegments[0]))
	    {
		    case MessageType.Command:
			    var commandType = Enum.Parse<Command>(messageSegments[1]);
			    log = ProcessCommand(messageSegments, commandType);

			    break;
		    case MessageType.Log:
			    log = new LogInfo(
				    LogInfo.ToDateTime(messageSegments[1]),
				    "",
				    messageSegments[2],
				    messageSegments[messageSegments.Count < 6 ? 4 : 5],
				    Command.Log);

			    break;
		    default:
			    throw new UnreachableException("Invalid MessageType for Log");
	    }
	    return log;
    }

    private static LogInfo? ProcessCommand(IReadOnlyList<string> messageSegments, Command commandType)
    {
	    LogInfo? log = null;
	    switch (commandType)
	    {
		    case Command.SetApp:
			    string name = messageSegments[2];

			    log = LogInfo.CreateCommad(commandType, name);

			    break;
		    case Command.Save:
			    log = LogInfo.CreateSave();

			    break;
		    case Command.Disconnect:
			    log = LogInfo.CreateCommad(commandType, "Socked Close");

			    break;
		    case Command.ShowConsole:
			    break;
		    case Command.SaveBdd:
			    int idInfo = int.Parse(messageSegments[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			    string information = messageSegments[3];
			    #pragma warning disable MA0076
			    var message = $"Sent {idInfo}, {information} to DataBase+";
			    #pragma warning restore MA0076

			    log = new LogInfo(DateTime.Now, "KernelLogger-Proxy", "Bdd", message, commandType);

			    break;
		    case Command.Log:
			    break;
		    default:
			    throw new UnreachableException("Invalid CommandType for Log");
	    }

	    return log;
    }

    internal enum MessageType
    {
        Command,
        Log,
    }
    
    private sealed class StringFormatter : INetworkMessageFormatter<string>
    {
	    public Memory<byte> Header => Beginmsg;
	    public Memory<byte> Tail => Endmsg;
	    
	    public string ReadMessage(in ReadOnlySequence<byte> bufferMemory)
		    => Encoding.UTF8.GetString(bufferMemory);
    }
    
    public void Dispose()
        => _dataReader.Dispose();
}