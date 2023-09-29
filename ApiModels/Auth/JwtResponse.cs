namespace ApiModels.Auth;

#pragma warning disable CS8618

public class JwtResponse
{
	public string AccessToken { get; set; }
	public string? RefreshToken { get; set; }
}