using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WireguardManipulator;

namespace UserInterface;

// ! entry-point
public partial class MainWindow : Window
{
	private UserInterfaceManager UIManager;
	System.Windows.Forms.NotifyIcon ni;
	[DllImport("kernel32")] public static extern bool AllocConsole();
	public MainWindow()
	{
#if DEBUG
		AllocConsole();
#endif
		Console.WriteLine("Console is alloced.");

		Console.WriteLine("Creating UIManager...");
		UIManager = new();
		UIManager.StateChanged += (state) => { Dispatcher.Invoke(()=>
			Console.WriteLine($"\nState changed to: {UIManager.State}.\n")); };

		Console.WriteLine("Initing components...");
		InitializeComponent();
		Console.WriteLine("Inited components.");


		var Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
		ni = new System.Windows.Forms.NotifyIcon();
		ni.Icon = Icon;
		ni.Visible = true;
		ni.DoubleClick += 
			delegate (object sender, EventArgs args) {
				this.Show();
				this.WindowState = WindowState.Normal;
			};
	}
	private void onNewAction()
	{
		AuthErrorTB.Visibility = Visibility.Hidden;
		AuthErrorTB.Text = string.Empty;
	}

	protected override async void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		UIManager.StateChanged += (state) => Dispatcher.Invoke(this.SetVisiblePanel);
		UIManager.StateChanged += (state) => {
			if(state != UserInterfaceManager.States.Ready) {
				Dispatcher.Invoke(this.OnAuthed);
			}
		};
		this.AuthPanel.Visibility = Visibility.Collapsed;
		this.TunnelingPanel.Visibility = Visibility.Collapsed;

		await UIManager.TryLoadUser();

		UIManager.ActiveNodeChanged += (nodeId) => {
			try {
				for(int i = 0; i < this.ServersListSP.Children.Count; i++) {
					// dont ask... + dont care
					var node = int.Parse(((TextBlock)((Border)this.ServersListSP.Children[i]).Child)
						.Text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0]
						.Trim("# ".ToCharArray()));
					if(nodeId.HasValue && node == nodeId.Value) {
						((TextBlock)((Border)this.ServersListSP.Children[i]).Child)
						.Background = new SolidColorBrush(Colors.LightGreen);
					} else {
						((TextBlock)((Border)this.ServersListSP.Children[i]).Child)
						.Background = new SolidColorBrush(Colors.White);
					}
				}
			} catch { }
		};

		this.SetVisiblePanel();
	}


	private async void OnAuthed()
	{
		if(UIManager.Nodes is null) {
			await UIManager.LoadNodes();
		}

		this.UserEmailLB.Text = this.UIManager.UserInfo?.Email;
		this.UserAccessLevelLB.Text = this.UIManager.UserInfo?.GetAccessLevel().ToString();

		this.ServersListSP.Children.Clear();
		if(UIManager.Nodes.Length < 1) {
			this.ServersListSP.Children.Add(new TextBlock() {
				Text = "Sorry, there are no servers available.",
				Margin = new(0, 5, 0, 0),
				FontWeight = FontWeight.FromOpenTypeWeight(600),
				FontSize = 18
			});
		}
		foreach(var node in UIManager.Nodes) {
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
					WrapperGrid.IsEnabled = false;
					await UIManager.ConnectToSelectedNode(node.Id);
					await Task.Delay(1000); // TODO: REMOVE IF UNNEEDED
					WrapperGrid.IsEnabled = true;
				}
			};

			border.Child = block;
			this.ServersListSP.Children.Add(border);
		}
	}


	private void SetVisiblePanel()
	{
		this.AuthPanel.Visibility = Visibility.Collapsed;
		this.TunnelingPanel.Visibility = Visibility.Collapsed;

		switch(UIManager.State) {
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
		onNewAction();

		var email = EmailTB.Text;
		var passwordTB = PasswordTB.Password;

		try {
			await UIManager.LoginAngRegister(email, passwordTB);
		} catch(UserException ex) {
			AuthErrorTB.Visibility = Visibility.Visible;
			AuthErrorTB.Text = ex.Message;
			return;
		}

		SetVisiblePanel();
	}

	private void asGuestBT_Click(object sender, RoutedEventArgs e)
	{

	}


	protected override async void OnClosing(CancelEventArgs e)
	{
		ni.Visible = false;

		try {
			await UIManager.Disconnect();
		} catch { }

		base.OnClosing(e);
	}

	private void EmailTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if(e.Key == Key.Enter) LoginBT_Click(sender, e);
	}

	private void PasswordTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if(e.Key == Key.Enter) LoginBT_Click(sender, e);
	}

	private async void LogOutBT_Click(object sender, RoutedEventArgs e)
	{
		WrapperGrid.IsEnabled = false;
		try {
			await UIManager.Disconnect();
			await UIManager.LogOut();
			Console.WriteLine("Logged out successfully. ");

			this.SetVisiblePanel();
		} catch { }
		WrapperGrid.IsEnabled = true;
	}

	protected override void OnStateChanged(EventArgs e)
	{
		if(WindowState == System.Windows.WindowState.Minimized)
			this.Hide();

		base.OnStateChanged(e);
	}
}