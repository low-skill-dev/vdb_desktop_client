namespace ApiModels.Device;

#pragma warning disable CS8618

public class ConnectDeviceResponse
{
	public string AllowedIps { get; set; }
	public string InterfacePublicKey { get; set; }
	public string AddedPeerPublicKey { get; init; }
	public string ServerIpAddress { get; init; }
	public int WireguardPort { get; init; }
}