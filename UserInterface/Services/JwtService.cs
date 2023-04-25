//using main_server_api.Models.UserApi.Website.Common;
//using System.Configuration;
//using System.Reflection.Metadata;
//using System.Security.Claims;
//using System.Windows.Controls;
//using UserInterface;
//using vdb_main_server_api.Services;


//using main_server_api.Models.UserApi.Website.Common;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Runtime;
//using System.Security.Claims;
//using System.Text;

//namespace UserInterface.Services;

//public sealed class JwtService
//{
//	private JwtSecurityTokenHandler _tokenHandler;

//	public JwtService()
//	{
//		this._tokenHandler = new JwtSecurityTokenHandler();
//		_tokenHandler.ReadJwtToken("").
//	}

//	#region app-specific
//	public string GenerateAccessJwtToken(UserInfo user)
//	{
//		return GenerateJwtToken(new Claim[]
//		{
//			new Claim(nameof(user.Id),user.Id.ToString()),
//			new Claim(nameof(user.IsAdmin), user.IsAdmin.ToString()),
//			new Claim(nameof(user.Email), user.Email),
//			new Claim(nameof(user.IsEmailConfirmed),user.IsEmailConfirmed.ToString()),
//			new Claim(nameof(user.PayedUntilUtc), user.PayedUntilUtc.ToString("o")) // 'o' format provider satisfies ISO 8601
//		});
//	}

//	public string GenerateRefreshJwtToken(RefreshToken token)
//	{
//		return GenerateJwtToken(new[] {
//			new Claim(nameof(token.IssuedToUser), token.IssuedToUser.ToString()),
//			new Claim(nameof(token.Entropy),token.Entropy.ToString()),
//			}, RefreshTokenLifespan);
//	}
//	#endregion
//}