using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using main_server_api.Models.UserApi.Application.Device;

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
		sb.AppendLine($"[Interface]");
		sb.AppendLine($"PrivateKey = {PrivateKey}");
		sb.AppendLine($"Address = {Address}");
		sb.AppendLine($"DNS = {DNS}");
		sb.AppendLine();
		sb.AppendLine($"[Peer]");
		sb.AppendLine($"PublicKey = {RemoteKey}");
		sb.AppendLine($"AllowedIPs = {AllowedIPs}");
		sb.AppendLine($"Endpoint = {RemoteAddress}:{RemotePort}");

		return sb.ToString();
	}
	#endregion
}
