namespace ApiModels.Auth;

#pragma warning disable CS8618

public abstract class JwtResponse
{
	public string AccessToken { get; init; }
	public string? RefreshToken { get; init; }
}