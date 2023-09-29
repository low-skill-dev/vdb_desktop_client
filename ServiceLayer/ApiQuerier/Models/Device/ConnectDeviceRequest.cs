using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ApiQuerier.Models;
internal class ConnectDeviceRequest
{
	public required string WireguardPublicKey { get; init; }
	public required int NodeId { get; init; }
}
