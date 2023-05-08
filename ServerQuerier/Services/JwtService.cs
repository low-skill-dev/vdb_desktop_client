using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ServerQuerier.Services;

public static class JwtService
{
	public static IEnumerable<Claim>? ValidateJwtToken(string token, out DateTime ValidTo)
	{
		var readed = new JwtSecurityTokenHandler().ReadJwtToken(token);
		ValidTo = readed.ValidTo;

		// consider invalidate if less than 10 seconds lifetime left
		if(readed.ValidTo > DateTime.UtcNow.AddSeconds(+10)) {
			return readed.Claims;
		} else {
			return null;
		}
	}
}