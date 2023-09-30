using ApiModels.Device;
using ApiQuerier.Helpers;
using ApiQuerier.Models;
using ApiQuerier.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using UserInterface.Services;
using WireguardManipulator;

namespace UserInterface.Windows;

public partial class AuthWindow : Window
{
	public AuthWindow()
	{
		InitializeComponent();
	}

	protected override void OnInitialized(EventArgs e)
	{
		Global.CurrentWindowNameof = nameof(AuthWindow);

		base.OnInitialized(e);
	}

	private void SwitchToTunnelingWindow()
	{
		base.Hide();

		var tw = new TunnelingWindow(this);
		tw.ShowDialog();

		base.Show();
	}

	private void SetErrorMessage(string? msg = null)
	{
		if(string.IsNullOrWhiteSpace(msg))
		{
			this.AuthErrorTB.Visibility = Visibility.Hidden;
			//this.AuthErrorTB.Text = string.Empty;
		}
		else
		{
			this.AuthErrorTB.Visibility = Visibility.Visible;
			this.AuthErrorTB.Text = msg;
		}
	}

	private async void LoginBT_Click(object sender, RoutedEventArgs e)
	{
		LoginBT.IsEnabled = false;
		SetErrorMessage();

		var email = this.EmailTB.Text;
		var password = this.PasswordTB.Password;

		Trace.WriteLine($"Authentication has been initiated by {nameof(AuthWindow)}.");

		try
		{
			DataValidator.ValidateEmail(email);
			DataValidator.ValidatePassword(password);

			var authResult = await AuthTokenProvider.AuthenticateAsync(email, password);

			AuthResponseToThrow(AuthTokenProvider.LastStatusCode, authResult);


			var aht = await ApiHelperTransient.Create();
			var regResult = aht?.RegisterDevice(new AddDeviceRequest
			{
				WireguardPublicKey = new TunnelManager().PublicKey
			});

			RegResponseToThrow(aht?.LastStatusCode ?? 0);


			SwitchToTunnelingWindow();
		}
		catch(UserException ex)
		{
			SetErrorMessage(ex.Message + " " + (ex.InnerException?.Message ?? ""));
		}
		finally
		{
			LoginBT.IsEnabled = true;
		}
	}

	private static bool wasLoggingEnabled = false;
	[DllImport("kernel32.dll")] static extern bool AllocConsole();
	private void LoggingBT_Click(object sender, RoutedEventArgs e)
	{
		if(wasLoggingEnabled) return;

		wasLoggingEnabled = true;

		AllocConsole();
		Trace.Listeners.Add(new ConsoleTraceListener());
		Trace.WriteLine("Logging enabled.");
	}

	#region UX features

	private void PasswordTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if(e.Key == Key.Enter) this.LoginBT_Click(sender, e);
	}

	private void EmailTB_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
	{
		if(e.Key == Key.Enter) this.LoginBT_Click(sender, e);
	}

	private static void AuthResponseToThrow(int status, AuthResult? response)
	{
		if((status >= 300 && status <= 399) || (status >= 500 && status <= 599))
		{
			throw new WebException("Authentication server is not reachable.");
		}
		else if(status == 401)
		{
			throw new WebException("Wrong email or password.");
		}
		else if(status == 404) // legacy
		{
			throw new WebException("User not found. Please visit the website and sign up.");
		}
		else if(status >= 400 && status <= 499)
		{
			throw new WebException("Your request was reject by the server.");
		}
		else if((!(status >= 200 && status <= 299)) || response == null)
		{
			throw new WebException($"Unknown error occurred during the request: HTTP_{status}.");
		}
		else if(response?.RefreshToken is null || response?.AccessToken is null)
		{
			throw new WebException("Server did not send refresh token which was expected.");
		}
	}

	private static void RegResponseToThrow(int status)
	{
		if(status == 409)
		{
			throw new WebException("Devices limit reached. Please visit you personal area on the website to delete one of the the devices.");
		}
		else if(status == 303)
		{
			throw new WebException("The generated key was rejected by the server. Please restart the program to create a new one.");
		}
		if(!(status >= 200 && status <= 299) && status != 302)
		{
			throw new WebException("Unknown error occurred during the request.");
		}
	}

	#endregion
}
