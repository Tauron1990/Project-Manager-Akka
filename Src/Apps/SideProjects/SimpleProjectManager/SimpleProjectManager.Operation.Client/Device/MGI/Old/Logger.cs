using System.Collections.Concurrent;
using System.Text;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ToPort;

public class LogInfo
{
	public string TimeStamp { get; private set; }

	public string Application { get; private set; }

	public string Type { get; private set; }

	public bool ToConsole { get; private set; }

	public string Content { get; private set; }

	public bool Permanent { get; private set; }

	public LogInfo(string timeStamp, string application, string type, bool toConsole, bool permanent, string content)
	{
		TimeStamp = timeStamp;
		Application = application.PadRight(15);
		Type = type.PadRight(15);
		ToConsole = toConsole;
		Permanent = permanent;
		Content = content;
	}

	public override string ToString()
	{
		return TimeStamp + ";" + Application + ";" + Type + ";" + Content;
	}
	
	public static string getTime()
	{
		DateTime now = DateTime.Now;
		return now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + ":" + now.Millisecond.ToString("000");
	}

	public static LogInfo CreateError(string error)
	{
		return new LogInfo(getTime(), "KernelLogger", "ERROR", toConsole: true, permanent: true, error);
	}

	public static LogInfo CreateSave()
	{
		return new LogInfo(getTime(), "KernelLogger", "SAVE", toConsole: true, permanent: true, "Saving Log");
	}

	public static string getTime()
	{
		DateTime now = DateTime.Now;
		return now.Hour.ToString("00") + ":" + now.Minute.ToString("00") + ":" + now.Second.ToString("00") + ":" + now.Millisecond.ToString("000");
	}
}

public class StateObject
{
	public Socket workSocket;

	public const int BufferSize = 1024;

	public byte[] buffer = new byte[1024];

	public StringBuilder tempBuilder = new StringBuilder();

	public string Application = "UknownApp";

	private int errorCount;

	private Timer heartBeatTmr;

	public void StopHeartBeat()
	{
		heartBeatTmr.Stop();
	}

	public StateObject()
	{
		heartBeatTmr = new Timer(1000.0);
		heartBeatTmr.AutoReset = true;
		heartBeatTmr.Elapsed += heartBeatTmr_Elapsed;
		heartBeatTmr.Start();
	}

	private void heartBeatTmr_Elapsed(object sender, ElapsedEventArgs e)
	{
		try
		{
			if (workSocket != null)
			{
				if (!SocketIsConnected(workSocket))
				{
					errorCount++;
					if (errorCount > 5)
					{
						Console.WriteLine("HeartBeat detected dead socket for " + Application);
						LoggerAsync.Log(LogInfo.CreateError("HeartBeat detected dead socket for " + Application));
						workSocket.Close();
					}
				}
				else
				{
					errorCount = 0;
				}
			}
			else
			{
				LoggerAsync.Log(LogInfo.CreateError("Client " + Application + " has null socket"));
				LoggerAsync.Save();
			}
		}
		catch (Exception ex)
		{
			LoggerAsync.Log(LogInfo.CreateError("Client " + Application + ", HeartBeat Exc " + ex.Message));
			LoggerAsync.Save();
		}
	}

	public static bool SocketIsConnected(Socket socket)
	{
		try
		{
			return !socket.Poll(1000, SelectMode.SelectRead) || socket.Available != 0;
		}
		catch (SocketException)
		{
			return false;
		}
	}
}

public class LoggerAsync
{
	public static List<SharpProcess> listProcessBluff;

	private static IntPtr threadLog = IntPtr.Zero;

	private static ConcurrentQueue<LogInfo> permanentBackLogs = new ConcurrentQueue<LogInfo>();

	private static int maxPermanentBackLogSize = 500;

	private static ConcurrentQueue<LogInfo> backLogs = new ConcurrentQueue<LogInfo>();

	private static int maxBackLogSize = 8000;

	private static ConcurrentQueue<LogInfo> logs = new ConcurrentQueue<LogInfo>();

	private static AutoResetEvent waitHandle = new AutoResetEvent(initialState: false);

	private static readonly object saveLocker = new object();

	private static bool running = false;

	public static bool Running => running;

	public static void Logging()
	{
		running = true;
		while (running)
		{
			if (logs.Count > 0)
			{
				try
				{
					if (!logs.TryDequeue(out var result) || result == null)
					{
						continue;
					}
					while (backLogs.Count >= maxBackLogSize)
					{
						backLogs.TryDequeue(out var _);
					}
					backLogs.Enqueue(result);
					if (result.Permanent)
					{
						while (permanentBackLogs.Count >= maxPermanentBackLogSize)
						{
							permanentBackLogs.TryDequeue(out var _);
						}
						permanentBackLogs.Enqueue(result);
					}
					if (result.ToConsole)
					{
						Console.WriteLine(result.ToString());
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error while logging : " + ex.Message);
				}
			}
			else
			{
				waitHandle.WaitOne();
			}
		}
	}

	public static void Save(string prefix = "D")
	{
		lock (saveLocker)
		{
			if (backLogs.Count <= 0)
			{
				return;
			}
			try
			{
				DateTime now = DateTime.Now;
				string text = ".\\Logs\\" + prefix + "-" + now.Year + "_" + now.Month.ToString("00") + "_" + now.Day.ToString("00") + "_T_" + now.Hour.ToString("00") + "_" + now.Minute.ToString("00") + "_" + now.Second.ToString("00") + ".csv";
				StringBuilder stringBuilder = new StringBuilder();
				LogInfo[] array = permanentBackLogs.ToArray();
				foreach (LogInfo logInfo in array)
				{
					if (logInfo != null)
					{
						stringBuilder.AppendLine(logInfo.ToString());
					}
				}
				stringBuilder.AppendLine();
				array = backLogs.ToArray();
				foreach (LogInfo logInfo2 in array)
				{
					if (logInfo2 != null)
					{
						stringBuilder.AppendLine(logInfo2.ToString());
					}
				}
				File.AppendAllText(text, stringBuilder.ToString(), Encoding.UTF8);
				Console.WriteLine("Log saved in " + text);
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR : Cannot save backlog : " + ex.Message);
			}
		}
	}

	public static void Log(LogInfo log)
	{
		if (running)
		{
			logs.Enqueue(log);
			waitHandle.Set();
		}
	}

	public static void Start()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (!Directory.Exists("Logs"))
		{
			Directory.CreateDirectory("Logs");
		}
		SharpProcess item = new SharpProcess(Logging);
		listProcessBluff = new List<SharpProcess>();
		listProcessBluff.Add(item);
		CarteReseauDLL.LaunchThreadSharp(ref threadLog, ref item);
	}

	public static void Stop()
	{
		if (running && threadLog != IntPtr.Zero)
		{
			running = false;
			waitHandle.Set();
			CarteReseauDLL.JoinThread(threadLog);
			threadLog = IntPtr.Zero;
			listProcessBluff.Clear();
		}
		running = false;
	}
