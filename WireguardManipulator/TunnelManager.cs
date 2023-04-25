using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireguardManipulator;
public class TunnelManager
{
	public string ConfigReadingPath { get; set; }
		= Path.Join(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Vdb",
			"vdb0.conf"
		);

	public TunnelManager()
	{

	}

	public async Task<bool> EstablishTunnel()
	{
		return string.IsNullOrWhiteSpace(await CommandRunner.RunAsync(
					$"wireguard /installtunnelservice {ConfigReadingPath}"));
	}

	public async Task<bool> DeleteTunnel()
	{
		return string.IsNullOrWhiteSpace(await CommandRunner.RunAsync(
			$"wireguard /uninstalltunnelservice {Path.GetFileNameWithoutExtension(ConfigReadingPath)}"));
	}
}
