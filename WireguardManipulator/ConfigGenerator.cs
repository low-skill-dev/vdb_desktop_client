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


public class ConfigGenerator
{
	private readonly KeyPair Keys;
	public string PublicKey => Keys.Public;

	public string ConfigWritingPath { get; set; }
		= Path.Join(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Vdb",
			"vdb0.conf"
		);


	public static async Task<ConfigGenerator> Create()
	{
		var keys = await KeyPair.Create();
		return new ConfigGenerator(keys);
	}

	private ConfigGenerator(KeyPair keys)
	{
		this.Keys = keys;
	}

	public FileInfo WriteConfigToFile(ConnectDeviceResponse mainServerResponse)
	{
		var file = new FileInfo(this.ConfigWritingPath);

		File.WriteAllText(file.FullName, 
			GenerateConfig(
				mainServerResponse.ServerIpAddress,
				mainServerResponse.WireguardPort.ToString(),
				mainServerResponse.InterfacePublicKey,
				mainServerResponse.AllowedIps
			));

		return file;
	}

	#region private
	private string GenerateConfig(
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
		sb.AppendLine($"PrivateKey = {this.Keys.Private}");
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
