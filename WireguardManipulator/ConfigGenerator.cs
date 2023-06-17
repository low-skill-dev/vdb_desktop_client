using ServerQuerier.Models.Device;
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

	#region private
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
		var sb = new StringBuilder(256);
		_ = sb.AppendLine($"[Interface]");
		_ = sb.AppendLine($"PrivateKey = {PrivateKey}");
		_ = sb.AppendLine($"Address = {Address}");
		_ = sb.AppendLine($"DNS = {DNS}");
		_ = sb.AppendLine();
		_ = sb.AppendLine($"[Peer]");
		_ = sb.AppendLine($"PublicKey = {RemoteKey}");
		_ = sb.AppendLine($"AllowedIPs = {AllowedIPs}");
		_ = sb.AppendLine($"Endpoint = {RemoteAddress}:{RemotePort}");

		return sb.ToString();
	}
	#endregion
}
