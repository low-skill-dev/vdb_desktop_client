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

//public static class JwtService
//{
//	public static IEnumerable<Claim>? ValidateJwtToken(string token, out DateTime ValidTo)
//	{
//		var readed = new JwtSecurityTokenHandler().ReadJwtToken(token);
//		ValidTo = readed.ValidTo;

//		// consider invalidate if less than 5 seconds lifetime left
//		if(readed.ValidTo > DateTime.UtcNow.AddSeconds(+5)) {
//			return readed.Claims;
//		} else {
//			return null;
//		}
//	}
//}