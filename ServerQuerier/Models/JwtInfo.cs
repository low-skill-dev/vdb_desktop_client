namespace ApiQuerier.Models;

// refactor 27-08-2023

internal class JwtInfo
{
	public required DateTime Nbf { get; init; }
	public required DateTime Exp { get; init; }
	public required DateTime Iat { get; init; }
}
