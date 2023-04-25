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

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private UserInterfaceManager UIManager;
    private ConfigGenerator configGenerator;
    private TunnelManager tunnelManager;
	public MainWindow()
    {
        UIManager = new();
		tunnelManager = new();
        OnCreate();


		InitializeComponent();
	}
    private async void OnCreate()
    {
        this.configGenerator = await ConfigGenerator.Create();
	}



    private void SetVisiblePanel()
    {
        return;

        this.AuthPanel.Visibility = Visibility.Collapsed;

        switch (UIManager.State) {
            case UserInterfaceManager.States.Authentication:
                this.AuthPanel.Visibility = Visibility.Visible; 
                break;
        }
    }

	private async void LoginBT_Click(object sender, RoutedEventArgs e)
	{
        var email = EmailTB.Text;
        var passwordTB = PasswordTB.Text;

        try {
            await UIManager.Login(email, passwordTB);
        } catch(UserException ex) {
            AuthErrorTB.Text = ex.Message;
            return;
        }

        SetVisiblePanel();
	}

	private void asGuestBT_Click(object sender, RoutedEventArgs e)
	{

	}
}