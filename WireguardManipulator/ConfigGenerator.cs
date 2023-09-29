using ApiModels.Device;
using System.Text;

namespace WireguardManipulator;

internal static class ConfigGenerator
{
	public static string GenerateConfig(string privKey, ConnectDeviceResponse mainServerResponse)
	{
		return GenerateConfig(
			privKey,
			mainServerResponse.ServerIpAddress,
			mainServerResponse.WireguardPort.ToString(),
			mainServerResponse.InterfacePublicKey,
			mainServerResponse.AllowedIps
		);
	}

	private static string GenerateConfig(
		string PrivateKey,
		string RemoteAddress,
		string RemotePort,
		string RemoteKey,
		string Address,
		string DNS = @"8.8.8.8",
		string AllowedIPs = @"0.0.0.0/0"
	)
	{
		return new StringBuilder(256)
			.AppendLine($"[Interface]")
			.AppendLine($"PrivateKey = {PrivateKey}")
			.AppendLine($"Address = {Address}")
			.AppendLine($"DNS = {DNS}")
			.AppendLine()
			.AppendLine($"[Peer]")
			.AppendLine($"PublicKey = {RemoteKey}")
			.AppendLine($"AllowedIPs = {AllowedIPs}")
			.AppendLine($"Endpoint = {RemoteAddress}:{RemotePort}")
			.ToString();
	}
}
