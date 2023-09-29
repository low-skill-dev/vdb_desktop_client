namespace WireguardManipulator;

internal sealed class KeyPair
{
	public string Public { get; set; }
	public string Private { get; set; }

	private KeyPair(string privKey, string pubKey)
	{
		this.Private = privKey.Trim("\r\n\t,; ".ToCharArray());
		this.Public = pubKey.Trim("\r\n\t,; ".ToCharArray());
	}

	public KeyPair(string privKey) : this(privKey, CommandRunner.Run($"echo {privKey} | wg pubkey")) { }

	public KeyPair() : this(CommandRunner.Run($"wg genkey")) { }
}
