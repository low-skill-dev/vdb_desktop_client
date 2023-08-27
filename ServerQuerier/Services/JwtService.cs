using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiQuerier.Services;

// refactor 27-08-2023

public static class JwtService
{
	public static IEnumerable<Claim>? ValidateJwtToken(string token,
		out DateTime ValidTo,
		out DateTime NotBefore,
		out DateTime IssuedAt)
	{
		var read = new JwtSecurityTokenHandler().ReadJwtToken(token);

		ValidTo = read.ValidTo;
		NotBefore = read.ValidFrom;
		IssuedAt = read.IssuedAt;

		// consider invalidate if less than 5 seconds lifetime left
		if(read.ValidTo > DateTime.UtcNow.AddSeconds(+5)) {
			return read.Claims;
		} else {
			return null;
		}
	}
}