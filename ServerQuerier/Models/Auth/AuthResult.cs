namespace ServerQuerier.Models.Auth;
public class AuthResult
{
	public JwtInfo JwtInfo { get; init; }
	public UserInfo UserInfo { get; init; }
	public string AccessToken { get; init; }
	public string RefreshToken { get; init; }

	public AuthResult(JwtInfo jwtInfo, UserInfo userInfo, string accessToken, string refreshToken)
	{
		this.JwtInfo = jwtInfo;
		this.UserInfo = userInfo;
		this.AccessToken = accessToken;
		this.RefreshToken = refreshToken;
	}
}
