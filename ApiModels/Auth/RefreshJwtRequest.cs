namespace ApiModels.Auth;

public class RefreshJwtRequest
{
	public required string RefreshToken { get; init; }
}