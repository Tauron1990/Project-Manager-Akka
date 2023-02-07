namespace SimpleProjectManager.Operation.Client.Tests;

public class LoggerTcpFormat
{
    	public enum LogType
	{
		Init,
		Com,
		ERROR,
		Crash,
		Elec,
		Pilot,
		Print,
		Clean,
		VDP,
		Close,
		iFoil
	}

	private interface ITcpable
	{
		string ToTcp();
	}

	private enum MessageType
	{
		Command,
		Log
	}

	private enum CommandType
	{
		SetApp,
		Save,
		Disconnect,
		ShowConsole,
		SaveBdd
	}

	private class Message : ITcpable
	{
		private MessageType type;

		private ITcpable content;

		public Message(MessageType type)
		{
			this.type = type;
			content = null;
		}

		public Message(MessageType type, ITcpable content)
		{
			this.type = type;
			this.content = content;
		}

		public string ToTcp()
		{
			if (content != null)
			{
				return BeginMsg + type.ToString() + Separator + content.ToTcp() + EndMsg;
			}
			return BeginMsg + type.ToString() + EndMsg;
		}

		public bool IsDisconnect()
		{
			if (type == MessageType.Command && ((Command)content).type == CommandType.Disconnect)
			{
				return true;
			}
			return false;
		}
	}

	private class Command : ITcpable
	{
		public CommandType type;

		private ITcpable content;

		public Command(CommandType type, string content)
		{
			this.type = type;
			this.content = new StringTcp(content);
		}

		public Command(int idInfo, string information)
		{
			type = CommandType.SaveBdd;
			content = new BddInfo(idInfo, information);
		}

		public Command(CommandType type)
		{
			this.type = type;
			content = null;
		}

		public string ToTcp()
		{
			if (content != null)
			{
				return type.ToString() + Separator + content.ToTcp();
			}
			return type.ToString();
		}
	}

	private class LogInfo : ITcpable
	{
		private string timeStamp;

		private LogType type;

		private bool toConsole;

		private string content;

		private bool permanent;

		public LogInfo(LogType type, bool toConsole, string content)
		{
			timeStamp = getTime();
			this.type = type;
			this.toConsole = toConsole;
			this.content = content;
		}

		public LogInfo(LogType type, bool toConsole, string content, bool permanent)
		{
			timeStamp = getTime();
			this.type = type;
			this.toConsole = toConsole;
			this.content = content;
			this.permanent = permanent;
		}

		public string ToTcp()
		{
			return timeStamp + Separator + type.ToString() + Separator + toConsole + Separator + permanent + Separator + content;
		}
	}

	private class StringTcp : ITcpable
	{
		private string content;

		public StringTcp(string content)
		{
			this.content = content;
		}

		public string ToTcp()
		{
			return content;
		}
	}

	private class BddInfo : ITcpable
	{
		private int idInfo;

		private string information;

		public BddInfo(int idInfo, string information)
		{
			this.idInfo = idInfo;
			this.information = information;
		}

		public string ToTcp()
		{
			return idInfo + Separator + information;
		}
	}

	public static readonly string Separator = "<!C>";

	public static readonly string BeginMsg = "<!M>";

	public static readonly string EndMsg = "<M!>";
	public static string getTime()
	{
		DateTime now = DateTime.Now;
		return now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + ":" + now.Millisecond.ToString("000");
	}

	public static string GetMessage(Client.Device.MGI.Logging.LogInfo data)
	{
		switch (data.Command)
		{

			case Client.Device.MGI.Logging.Command.Log:
				if(!Enum.TryParse<LogType>(data.Type, out var type))
					type = LogType.Crash;

				return Format(MessageType.Log, new LogInfo(type, toConsole: true, data.Content, permanent: true));
			case Client.Device.MGI.Logging.Command.SetApp:
				return Format(MessageType.Command, new Command(CommandType.SetApp, data.Application));
			case Client.Device.MGI.Logging.Command.Save:
				return Format(MessageType.Command, new Command(CommandType.Save));
			case Client.Device.MGI.Logging.Command.Disconnect:
				return Format(MessageType.Command, new Command(CommandType.Disconnect));
			case Client.Device.MGI.Logging.Command.ShowConsole:
				return Format(MessageType.Command, new Command(CommandType.ShowConsole));
			case Client.Device.MGI.Logging.Command.SaveBdd:
				return Format(MessageType.Command, new Command(CommandType.SaveBdd));
			default:
				throw new ArgumentOutOfRangeException(nameof(data));
		}
	}

	private static string Format(MessageType type, ITcpable content)
		=> new Message(type, content).ToTcp();
}