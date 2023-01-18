using System.Collections.Concurrent;
using System.Text;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Old;

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