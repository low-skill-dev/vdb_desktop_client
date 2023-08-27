using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ApiQuerier.Models;

internal class AddDeviceRequest
{
	public required string WireguardPublicKey { get; init; }
}

