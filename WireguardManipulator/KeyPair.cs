using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireguardManipulator;

public sealed class KeyPair
{
	public string Public { get; init; }
	public string Private { get; init; }

	public static async Task<KeyPair> Create()
	{
		var priv = await CommandRunner.RunAsync($"wg genkey");
		var pub = await CommandRunner.RunAsync($"echo {priv} | wg pubkey");

		return new KeyPair { Private = priv, Public = pub };
	}

#pragma warning disable CS8618 // factory method implementation be like...
	private KeyPair() { }
#pragma warning restore CS8618
}
