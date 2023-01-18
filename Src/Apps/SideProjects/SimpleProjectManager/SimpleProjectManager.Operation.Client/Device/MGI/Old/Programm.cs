using SimpleProjectManager.Operation.Client.Device.MGI.Logging;

namespace SimpleProjectManager.Operation.Client.Device.MGI.Old;

public class Programm
{
	private const int MF_BYCOMMAND = 0;

	public const int SC_CLOSE = 61536;

	private const int SW_HIDE = 0;

	private const int SW_SHOW = 5;

	private static IntPtr console;

	private static NotifyIcon notifyIcon;

	private static bool consoleShown;

	private static int ID_Printer = 0;

	public static readonly string[] SEPARATOR = new string[1] { "<!C>" };

	public static readonly string[] BEGINMSG = new string[1] { "<!M>" };

	public static readonly string BEGINMSGSTR = "<!M>";

	public static readonly string[] ENDMSG = new string[1] { "<M!>" };

	public static readonly string ENDMSGSTR = "<M!>";

	public static List<StateObject> clientStates;

	public static int connectedClients = 0;

	public static bool running;

	public static ManualResetEvent allDone = new ManualResetEvent(initialState: false);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	[DllImport("user32")]
	public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

	[DllImport("user32")]
	private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

	private static void Main(string[] args)
	{
		try
		{
			ushort port = 23421;
			console = GetConsoleWindow();
			Console.Title = "MGI Logger";
			DeleteMenu(GetSystemMenu(GetConsoleWindow(), bRevert: false), 61536, 0);
			HideConsole();
			try
			{
				ID_Printer = int.Parse(File.ReadAllText("IdPrinter.ini"));
			}
			catch
			{
				LoggerAsync.Log(LogInfo.CreateError("fail to parse id Printer"));
			}
			if(args.Length >= 1)
			{
				port = ushort.Parse(args[0]);
			}
			LoggerAsync.Start();
			notifyIcon = new NotifyIcon();
			notifyIcon.Text = "MGI Logger";
			notifyIcon.Icon = Resources.log_icon;
			notifyIcon.Visible = true;
			notifyIcon.ContextMenu = new ContextMenu();
			notifyIcon.ContextMenu.MenuItems.Add("Save Log", SaveClick);
			notifyIcon.ContextMenu.MenuItems.Add("Toggle Console", ToggleConsole);
			Task.Factory.StartNew(
				delegate
				{
					StartListening(port);
					Console.WriteLine("Done\n");
					LoggerAsync.Stop();
					notifyIcon.Visible = false;
					Application.Exit();
				});
			AppDomain.CurrentDomain.UnhandledException += MyHandler;
			Application.Run();
		}
		catch { }
	}

	private static void MyHandler(object sender, UnhandledExceptionEventArgs e)
	{
		if(e.ExceptionObject is Exception ex)
		{
			LoggerAsync.Log(LogInfo.CreateError(ex.Message + "\n" + ex.StackTrace));
			LoggerAsync.Save("L");
		}
	}

	public static void SaveClick(object sender, EventArgs e)
	{
		LoggerAsync.Save("I");
	}

	public static void ToggleConsole(object sender, EventArgs e)
	{
		if(consoleShown)
		{
			HideConsole();
		}
		else
		{
			ShowConsole();
		}
	}

	public static void ShowConsole()
	{
		ShowWindow(console, 5);
		consoleShown = true;
	}

	public static void HideConsole()
	{
		ShowWindow(console, 0);
		consoleShown = false;
	}

	public static void StartListening(ushort port)
	{
		try
		{
			_ = new byte[1024];
			IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
			IPEndPoint localEP = new IPEndPoint(iPAddress, port);
			Socket socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			running = true;
			clientStates = new List<StateObject>();
			socket.Bind(localEP);
			socket.Listen(100);
			while (running)
			{
				allDone.Reset();
				socket.BeginAccept(AcceptCallback, socket);
				allDone.WaitOne();
				if(connectedClients == 0)
				{
					running = false;
				}
			}
		}
		catch (Exception ex)
		{
			LoggerAsync.Log(LogInfo.CreateError(ex.ToString()));
		}
	}

