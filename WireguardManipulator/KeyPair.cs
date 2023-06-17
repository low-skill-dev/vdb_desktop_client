namespace WireguardManipulator;

public sealed class KeyPair
{
	public string Public { get; set; }
	public string Private { get; set; }

	public KeyPair() : this(CommandRunner.Run($"wg genkey")) { }
	public KeyPair(string privKey)
	{
		this.Private = privKey.Trim("\r\n\t,; ".ToCharArray());
		this.Public = CommandRunner.Run($"echo {this.Private} | wg pubkey").Trim("\r\n\t,; ".ToCharArray());
	}
}
