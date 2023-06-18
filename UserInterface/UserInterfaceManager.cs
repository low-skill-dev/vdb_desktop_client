using ServerQuerier.Helpers;
using ServerQuerier.Models.Auth;
using ServerQuerier.Models.Nodes;
using UserInterface.Services;
using WireguardManipulator;

namespace UserInterface;
internal class WebException : UserException
{
	public WebException(string message) : base(message) { }
}
internal sealed class UserInterfaceManager
{
	#region enums
	public enum States
	{
		Authentication,
		Registering,
		Ready,
		Tunneling,
		Settings
	}
	#endregion

	#region stateful-providing props
	private States _state;
	private int? _currentlyConnectedNode;

	public States State {
		get {
			return this._state;
		}
		set {
			if(this._state != value) {
				this._state = value;
				StateChanged?.Invoke(value);
			}
		}
	}
	public int? CurrentlyConnectedNode {
		get {
			return this._currentlyConnectedNode;
		}
		set {
			if(this._currentlyConnectedNode != value) {
				this._currentlyConnectedNode = value;
				ActiveNodeChanged?.Invoke(value);
			}
		}
	}

	public event Action<States>? StateChanged;
	public event Action<int?>? ActiveNodeChanged;
	#endregion


	public UserInfo UserInfo { get; private set; }

	public TunnelManager tunnelManager;
	public PublicNodeInfo[] Nodes { get; private set; }

	public UserInterfaceManager()
	{
		this.State = 0;
		this.tunnelManager = new();

		Console.WriteLine("Trying to load user from local storage...");

		ApiHelperTransient.AuthorizationFailed += () => { this.State = States.Authentication; };
	}


	public async Task<bool> TryLoadUser()
	{
		try {
			var result = await ApiHelperTransient.Create();
			if(result is null) return false;

			this.UserInfo = ApiHelperTransient.UserInfo;
		} catch {
			return false;
		}

		try {
			_ = await this.RegisterDevice();
		} catch {
			return false;
		}

		this.State = States.Tunneling;
		return true;
	}



	private int retryCount = 0;
	private readonly int maxRetryCount = 1;
	public async Task<bool> ConnectToSelectedNode(int nodeId)
	{
		var prev = this.CurrentlyConnectedNode;
		_ = await this.EnsureDisconnected();

		if(nodeId == prev) return true;

		var client = await ApiHelperTransient.Create();
		if(client is null) return false;

		var response = await client.ConnectToNode(new() { NodeId = nodeId, WireguardPublicKey = this.tunnelManager.PublicKey });
		if(response is null) {
			if(this.retryCount++ < this.maxRetryCount) {
				await Task.Delay(3000); // wait windows to establish regular connection
				return await this.ConnectToSelectedNode(nodeId); // try once more
			}
			return false;
		}
		this.retryCount = 0;

		this.tunnelManager.WriteConfig(response);
		var result = await this.tunnelManager.EstablishTunnel();
		if(result) {
			this.CurrentlyConnectedNode = nodeId;
		}

		return result;
	}

	public async Task<bool> EnsureDisconnected()
	{
		var response = await this.tunnelManager.DeleteTunnel(); // just ensure
		this.CurrentlyConnectedNode = null;
		return response;
	}

	public async Task<bool> LoginAngRegister(string email, string password)
	{
		return (await this.Login(email, password)) && (await this.RegisterDevice());
	}

	public async Task<bool> LoadNodes()
	{
		var client = await ApiHelperTransient.Create();
		if(client is null) return false;

		var response = await client.GetNodesList();
		if(response is null) return false;

		this.Nodes = response
			.Where(x => x.IsActive == true)
			.Where(x => x.UserAccessLevelRequired <= (int)this.UserInfo.GetAccessLevel())
			.OrderByDescending(x => x.UserAccessLevelRequired)
			.ToArray();

		return true;
	}

	private async Task<bool> Login(string email, string password)
	{
		if(!DataValidator.ValidateEmail(email) || !DataValidator.ValidatePassword(password)) {
			throw new UserException("Password or email failed the validation.");
		}

		var response = await AuthHelper.Authenticate(new() { Email = email, Password = password });
		var status = AuthHelper.LastStatusCode;

		if((status >= 300 && status <= 399) || (status >= 500 && status <= 599)) {
			throw new WebException("Authentication server is not reachable");
		} else
		if(status == 401) {
			throw new WebException("Wrong email or password.");
		} else
		if(status == 404) {
			throw new WebException("User not found. Please visit the website and sign up.");
		} else
		if(status >= 400 && status <= 499) {
			throw new WebException("Your request was reject by the server.");
		} else
		if((!(status >= 200 && status <= 299)) || response == null) {
			throw new WebException("Unknown error occurred during the request.");
		} else
		if(response.RefreshToken is null || response.AccessToken is null) {
			throw new WebException("Server did not send refresh token which was expected.");
		}

		this.UserInfo = response.UserInfo;

		Console.WriteLine("Authenticated successfully.");
		this.State = States.Registering;

		return true;
	}

	private static int safeCounter = 0;
	private async Task<bool> RegisterDevice()
	{
		var client = await ApiHelperTransient.Create();
		if(client is null) return false;
		_ = await client.RegisterDevice(new() { WireguardPublicKey = this.tunnelManager.PublicKey });
		var status = client.LastStatusCode;

		if(status == 409) {
			this.State = States.Authentication;
			throw new WebException("Devices limit reached. Please visit you personal area on the website to delete one of the the devices.");
		} else
		if(status == 303) { // key duplicates
			if(safeCounter++ < 3) { // there is an unbelievable low chance this to happen 5 times, like (N/2^256)^safeCounter.
				this.tunnelManager = new();
				return await this.RegisterDevice();
			} else {
				throw new WebException("The generated key was rejected by the server. Please restart the program to create a new one.");
			}
		}
		if(!(status >= 200 && status <= 299) && status != 302) {
			throw new WebException("Unknown error occurred during the request.");
		}

		this.State = States.Tunneling;

		return true;
	}
	private async Task<bool> UnregisterDevice()
	{
		var client = await ApiHelperTransient.Create();
		if(client is null) return false;

		var response = await client.UnregisterDevice(new() { WireguardPublicKey = this.tunnelManager.PublicKey });

		this.State = States.Ready;

		return response;
	}
	private async Task<bool> TerminateSelf()
	{
		var client = await ApiHelperTransient.Create();
		if(client is null) return false;

		var response = await client.ServerLogout();

		this.State = States.Authentication;

		return response;
	}

	public async Task<bool> LogOut()
	{
		try {
			_ = await this.EnsureDisconnected();
			_ = await this.UnregisterDevice();
			_ = await this.TerminateSelf();
			this.tunnelManager.DeleteAllFiles();
			LocalHelper.DeleteRefreshToken();
			this.State = States.Authentication;
			return true;
		} catch {
			return false;
		}
	}

}
