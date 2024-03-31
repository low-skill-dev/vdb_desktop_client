namespace ApiModels.Device;

public class ConnectDeviceRequest
{
	public /*required*/ string WireguardPublicKey { get; init; }
	public /*required*/ int NodeId { get; init; }
}