	public static void AcceptCallback(IAsyncResult ar)
	{
		try
		{
			connectedClients++;
			allDone.Set();
			Socket socket = ((Socket)ar.AsyncState).EndAccept(ar);
			StateObject stateObject = new StateObject();
			clientStates.Add(stateObject);
			stateObject.workSocket = socket;
			socket.BeginReceive(stateObject.buffer, 0, 1024, SocketFlags.None, ReadCallback, stateObject);
		}
		catch (Exception ex)
		{
			LoggerAsync.Log(LogInfo.CreateError("Begin Receive exc : " + ex.Message));
			allDone.Set();
		}
	}

	public static void ReadCallback(IAsyncResult ar)
	{
		try
		{
			string empty = string.Empty;
			StateObject stateObject = (StateObject)ar.AsyncState;
			Socket workSocket = stateObject.workSocket;
			bool flag = false;
			int num = workSocket.EndReceive(ar);
			if(num > 0)
			{
				empty = Encoding.UTF8.GetString(stateObject.buffer, 0, num);
				stateObject.tempBuilder.Append(empty);
				string text = stateObject.tempBuilder.ToString();
				string[] array = text.Split(BEGINMSG, StringSplitOptions.RemoveEmptyEntries);
				List<string> list = new List<string>();
				for (int i = 0; i < array.Length; i++)
				{
					if(array[i].EndsWith(ENDMSGSTR))
					{
						list.Add(array[i].Remove(array[i].Length - ENDMSGSTR.Length, ENDMSGSTR.Length));
					}
				}
				int length = Math.Max(text.LastIndexOf(ENDMSGSTR), text.LastIndexOf(BEGINMSGSTR));
				stateObject.tempBuilder.Remove(0, length);
				int idInfo;
				string information;
				foreach (string item in list)
				{
					try
					{
						if(item == "")
						{
							continue;
						}
						string[] array2 = item.Split(SEPARATOR, StringSplitOptions.None);
						if(array2.Length == 0)
						{
							continue;
						}
						switch ((MessageType)Enum.Parse(typeof(MessageType), array2[0]))
						{
							case MessageType.Command:
							{
								CommandType commandType = (CommandType)Enum.Parse(typeof(CommandType), array2[1]);
								switch (commandType)
								{
									case CommandType.SetApp:
										stateObject.Application = array2[2];

										break;
									case CommandType.Save:
										LoggerAsync.Log(LogInfo.CreateSave());
										LoggerAsync.Save("S");

										break;
									case CommandType.Disconnect:
										stateObject.StopHeartBeat();
										clientStates.Remove(stateObject);
										connectedClients--;
										workSocket.Close();
										allDone.Set();
										LoggerAsync.Log(LogInfo.CreateError("Client " + stateObject.Application + " asked to close socket"));
										flag = true;

										break;
									case CommandType.ShowConsole:
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
							}
							case MessageType.Log:
								if(array2.Length < 6)
								{
									LoggerAsync.Log(new LogInfo(array2[1], stateObject.Application, array2[2], bool.Parse(array2[3]), permanent: false, array2[4]));
								}
								else
								{
									LoggerAsync.Log(new LogInfo(array2[1], stateObject.Application, array2[2], bool.Parse(array2[3]), bool.Parse(array2[4]), array2[5]));
								}

								break;
						}
					}
					catch (Exception ex)
					{
						LoggerAsync.Log(LogInfo.CreateError(ex.Message));
					}
				}
				if(!flag)
				{
					workSocket.BeginReceive(stateObject.buffer, 0, 1024, SocketFlags.None, ReadCallback, stateObject);
				}
			}
			else if(!StateObject.SocketIsConnected(workSocket))
			{
				LoggerAsync.Log(LogInfo.CreateError("Client " + stateObject.Application + " sent incorrect Message"));
				LoggerAsync.Save();
				stateObject.StopHeartBeat();
				clientStates.Remove(stateObject);
				connectedClients--;
				allDone.Set();
			}
		}
		catch (Exception ex2)
		{
			Console.WriteLine("Exc ReadCallback " + ex2.Message);
			LoggerAsync.Log(LogInfo.CreateError("Exc ReadCallback : " + ex2.Message));
			if(!(ar.AsyncState is StateObject stateObject2))
			{
				LoggerAsync.Log(LogInfo.CreateError("Cannot get StateObject from client"));
			}
			else
			{
				stateObject2.StopHeartBeat();
				clientStates.Remove(stateObject2);
				LoggerAsync.Log(LogInfo.CreateError("Client " + stateObject2.Application + " sent incorrect Message"));
			}
			LoggerAsync.Save("P");
			connectedClients--;
			allDone.Set();
		}
	}
}