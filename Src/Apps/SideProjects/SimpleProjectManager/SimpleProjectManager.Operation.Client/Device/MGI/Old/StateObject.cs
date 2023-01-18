using System.Text;
using SimpleProjectManager.Operation.Client.Device.MGI.Logging;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Old;

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