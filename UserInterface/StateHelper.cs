using ApiModels.Device;
using ApiModels.Node;
using ApiQuerier.Helpers;
using ApiQuerier.Models;
//using ApiQuerier.Models.Auth;
//using ApiQuerier.Models.Nodes;
using System.Diagnostics;
using UserInterface.Services;
using WireguardManipulator;

namespace UserInterface;

internal sealed class StateHelper
{
	private int? _currentlyConnectedNode;
	public int? CurrentlyConnectedNode
	{
		get
		{
			return this._currentlyConnectedNode;
		}
		set
		{
			if(this._currentlyConnectedNode != value)
			{
				this._currentlyConnectedNode = value;
				ActiveNodeChanged?.Invoke(value);
			}
		}
	}

	public event Action<int?>? ActiveNodeChanged;


	public UserInfo UserInfo { get; private set; }
	public PublicNodeInfo[] Nodes { get; private set; }

	private TunnelManager tunnelManager;
	public StateHelper()
	{
		Trace.WriteLine($"{nameof(StateHelper)} ctor started.");

		this.tunnelManager = new();

		Trace.WriteLine($"{nameof(StateHelper)} ctor completed.");
	}

	private const int maxRetryCount = 3;
	public async Task<bool> ConnectToSelectedNode(int nodeId)
	{
		Trace.WriteLine($"{nameof(ConnectToSelectedNode)}({nodeId}) started.");

		await this.EnsureDisconnected();

		var client = await ApiHelperTransient.Create();
		if(client is null)
		{
			Trace.WriteLine($"{nameof(ConnectToSelectedNode)}({nodeId}) failed: unable to create {nameof(ApiHelperTransient)}.");
			return false;
		}

		int tries = 0;
		ConnectDeviceResponse? response = null;

		while(tries++ < maxRetryCount && response is null)
		{
			response = await client.ConnectToNode(new() { NodeId = nodeId, WireguardPublicKey = this.tunnelManager.PublicKey });
			if(response is null) await Task.Delay(3000);
		}
		if(response is null)
		{
			Trace.WriteLine($"{nameof(ConnectToSelectedNode)}({nodeId}) failed: server unreachable.");
			return false;
		}

		this.tunnelManager.WriteConfig(response);
		var result = await this.tunnelManager.EstablishTunnel();

		if(result)
		{
			Trace.WriteLine($"{nameof(ConnectToSelectedNode)}({nodeId}) completed..");
			this.CurrentlyConnectedNode = nodeId;
			return result;
		}

		Trace.WriteLine($"{nameof(ConnectToSelectedNode)}({nodeId}) failed: unable to establish tunnel.");
		return result;
	}

	public async Task<bool> EnsureDisconnected()
	{
		var response = await this.tunnelManager.DeleteTunnel(); // just ensure
		this.CurrentlyConnectedNode = null;
		return response;
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
			.ThenBy(x=> x.Id)
			.ToArray();

		return true;
	}

	//private static int safeCounter = 0;
	//private async Task<bool> RegisterDevice()
	//{
	//	var client = await ApiHelperTransient.Create();
	//	if(client is null) return false;
	//	_ = await client.RegisterDevice(new() { WireguardPublicKey = this.tunnelManager.PublicKey });
	//	var status = client.LastStatusCode;

	//	if(status == 409)
	//	{
	//		//this.State = States.Authentication;
	//		throw new WebException("Devices limit reached. Please visit you personal area on the website to delete one of the the devices.");
	//	}
	//	else
	//	if(status == 303)
	//	{ // key duplicates
	//		if(safeCounter++ < 3)
	//		{ // there is an unbelievable low chance this to happen 5 times, like (N/2^256)^safeCounter.
	//			this.tunnelManager = new();
	//			return await this.RegisterDevice();
	//		}
	//		else
	//		{
	//			throw new WebException("The generated key was rejected by the server. Please restart the program to create a new one.");
	//		}
	//	}
	//	if(!(status >= 200 && status <= 299) && status != 302)
	//	{
	//		throw new WebException("Unknown error occurred during the request.");
	//	}

	//	//this.State = States.Tunneling;

	//	return true;
	//}
	//private async Task<bool> UnregisterDevice()
	//{
	//	var client = await ApiHelperTransient.Create();
	//	if(client is null) return false;

	//	var response = await client.UnregisterDevice(new() { WireguardPublicKey = this.tunnelManager.PublicKey });

	//	//this.State = States.Ready;

	//	return response;
	//}
	//private async Task<bool> TerminateSelf()
	//{
	//	var client = await ApiHelperTransient.Create();
	//	if(client is null) return false;

	//	var response = await client.ServerLogout();

	//	//this.State = States.Authentication;

	//	return response;
	//}

	//public async Task<bool> LogOut()
	//{
	//	try
	//	{
	//		_ = await this.EnsureDisconnected();
	//		_ = await this.UnregisterDevice();
	//		_ = await this.TerminateSelf();
	//		this.tunnelManager.DeleteAllFiles();
	//		//LocalHelper.DeleteRefreshToken();
	//		//this.State = States.Authentication;
	//		return true;
	//	}
	//	catch
	//	{
	//		return false;
	//	}
	//}
}
