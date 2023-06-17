using ServerQuerier.Models.Auth;
using ServerQuerier.Services;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.Json;
using static ServerQuerier.Helpers.Constants;

namespace ServerQuerier.Helpers;


public static class AuthHelper
{
	private static readonly JsonSerializerOptions jsonOptions;
	private static readonly HttpClientHandler httpHandler;
	private static readonly HttpClient httpClient;
	public static bool AutoWriteRefresh { get; set; } = true;
	public static int LastStatusCode { get; private set; } = -1;
	static AuthHelper()
	{
		httpHandler = new() {
			SslProtocols = Environment.OSVersion.Version.Major > 10
				? SslProtocols.Tls13
				: SslProtocols.Tls12,
		};

		httpClient = new(httpHandler);
		httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

		jsonOptions = new(JsonSerializerDefaults.Web);
	}

	#region private methods

	private static UserInfo UserInfoFromJwt(IEnumerable<Claim> decoded)
	{
		return new UserInfo() {
			Id = int.Parse(decoded.First(x => x.Type.Equals(nameof(UserInfo.Id))).Value),
			Email = decoded.First(x => x.Type.Equals(nameof(UserInfo.Email))).Value
				?? throw new ArgumentNullException(nameof(UserInfo.Email)),
			IsAdmin = bool.Parse(decoded.First(x => x.Type.Equals(nameof(UserInfo.IsAdmin))).Value),
			IsEmailConfirmed = bool.Parse(decoded.First(x => x.Type.Equals(nameof(UserInfo.IsEmailConfirmed))).Value),
			PayedUntilUtc = DateTime.Parse(decoded.First(x => x.Type.Equals(nameof(UserInfo.PayedUntilUtc))).Value,
							CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind)
		};
	}

	private static async Task<AuthResult> DecodeJwtResponse(JwtResponse response)
	{
		if(response.RefreshToken is null) // server must provide refresh
			throw new ArgumentException(nameof(response.RefreshToken));

		var accessToken = response.AccessToken;
		var refreshToken = response.RefreshToken;

		var decoded = JwtService.ValidateJwtToken(accessToken,
			out var validTo, out var notBefore, out var issuedAt)!;

		var jwtInfo = new JwtInfo(notBefore, validTo, issuedAt);
		var userInfo = UserInfoFromJwt(decoded);

		if(AutoWriteRefresh) await LocalHelper.WriteRefreshToken(refreshToken);

		return new(jwtInfo, userInfo, accessToken, refreshToken);
	}

	#endregion

	public static async Task<AuthResult?> Authenticate(LoginRequest request)
	{
		try {
			Console.WriteLine("Begin authentication...");

			var response = await httpClient.PostAsync(
				HostPathTls + ApiBasePath + AuthPath + QueryStartString + RefreshJwtInBodyQuery,
				JsonContent.Create(request, options: jsonOptions));

			LastStatusCode = (int)response.StatusCode;

			Console.WriteLine($"Auth endpoint responded with {response.StatusCode} status code.");

			if(!response.IsSuccessStatusCode) return null;

			var jwtResponse = (await response.Content.ReadFromJsonAsync<JwtResponse>(jsonOptions))!;

			var result = await DecodeJwtResponse(jwtResponse);
			_ = await ApiHelperTransient.Create(result);
			return result;
		} catch {
			return null;
		}
	}

	private static async Task<AuthResult?> Refresh(RefreshJwtRequest request) // made it private only becouse was not used outside
	{
		try {
			Console.WriteLine("Begin refresh...");

			var response = await httpClient.PatchAsync(
				HostPathTls + ApiBasePath + AuthPath + QueryStartString + RefreshJwtInBodyQuery,
				JsonContent.Create(request, options: jsonOptions));

			LastStatusCode = (int)response.StatusCode;

			Console.WriteLine($"Refresh endpoint responded with {response.StatusCode} status code.");

			if(!response.IsSuccessStatusCode) return null;

			var jwtResponse = (await response.Content.ReadFromJsonAsync<JwtResponse>(jsonOptions))!;

			return await DecodeJwtResponse(jwtResponse);
		} catch {
			return null;
		}
	}

	public static async Task<AuthResult?> RefreshUsingLocalToken()
	{
		var token = await LocalHelper.ReadRefreshToken();
		if(token is null) return null;

		var result = await Refresh(new() { RefreshToken = token });
		if(result is null) LocalHelper.DeleteRefreshToken();

		return result;
	}
}
