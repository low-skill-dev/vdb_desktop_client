namespace ServerQuerier.Models.Device;

public class AddPeerResponse
{
	public string AllowedIps { get; set; }
	public string InterfacePublicKey { get; set; }

	public AddPeerResponse(string allowedIps, string interfacePublicKey)
	{
		this.AllowedIps = allowedIps;
		this.InterfacePublicKey = interfacePublicKey;
	}

	public AddPeerResponse()
	{

	}
}
