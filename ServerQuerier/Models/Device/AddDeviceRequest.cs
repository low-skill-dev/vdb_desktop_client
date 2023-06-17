using System.ComponentModel.DataAnnotations;

namespace ServerQuerier.Models.Device;

public class AddDeviceRequest
{
	private const int LengthOfBase64For256Bits = 256 / 8 * 4 / 3 + 3;

	[Required]
	[MaxLength(LengthOfBase64For256Bits)]
	public string WireguardPublicKey { get; set; }
}
