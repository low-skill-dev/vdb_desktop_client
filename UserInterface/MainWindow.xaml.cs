using ApiQuerier.Services;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace UserInterface;

// ! entry-point
public partial class MainWindow : Window
{
	private static string WorkingDirectoryPath => Environment.CurrentDirectory;
	private static string LogPath => Path.Join(WorkingDirectoryPath, @"vdb.log");

	private UserInterfaceManager UIManager;
	private readonly System.Windows.Forms.NotifyIcon ni;

	private System.Drawing.Icon InactiveIcon;
	private System.Drawing.Icon ActiveIcon;

	[DllImport("kernel32")] private static extern bool AllocConsole();
	[DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	[DllImport("User32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
	public MainWindow()
	{

		ServicePointManager.ServerCertificateValidationCallback +=
			(_, _, _, _) => true;

#if DEBUG
		AllocConsole();
#endif

		Trace.Listeners.Add(new ConsoleTraceListener());
		Trace.WriteLine("Trace is listened by console.");

		// file logging

		if(File.Exists(LogPath) && (new FileInfo(LogPath).Length / 1024 / 1024) > 10)
		{
			Trace.WriteLine("Log file will be trimmed.");
			var lines = File.ReadAllLines(LogPath);
			lines = lines.Skip(lines.Length / 4 * 3).ToArray();
			File.WriteAllLines(LogPath, lines);
		}

		Stream logFile = File.Exists(LogPath) ? File.OpenWrite(LogPath) : File.Create(LogPath);

		var fileListener = new TextWriterTraceListener(logFile);
		fileListener.TraceOutputOptions =
			TraceOptions.LogicalOperationStack |
			TraceOptions.DateTime;

		Trace.Listeners.Add(fileListener);
		Trace.AutoFlush = true;
		//

		#region single instance ensure
		string procName = Process.GetCurrentProcess().ProcessName;
		Process[] processes = Process.GetProcessesByName(procName);
		if(processes.Length > 1)
		{
			foreach(Process process in processes)
			{
				try
				{
					const int SW_SHOWNORMAL = 1;
					ShowWindow(process.MainWindowHandle, SW_SHOWNORMAL);
					SetForegroundWindow(process.MainWindowHandle);
				}
				catch
				{

				}
			}
			return;
		}
		#endregion

		Console.WriteLine("Console is alloced.");

		this.UIManager = new();
		this.UIManager.StateChanged += (state) =>
		{
			this.Dispatcher.Invoke(() =>
				Console.WriteLine($"\nState changed to: {this.UIManager.State}.\n"));
		};

		this.InitializeComponent();
		Console.WriteLine("Inited components.");

		#region minimized icon creator
		this.InactiveIcon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath)!;
		var t = this.InactiveIcon.ToBitmap();
		ChangeBlueToRedOnBitmap(t);
		this.ActiveIcon = System.Drawing.Icon.FromHandle(t.GetHicon());
		this.ni = new System.Windows.Forms.NotifyIcon();
		this.ni.Icon = InactiveIcon;
		this.ni.Text = "Disconnected.";
		this.ni.Visible = true;
		this.ni.ContextMenuStrip = new()
		{
			AutoSize = true
		};
		this.ni.ContextMenuStrip.Items.Add("Hide").Click +=
			delegate (object sender, EventArgs args)
			{
				var sdr = (ToolStripItem)sender;

				//if(args.Clicks != 0) return;
				if(sdr.Text == "Show")
				{
					sdr.Text = "Hide";
					this.Show();
					this.WindowState = WindowState.Normal;
				}
				else
				{
					sdr.Text = "Show";
					this.WindowState = WindowState.Minimized;
					this.Hide();
				}
			}
		!;
		this.ni.ContextMenuStrip.Items.Add("Exit").Click +=
			delegate (object sender, EventArgs args)
			{
				this.Close();
				System.Windows.Forms.Application.Exit();
			}
		!;
		this.ni.MouseDoubleClick += this.OnIconSwitchClick;
		Console.WriteLine("Created icons.");
		#endregion

		ScheduleNetCheck();
	}
	private void onNewAction()
	{
		this.AuthErrorTB.Visibility = Visibility.Hidden;
		this.AuthErrorTB.Text = string.Empty;
	}
	private void ChangeBlueToRedOnBitmap(Bitmap blueBm)
	{
		DateTime start = DateTime.UtcNow;
		for(int w = 0; w < blueBm.Width; w++)
		{
			for(int h = 0; h < blueBm.Height; h++)
			{
				var p = blueBm.GetPixel(w, h);
				if(p.B > 250 && p.R < 50 && p.G < 50)
				{
					blueBm.SetPixel(w, h, System.Drawing.Color.FromArgb(255, 255, 0, 0));
				}
			}
		}

		Console.WriteLine($"Took {(DateTime.UtcNow - start).TotalMilliseconds.ToString("0.00")} ms to created active icon.");
	}

	protected override async void OnInitialized(EventArgs e)
	{
		this.UIManager ??= new();

		(await AuthTokenProvider.Create()).AccessTokenChanged += (t) =>
		{
			if(this.UIManager.State == UserInterfaceManager.States.Authentication)
				this.UIManager.Register().Wait();
		};
		(await AuthTokenProvider.Create()).AccessTokenChanged += async (t) =>
		{
			this.UIManager.UserInfo = (await AuthTokenProvider.Create()).CurrentUser;
		};

		if(this.UIManager.UserInfo is null)
		{
			this.UIManager.State = UserInterfaceManager.States.Authentication;
			this.SetVisiblePanel();
		}

		base.OnInitialized(e);
		Console.WriteLine("OnInitialized called.");

		this.UIManager.StateChanged += (state) => this.Dispatcher.Invoke(this.SetVisiblePanel);
		this.UIManager.StateChanged += (state) =>
		{
			if(state != UserInterfaceManager.States.Authentication)
			{
				this.Dispatcher.Invoke(this.OnAuthed);
			}
		};
		this.AuthPanel.Visibility = Visibility.Collapsed;
		this.TunnelingPanel.Visibility = Visibility.Collapsed;

		Console.WriteLine("Trying to load user.");
		_ = await this.UIManager.TryLoadUser();

		this.UIManager.ActiveNodeChanged += (nodeId) =>
		{
			Dispatcher.Invoke(() =>
			{
				try
				{
					for(int i = 0; i < this.ServersListSP.Children.Count; i++)
					{
						var nodeComponent = (TextBlock)((Border)this.ServersListSP.Children[i]).Child;

						// dont ask... + dont care
						var node = int.Parse(nodeComponent
							.Text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0]
							.Trim("# ".ToCharArray()));

						nodeComponent.Background = new SolidColorBrush(
							nodeId.HasValue && node == nodeId.Value
							? Colors.LightGreen
							: Colors.White);
					}
				}
				catch { }
			});
		};

		this.UIManager.ActiveNodeChanged += (_) => SetProperIcon();

		this.SetVisiblePanel();
	}

	private async void OnIconSwitchClick(object? sender, EventArgs args)
	{
		if(!(await AuthTokenProvider.Create()).IsAuthenticated) return;
		if(!this.WrapperGrid.IsEnabled) return;
		try
		{
			this.WrapperGrid.IsEnabled = false;
			await this.UIManager.ConnectToSelectedNode(await this.UIManager.LastConnectedNode());
		}
		finally
		{
			this.WrapperGrid.IsEnabled = true;
		}
	}

	private static readonly ConcurrentDictionary<string, int>
		_usedPingHosts = new(Environment.ProcessorCount, TopSites.Sites.Length);
	private static IEnumerable<string>? _orderedPingHosts;
	private static DateTime _pingHostsLastOrdered;
	private static async Task<bool> CheckNetwork(bool log = true)
	{
		var cancelSource = new CancellationTokenSource();
		if(_orderedPingHosts is null || (DateTime.UtcNow - _pingHostsLastOrdered).TotalMinutes > 5)
		{
			_orderedPingHosts = TopSites.Sites.OrderBy(x => _usedPingHosts.TryGetValue(x, out var v) ? v : 0).Take(8).ToList();
			_pingHostsLastOrdered = DateTime.UtcNow;
			if(log) Console.WriteLine($"NetCheck: ping hosts reordered.");
		}

		var tasks = _orderedPingHosts.Select(x => new Action(() =>
		{
			if(cancelSource.IsCancellationRequested) return;

			_usedPingHosts.TryAdd(x, 0);
			_usedPingHosts[x]++;

			if(log) Console.WriteLine($"NetCheck: ping '{x}'.");
			bool success = false;
			try { success = new Ping().Send(x, 1000).Status == 0; }
			catch { }
			if(!cancelSource.IsCancellationRequested)
			{
				if(success)
				{
					cancelSource.Cancel();
					if(log) Console.WriteLine($"NetCheck: ping succeeded.");
				}
				else
				{
					if(log) Console.WriteLine($"NetCheck: ping failed.");
				}
			}
		}));

		try
		{
			await Task.Run(() => Parallel.ForEach(tasks, new ParallelOptions
			{
				CancellationToken = cancelSource.Token,
				MaxDegreeOfParallelism = 1,
			}, x => x()));
		}
		catch(OperationCanceledException) { }

		// cancelled if ping was successful
		return cancelSource.IsCancellationRequested;
	}

	public async void ScheduleNetCheck()
	{
		Console.WriteLine($"{nameof(PerformNetCheck)}: scheduled.");
		while(true)
		{
			await Task.Delay(2000);
			Console.WriteLine($"{nameof(PerformNetCheck)}: iteration.");

#if RELEASE
			var gapMin = 1;
			var delay = 3*1000;
#else
			var gapMin = 0;
			var delay = 1000;
#endif

			if(this.UIManager.IsConnected &&
				(DateTime.UtcNow - this.UIManager.LastConnected).TotalMinutes > gapMin)
				await PerformNetCheck();
			else
				await Task.Delay(delay);
		}
	}

	public async Task PerformNetCheck()
	{
		if(this.UIManager.IsConnected == false) return;
		if(await CheckNetwork()) return;
		Console.WriteLine($"NetCheck: all pings failed.");
		if(this.UIManager.IsConnected == false) return;

		this.WrapperGrid.IsEnabled = false;
		await this.UIManager.EnsureDisconnected();
#if RELEASE
		const int limit = 60;
#else
		const int limit = 3;
#endif
		for(int i = 1; i <= limit; i += 1)
		{
			Console.WriteLine($"NetCheck: waiting for the internet.");

			if(i == limit)
			{
				new ToastContentBuilder()
					.AddText("VPN disconnected.")
					.AddText("Click to reconnect now.")
					.Show(t => t.Activated += async (sender, args) =>
					{
						if(!this.UIManager.IsConnected)
						{
							Console.WriteLine($"NetCheck: reconnection invoked.");
							await this.UIManager.ConnectToSelectedNode(await this.UIManager.LastConnectedNode());
						}
					});
				this.WrapperGrid.IsEnabled = true;
				return;
			}

			if(await CheckNetwork(false)) break;
			await Task.Delay(3000);
		}

		Console.WriteLine($"NetCheck: reconnection invoked.");

		this.WrapperGrid.IsEnabled = false;
		await this.UIManager.ConnectToSelectedNode(await this.UIManager.LastConnectedNode());
		this.WrapperGrid.IsEnabled = true;
	}

	private void SetProperIcon()
	{
		if(this.UIManager.CurrentlyConnectedNode is null)
		{
			this.ni.Icon = InactiveIcon;
			this.ni.Text = "Disconnected.";
		}
		else
		{
			this.ni.Icon = ActiveIcon;
			string text = "Connected.";
			try
			{
				text = $"Connected to \'{this.UIManager.Nodes.Single(x => x.Id == this.UIManager.CurrentlyConnectedNode).Name}\'";
			}
			catch { }
			this.ni.Text = text;
		}
	}
	private async void OnAuthed()
	{
		if(this.UIManager.Nodes is null)
		{
			_ = await this.UIManager.LoadNodes();
		}

		this.UserEmailLB.Text = this.UIManager.UserInfo?.Email;
		this.UserAccessLevelLB.Text = this.UIManager.UserInfo?.GetAccessLevel().ToString();

		this.ServersListSP.Children.Clear();
		if(this.UIManager.Nodes is null || this.UIManager.Nodes.Length < 1)
		{
			_ = this.ServersListSP.Children.Add(new TextBlock()
			{
				Text = "Sorry, there are no servers available.",
				Margin = new(0, 5, 0, 0),
				FontWeight = FontWeight.FromOpenTypeWeight(600),
				FontSize = 18
			});
			return;
		}
		foreach(var node in this.UIManager.Nodes)
		{
			var space = node.Id.ToString().Length > 1 ? "" : " ";
			// https://www.autoitscript.com/autoit3/docs/appendix/fonts.htm
			var border = new Border()
			{
				BorderThickness = new(1),
				BorderBrush = new SolidColorBrush(Colors.Black),
				Margin = new(0, 5, 0, 0),
			};
			var block = new TextBlock()
			{
				Text = $"#{node.Id}{space} {node.Name}",
				FontFamily = new System.Windows.Media.FontFamily(@"Lucida Console"),
				FontWeight = FontWeight.FromOpenTypeWeight(600),
				Padding = new(5),
			};
			block.MouseDown += async (s, e) =>
			{
				if(e.ClickCount == 2)
				{
					this.WrapperGrid.IsEnabled = false;
					_ = await this.UIManager.ConnectToSelectedNode(node.Id);
					await Task.Delay(1000); // TODO: REMOVE IF UNNEEDED
					this.WrapperGrid.IsEnabled = true;
				}
			};

			border.Child = block;
			_ = this.ServersListSP.Children.Add(border);
		}
	}


	private void SetVisiblePanel()
	{
		this.AuthPanel.Visibility = Visibility.Collapsed;
		this.TunnelingPanel.Visibility = Visibility.Collapsed;

		switch(this.UIManager.State)
		{
			case UserInterfaceManager.States.Authentication:
				this.AuthPanel.Visibility = Visibility.Visible;
				break;
			case UserInterfaceManager.States.Tunneling:
			case UserInterfaceManager.States.Ready:
				this.TunnelingPanel.Visibility = Visibility.Visible;
				break;
		}
	}

	private async void LoginBT_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			this.WrapperGrid.IsEnabled = false;

			this.onNewAction();

			var email = this.EmailTB.Text;
			var passwordTB = this.PasswordTB.Password;

			try
			{
				_ = await this.UIManager.LoginAngRegister(email, passwordTB);
			}
			catch(UserException ex)
			{
				this.AuthErrorTB.Visibility = Visibility.Visible;
				this.AuthErrorTB.Text = ex.Message;
				return;
			}

			this.SetVisiblePanel();
		}
		finally
		{
			this.WrapperGrid.IsEnabled = true;
		}
	}

	private void asGuestBT_Click(object sender, RoutedEventArgs e)
	{

	}


	protected override async void OnClosing(CancelEventArgs e)
	{
		try
		{
			this.UIManager.tunnelManager.DeleteConfigFile();
		}
		catch { }
		try
		{
			this.ni.Visible = false;
		}
		catch { }

		base.OnClosing(e);

		await this.UIManager.EnsureDisconnected();
	}

	private void EmailTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if(e.Key == Key.Enter) this.LoginBT_Click(sender, e);
	}

	private void PasswordTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if(e.Key == Key.Enter) this.LoginBT_Click(sender, e);
	}

	private async void LogOutBT_Click(object sender, RoutedEventArgs e)
	{
		this.WrapperGrid.IsEnabled = false;
		try
		{
			_ = await this.UIManager.LogOut();
			Console.WriteLine("Logged out successfully. ");

			this.SetVisiblePanel();
		}
		catch { }
		this.WrapperGrid.IsEnabled = true;
	}

	protected override void OnStateChanged(EventArgs e)
	{
		if(this.WindowState == System.Windows.WindowState.Minimized)
		{
			this.ni.ContextMenuStrip.Items[0].Text = "Show";
			this.Hide();
		}
		else
		{
			this.ni.ContextMenuStrip.Items[0].Text = "Hide";
		}
		base.OnStateChanged(e);
	}
}