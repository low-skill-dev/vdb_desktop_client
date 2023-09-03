using ApiModels.Auth;
using ApiQuerier.Models;
using ApiQuerier.Services;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Json;
using static ApiQuerier.Helpers.Constants;
using FilesHelper;
using static ApiQuerier.Helpers.WebCommon;

namespace ApiQuerier.Helpers;

// refactor 27-08-2023

public static class AuthHelper
{
	private static int _lastStatusCode;
	public static int LastStatusCode
	{
		get
		{
			return _lastStatusCode;
		}
		private set
		{
			if(value < 200 || value > 299)		
				Trace.WriteLine($"Request failed: HTTP_{value}.");
			else
				Trace.WriteLine($"Request succeeded: HTTP_{value}.");

			_lastStatusCode = value;
		}
	}

	#region private methods

	private static UserInfo UserInfoFromJwt(IEnumerable<Claim> decoded)
	{
		return new UserInfo()
		{
			Id = int.Parse(decoded.First(x => x.Type.Equals(nameof(UserInfo.Id))).Value),
			Email = decoded.First(x => x.Type.Equals(nameof(UserInfo.Email))).Value
				?? throw new ArgumentNullException(nameof(UserInfo.Email)),
			IsAdmin = bool.Parse(decoded.First(x => x.Type.Equals(nameof(UserInfo.IsAdmin))).Value),
			IsEmailConfirmed = bool.Parse(decoded.First(x => x.Type.Equals(nameof(UserInfo.IsEmailConfirmed))).Value),
			PayedUntilUtc = DateTime.Parse(decoded.First(x => x.Type.Equals(nameof(UserInfo.PayedUntilUtc))).Value,
							CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind)
		};
	}

	private static AuthResult DecodeJwtResponse(JwtResponse response)
	{
		if(string.IsNullOrWhiteSpace(response.RefreshToken)) // server must provide refresh
			throw new ArgumentException(nameof(response.RefreshToken));

		var accessToken = response.AccessToken;
		var refreshToken = response.RefreshToken;

		var decoded = JwtService.ValidateJwtToken(accessToken,
			out var validTo, out var notBefore, out var issuedAt)!;

		var jwtInfo = new JwtInfo
		{
			Nbf = notBefore,
			Exp = validTo,
			Iat = issuedAt,
		};

		var userInfo = UserInfoFromJwt(decoded);

		// we always write latest token into the file
		TokenFilesHelper.WriteRefreshToken(refreshToken);

		return new()
		{
			JwtInfo = jwtInfo,
			UserInfo = userInfo,
			AccessToken = accessToken,
			RefreshToken = refreshToken,
		};
	}

	private static async Task<AuthResult?> Refresh(RefreshJwtRequest request)
	{
		Trace.WriteLine($"{nameof(Refresh)} started.");

		try
		{
			var response = await httpClient.PatchAsync(
				HostPathTls + ApiBasePath + AuthPath + QueryStartString + RefreshJwtInBodyQuery,
				JsonContent.Create(request, options: jsonOptions));

			LastStatusCode = (int)response.StatusCode;

			if(!response.IsSuccessStatusCode)
			{
				Trace.WriteLine($"{nameof(Refresh)} failed.");
				return null;
			}

			var jwtResponse = (await response.Content.ReadFromJsonAsync<JwtResponse>(jsonOptions))!;

			return DecodeJwtResponse(jwtResponse);
		}
		catch(Exception e)
		{
			Trace.WriteLine($"{nameof(Refresh)} failed: {e.Message}.");
			return null;
		}
	}

	#endregion

	public static async Task Authenticate(string login, string password)
	{
		await Authenticate(new LoginRequest { Email = login, Password = password });
	}

	// This method is used by external projects, e.g. UI to send credentials to the server
	internal static async Task<AuthResult?> Authenticate(LoginRequest request)
	{
		Trace.WriteLine($"{nameof(Authenticate)} started.");

		try
		{
			var response = await httpClient.PostAsync(
				HostPathTls + ApiBasePath + AuthPath + QueryStartString 
				+ RefreshJwtInBodyQuery + QueryAndString + RedirectToLoginQuery,
				JsonContent.Create(request, options: jsonOptions));

			LastStatusCode = (int)response.StatusCode;

			if(!response.IsSuccessStatusCode)
			{
				Trace.WriteLine($"{nameof(Authenticate)} failed.");
				return null;
			}

			var jwtResponse = (await response.Content.ReadFromJsonAsync<JwtResponse>(jsonOptions))!;

			var result = DecodeJwtResponse(jwtResponse);
			await ApiHelperTransient.Create(result); // go to implementation for details of this call
			return result;
		}
		catch(Exception e)
		{
			Trace.WriteLine($"{nameof(Authenticate)} failed: {e.Message}.");
			return null;
		}
	}

	internal static async Task<AuthResult?> RefreshUsingLocalToken()
	{
		Trace.WriteLine($"{nameof(RefreshUsingLocalToken)} started.");

		var token = TokenFilesHelper.ReadRefreshToken();
		if(string.IsNullOrWhiteSpace(token))
		{
			Trace.WriteLine($"{nameof(RefreshUsingLocalToken)} failed: local token not found.");
			return null;
		}

		var result = await Refresh(new() { RefreshToken = token });

		var code = LastStatusCode;
		if(result is null)
		{
			// if refresh failed client-side, delete obsolete token
			if((code >= 400 || code <= 499) && code != 429)
				TokenFilesHelper.DeleteRefreshToken();

			Trace.WriteLine($"{nameof(RefreshUsingLocalToken)} failed.");
		}

		return result;
	}
}
