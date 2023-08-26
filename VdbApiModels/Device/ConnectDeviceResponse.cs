namespace ApiModels.Device;

#pragma warning disable CS8618

public abstract class AddPeerResponse
{
	public string AllowedIps { get; init; }
	public string InterfacePublicKey { get; init; }
}

public abstract class ConnectDeviceResponse: AddPeerResponse
{
	public string AddedPeerPublicKey { get; init; }
	public string ServerIpAddress { get; init; }
	public int WireguardPort { get; init; }
}