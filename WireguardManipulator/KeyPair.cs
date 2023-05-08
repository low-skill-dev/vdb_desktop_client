using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireguardManipulator;

public sealed class KeyPair
{
	public string Public { get; set; }
	public string Private { get; set; }

	public KeyPair() : this(CommandRunner.Run($"wg genkey")) { }
	public KeyPair(string privKey)
	{
		Private = privKey.Trim("\r\n\t,; ".ToCharArray());
		Public = CommandRunner.Run($"echo {Private} | wg pubkey").Trim("\r\n\t,; ".ToCharArray());
	}
}
