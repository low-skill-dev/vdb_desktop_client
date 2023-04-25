using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using main_server_api.Models.UserApi.Website.Common;
using ServerQuerier;
using UserInterface.Models;
using UserInterface.Services;

namespace UserInterface;
internal class WebException : UserException
{
	public WebException(string message) : base(message) { }
}
internal class UserInterfaceManager
{
	#region enums
	public enum States
	{
		Authentication,
		Tunneling,
		Settings
	}
	public enum AuthenticationState
	{
		None, // still on authentication window
		Disabled, // user choose anonymous usage strategy 
		Passed // user authorized successfully
	}
	#endregion

	#region stateful-providing props
	public States State { get; protected set; }
	public AuthenticationState AuthState { get; protected set; }
	#endregion

	public JwtInfo JwtInfo { get; protected set; }
	public UserInfo UserInfo { get; protected set; }


	private readonly VdbClient serverClient;
	private readonly JwtSecurityTokenHandler tokenHandler;
	public UserInterfaceManager()
	{
		this.State = 0;
		this.AuthState = 0;
		this.serverClient = new();
		this.tokenHandler = new();

	}


	public async Task Login(string email, string password)
	{
		if(!DataValidator.ValidateEmail(email) || !DataValidator.ValidatePassword(password))
			return;

		var response = await serverClient.Authenticate(new() { Email = email, Password = password });
		var status = serverClient.LastStatusCode;

		if((status >= 300 && status <= 399) || (status >= 500 && status <= 599)) {
			throw new WebException("Authentication server is not reachable");
		} else
		if(status == 401) {
			throw new WebException("Wrong email or password.");
		} else
		if(status >= 400 && status <= 499) {
			throw new WebException("Your request was reject by the server.");
		}else
		if((!(status >= 200 && status <=299)) || response is null) {
			throw new WebException("Unknown error occurred during the request.");
		} else
		if(response.RefreshToken is null) {
			throw new WebException("Server did not send refresh token which was expected.");
		}

		var access = tokenHandler.ReadJwtToken(response.AccessToken);
		var refresh = tokenHandler.ReadJwtToken(response.RefreshToken);

		this.JwtInfo = new JwtInfo {
			AccessJwt = response.AccessToken,
			RefreshJwt = response.RefreshToken,
			AccessValidUntil = access.ValidTo,
			RefreshValidUntil = refresh.ValidTo
		};

		try {
			this.UserInfo = new() {
				Id = int.Parse(access.Claims.First(x => x.Type.Equals(nameof(UserInfo.Id))).Value),
				Email = access.Claims.First(x => x.Type.Equals(nameof(UserInfo.Email))).Value
					?? throw new ArgumentNullException(nameof(UserInfo.Email)),
				IsAdmin = bool.Parse(access.Claims.First(x=> x.Type.Equals(nameof(UserInfo.IsAdmin))).Value),
				IsEmailConfirmed = bool.Parse(access.Claims.First(x=> x.Type.Equals(nameof(UserInfo.IsEmailConfirmed))).Value),
				PayedUntilUtc = DateTime.Parse(access.Claims.First(x => x.Type.Equals(nameof(UserInfo.PayedUntilUtc))).Value,
					CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind)
			};
		} catch {
			throw new WebException("Server response was in unexpected format.");
		}
	}
}
