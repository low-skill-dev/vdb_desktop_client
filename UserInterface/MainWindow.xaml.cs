using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UserInterface;

// ! entry-point
public partial class MainWindow : Window
{
	private readonly UserInterfaceManager UIManager;
	private readonly System.Windows.Forms.NotifyIcon ni;
	[DllImport("kernel32")] private static extern bool AllocConsole();
	[DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	[DllImport("User32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
	public MainWindow()
	{
		#region single instance ensure
		string procName = Process.GetCurrentProcess().ProcessName;
		Process[] processes = Process.GetProcessesByName(procName);
		if(processes.Length > 1) {
			foreach(Process process in processes) {
				try {
					const int SW_SHOWNORMAL = 1;
					ShowWindow(process.MainWindowHandle, SW_SHOWNORMAL);
					SetForegroundWindow(process.MainWindowHandle);
				} catch {

				}
			}
			return;
		}
		#endregion

#if DEBUG
		AllocConsole();
#endif
		Console.WriteLine("Console is alloced.");

		this.UIManager = new();
		this.UIManager.StateChanged += (state) => {
			this.Dispatcher.Invoke(() =>
				Console.WriteLine($"\nState changed to: {this.UIManager.State}.\n"));
		};

		this.InitializeComponent();
		Console.WriteLine("Inited components.");

		#region minimized icon creator
		var Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
		this.ni = new System.Windows.Forms.NotifyIcon();
		this.ni.Icon = Icon;
		this.ni.Visible = true;
		this.ni.DoubleClick +=
			delegate (object sender, EventArgs args) {
				this.Show();
				this.WindowState = WindowState.Normal;
			}
		!;
		Console.WriteLine("Created icons.");
		#endregion
	}
	private void onNewAction()
	{
		this.AuthErrorTB.Visibility = Visibility.Hidden;
		this.AuthErrorTB.Text = string.Empty;
	}

	protected override async void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		Console.WriteLine("OnInitialized called.");
		this.UIManager.StateChanged += (state) => this.Dispatcher.Invoke(this.SetVisiblePanel);
		this.UIManager.StateChanged += (state) => {
			if(state != UserInterfaceManager.States.Authentication) {
				this.Dispatcher.Invoke(this.OnAuthed);
			}
		};
		this.AuthPanel.Visibility = Visibility.Collapsed;
		this.TunnelingPanel.Visibility = Visibility.Collapsed;

		Console.WriteLine("Trying to load user.");
		_ = await this.UIManager.TryLoadUser();

		this.UIManager.ActiveNodeChanged += (nodeId) => {
			try {
				for(int i = 0; i < this.ServersListSP.Children.Count; i++) {
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
			} catch { }
		};

		this.SetVisiblePanel();
	}


	private async void OnAuthed()
	{
		if(this.UIManager.Nodes is null) {
			_ = await this.UIManager.LoadNodes();
		}

		this.UserEmailLB.Text = this.UIManager.UserInfo?.Email;
		this.UserAccessLevelLB.Text = this.UIManager.UserInfo?.GetAccessLevel().ToString();

		this.ServersListSP.Children.Clear();
		if(this.UIManager.Nodes is null || this.UIManager.Nodes.Length < 1) {
			_ = this.ServersListSP.Children.Add(new TextBlock() {
				Text = "Sorry, there are no servers available.",
				Margin = new(0, 5, 0, 0),
				FontWeight = FontWeight.FromOpenTypeWeight(600),
				FontSize = 18
			});
			return;
		}
		foreach(var node in this.UIManager.Nodes) {
			var space = node.Id.ToString().Length > 1 ? "" : " ";
			// https://www.autoitscript.com/autoit3/docs/appendix/fonts.htm
			var border = new Border() {
				BorderThickness = new(1),
				BorderBrush = new SolidColorBrush(Colors.Black),
				Margin = new(0, 5, 0, 0),
			};
			var block = new TextBlock() {
				Text = $"#{node.Id}{space} {node.Name}",
				FontFamily = new System.Windows.Media.FontFamily(@"Lucida Console"),
				FontWeight = FontWeight.FromOpenTypeWeight(600),
				Padding = new(5),
			};
			block.MouseDown += async (s, e) => {
				if(e.ClickCount == 2) {
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

		switch(this.UIManager.State) {
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
		this.onNewAction();

		var email = this.EmailTB.Text;
		var passwordTB = this.PasswordTB.Password;

		try {
			_ = await this.UIManager.LoginAngRegister(email, passwordTB);
		} catch(UserException ex) {
			this.AuthErrorTB.Visibility = Visibility.Visible;
			this.AuthErrorTB.Text = ex.Message;
			return;
		}

		this.SetVisiblePanel();
	}

	private void asGuestBT_Click(object sender, RoutedEventArgs e)
	{

	}


	protected override async void OnClosing(CancelEventArgs e)
	{
		this.ni.Visible = false;

		try {
			_ = await this.UIManager.EnsureDisconnected();
		} catch { }

		base.OnClosing(e);
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
		try {
			_ = await this.UIManager.LogOut();
			Console.WriteLine("Logged out successfully. ");

			this.SetVisiblePanel();
		} catch { }
		this.WrapperGrid.IsEnabled = true;
	}

	protected override void OnStateChanged(EventArgs e)
	{
		if(this.WindowState == System.Windows.WindowState.Minimized)
			this.Hide();

		base.OnStateChanged(e);
	}
}