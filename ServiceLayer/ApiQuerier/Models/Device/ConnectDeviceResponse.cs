using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ApiQuerier.Models;

internal abstract class AddPeerResponse
{
	public string AllowedIps { get; set; } = null!;
	public string InterfacePublicKey { get; set; } = null!;
}

internal abstract class ConnectDeviceResponse: AddPeerResponse
{
	public string AddedPeerPublicKey { get; set; } = null!;
	public string ServerIpAddress { get; set; } = null!;
	public int WireguardPort { get; set; }
}
