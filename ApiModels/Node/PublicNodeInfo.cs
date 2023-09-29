
namespace ApiModels.Node;

#pragma warning disable CS8618 

public class PublicNodeInfo
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string IpAddress { get; set; }
	public int WireguardPort { get; set; }
	public int UserAccessLevelRequired { get; set; }
	public bool IsActive { get; set; }
	public int ClientsConnected { get; set; }
}