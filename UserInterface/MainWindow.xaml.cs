using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
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

	[DllImport("kernel32")] public static extern bool AllocConsole();
	public MainWindow()
	{
		AllocConsole();
		Console.WriteLine("Console is alloced.");

		Console.WriteLine("Creating UIManager...");
		UIManager = new();

		Console.WriteLine("Initing components...");
		InitializeComponent();

		this.AuthPanel.Visibility = Visibility.Collapsed;
		this.TunnelingPanel.Visibility = Visibility.Collapsed;

		//Console.WriteLine("Setting visible panel...");
		//SetVisiblePanel();
	}
	private void onNewAction()
	{
		AuthErrorTB.Visibility = Visibility.Hidden;
		AuthErrorTB.Text = string.Empty;
	}

	protected override async void OnInitialized(EventArgs e)
	{
		await UIManager.TryLoadUser();
		await UIManager.LoadNodes();

		this.ServersListSP.Children.Clear();
		foreach(var node in UIManager.Nodes) {
			var space = node.Id.ToString().Length > 1 ? "" : " ";
			var block = new TextBlock() { Text = $"#{node.Id}{space} {node.Name}" };
			block.MouseDown += async (s, e) => {
				if(e.ClickCount == 2) {
					WrapperGrid.IsEnabled = false;
					await UIManager.ConnectToSelectedNode(node.Id);
					await Task.Delay(1000); // TODO: REMOVE IF UNNEEDED
					WrapperGrid.IsEnabled = true;
				}
			};

			this.ServersListSP.Children.Add(block);
		}

		UIManager.ActiveNodeChanged += (nodeId) => {
			try {
				for(int i = 0; i < this.ServersListSP.Children.Count; i++) {
					// dont ask... + dont care
					var node = int.Parse(((TextBlock)this.ServersListSP.Children[i]).Text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0].Trim("# ".ToCharArray()));
					if(nodeId.HasValue && node == nodeId.Value) {
						((TextBlock)this.ServersListSP.Children[i])
						.Background = new SolidColorBrush(Colors.LightGreen);
					} else {
						((TextBlock)this.ServersListSP.Children[i])
						.Background = new SolidColorBrush(Colors.White);
					}
				}
			} catch { }
		};

		base.OnInitialized(e);
		SetVisiblePanel();
		UIManager.StateChanged += (state) => Dispatcher.Invoke(this.SetVisiblePanel);
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
		var passwordTB = PasswordTB.Text;

		try {
			await UIManager.LoginAngRegister(email, passwordTB);
		} catch(UserException ex) {
			AuthErrorTB.Visibility = Visibility.Visible;
			AuthErrorTB.Text = ex.Message;
			return;
		}

		this.UserEmailLB.Text = this.UIManager.UserInfo.Email;
		this.UserAccessLevelLB.Text = this.UIManager.UserInfo.GetAccessLevel().ToString();

		SetVisiblePanel();
	}

	private void asGuestBT_Click(object sender, RoutedEventArgs e)
	{

	}


	protected override async void OnClosing(CancelEventArgs e)
	{
		try {
			await UIManager.Disconnect();
		} catch { }

		base.OnClosing(e);
	}
}