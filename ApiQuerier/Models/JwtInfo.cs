namespace ApiQuerier.Models;

public class JwtInfo
{
	public /*required*/ DateTime Nbf { get; init; }
	public /*required*/ DateTime Exp { get; init; }
	public /*required*/ DateTime Iat { get; init; }
}
