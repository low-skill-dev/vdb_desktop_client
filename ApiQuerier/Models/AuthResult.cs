namespace ApiQuerier.Models;

// refactor 27-08-2023

public class AuthResult
{
	public required JwtInfo JwtInfo { get; init; }
	public required UserInfo UserInfo { get; init; }
	public required string AccessToken { get; init; }
	public required string RefreshToken { get; init; }
}
