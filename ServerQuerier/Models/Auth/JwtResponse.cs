namespace ServerQuerier.Models.Auth;

public class JwtResponse
{
	public string AccessToken { get; set; }
	public string? RefreshToken { get; set; }
	public int? RefreshExpires { get; set; }


	public JwtResponse(string accessToken, string? refreshToken = null)
	{
		this.AccessToken = accessToken;
		this.RefreshToken = refreshToken;
	}
}
