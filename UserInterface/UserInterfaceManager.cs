using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using main_server_api.Models.UserApi.Website.Common;
using ServerQuerier;
using ServerQuerier.Services;
using ServicesLayer.Models.Services;
using UserInterface.Models;
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
	public States State { get; private set; }
	 
	public event Action<States>? StateChanged;
	public event Action<int?>? ActiveNodeChanged;
	public bool IsGuestMode { get; set; }
	public int? CurrentlyConnectedNode { get; set; }
	#endregion

	public UserInfo UserInfo { get; private set; }


	public readonly VdbClient serverClient;
	public readonly TunnelManager tunnelManager;
	public PublicNodeInfo[] Nodes { get; private set; }
	//public readonly JwtSecurityTokenHandler tokenHandler;
	public UserInterfaceManager()
	{
		this.State = 0;
		this.serverClient = new();
		this.tunnelManager = new();
		//this.tokenHandler = new();

		Console.WriteLine("Trying to load user from local storage...");
	}

	public async Task<bool> TryLoadUser()
	{
		var result =  await serverClient.TryLoadUser();
		if(result) {
			try {
				await RegisterDevice();
				this.State = States.Tunneling;
				StateChanged?.Invoke(this.State);
				return true;
			} catch { }
		}

		return false;
	}

	public async Task ConnectToSelectedNode(int nodeId)
	{
		await Disconnect();
		await Task.Delay(300); // windows to establish regular connection

		var response = await serverClient.ConnectToNode(new() { NodeId = nodeId, WireguardPublicKey = this.tunnelManager.PublicKey });
		if(response is null) return;
		tunnelManager.WriteConfig(response);
		var result = await tunnelManager.EstablishTunnel();
		if(result) this.CurrentlyConnectedNode = nodeId;
		ActiveNodeChanged?.Invoke(nodeId);
	}

	public async Task Disconnect()
	{
		await tunnelManager.DeleteTunnel(); // just ensure
		this.CurrentlyConnectedNode = null;
		ActiveNodeChanged?.Invoke(null);
	}

	public async Task LoginAngRegister(string email, string password)
	{
		await Login(email, password);
		await RegisterDevice();
	}

	public async Task LoadNodes()
	{
		this.Nodes = (await serverClient.GetNodesList())!.Where(x => x.IsActive == true).ToArray();
	}


	private async Task Login(string email, string password)
	{
		if(!DataValidator.ValidateEmail(email) || !DataValidator.ValidatePassword(password)) {
			throw new UserException("Password or email failed the validation.");
		}

		var response = await serverClient.Authenticate(new() { Email = email, Password = password });
		var status = serverClient.LastStatusCode;

		if((status >= 300 && status <= 399) || (status >= 500 && status <= 599)) {
			throw new WebException("Authentication server is not reachable");
		} else
		if(status == 401) {
			throw new WebException("Wrong email or password.");
		} else
		if(status >= 400 && status <= 499) {
			throw new WebException("Your request was reject by the server.");
		} else
		if((!(status >= 200 && status <= 299)) || response == false) {
			throw new WebException("Unknown error occurred during the request.");
		} else
		if(serverClient.RefreshToken is null || serverClient.AccessToken is null) {
			throw new WebException("Server did not send refresh token which was expected.");
		}

		var access = JwtService.ValidateJwtToken(serverClient.AccessToken, out _);
		try {
			this.UserInfo = new() {
				Id = int.Parse(access.First(x => x.Type.Equals(nameof(UserInfo.Id))).Value),
				Email = access.First(x => x.Type.Equals(nameof(UserInfo.Email))).Value
					?? throw new ArgumentNullException(nameof(UserInfo.Email)),
				IsAdmin = bool.Parse(access.First(x => x.Type.Equals(nameof(UserInfo.IsAdmin))).Value),
				IsEmailConfirmed = bool.Parse(access.First(x => x.Type.Equals(nameof(UserInfo.IsEmailConfirmed))).Value),
				PayedUntilUtc = DateTime.Parse(access.First(x => x.Type.Equals(nameof(UserInfo.PayedUntilUtc))).Value,
					CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind)
			};

			Console.WriteLine("Authenticated successfully.");
			this.State = States.Registering;
		} catch {
			throw new WebException("Server response was in unexpected format.");
		}
	}
	private async Task RegisterDevice()
	{
		var response = await serverClient.RegisterDevice(new() { WireguardPublicKey = tunnelManager.PublicKey });
		var status = serverClient.LastStatusCode;

		if(status == 409) {
			throw new WebException("Devices limit reached. Please visit you personal area on the website to delete one of the the devices.");
		} else
		if((!(status >= 200 && status <= 299)) || response == false) {
			throw new WebException("Unknown error occurred during the request.");
		}

		this.State = States.Ready;
	}
}
