namespace ServerQuerier.Models.Auth;

public class JwtInfo
{
	public DateTime Nbf { get; init; }
	public DateTime Exp { get; init; }
	public DateTime Iat { get; init; }

	public JwtInfo(DateTime nbf, DateTime exp, DateTime iat)
	{
		this.Nbf = nbf;
		this.Exp = exp;
		this.Iat = iat;
	}
}
