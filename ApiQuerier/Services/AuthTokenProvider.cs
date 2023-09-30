using ApiModels.Auth;
using ApiQuerier.Models;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Authentication;
using static ApiQuerier.Helpers.Constants;
using FilesHelper;
using System.Globalization;
using System.Security.Claims;

namespace ApiQuerier.Services;

/// <summary>
/// A gateway class for all authentication operations.
/// The role is to provide access token with auto refresh
/// to other classes.
/// </summary>
public static class AuthTokenProvider
{
	private static void Log(string message)
		=> Trace.WriteLine($"{nameof(AuthTokenProvider)}: {message}.");

	private static void LogRequest(string path, int code)
	{
		LastStatusCode = code;

		string msg = string.Empty;

		msg += $"request to {path} ";

		msg += (200 <= code && code <= 299)
			? "succeeded" : "failed";

		msg += code > 0
			? " with exception" : $": HTTP_{code}";

		Log(msg);
	}

	#region private fields

	private static string? _accessToken;
	private static string? _refreshToken;

	private static TimeSpan? _accessTokenLifespan;
	private static TimeSpan? _refreshTokenLifespan;

	private static DateTime? _accessValidTo;
	private static DateTime? _refreshValidTo;

	private static double _minimalAccessTokenLifespanPercent;
	private static double _minimalRefreshTokenLifespanPercent;

	private static TimeSpan? _minimalAccessTokenLifespan;
	private static TimeSpan? _minimalRefreshTokenLifespan;

	private static readonly HttpClient _httpClient;
	private static readonly HttpClientHandler _httpHandler;
	private static readonly JsonSerializerOptions _jsonOpts;

	private static int _maxRetries;
	private static TimeSpan _retryDelay;

	#endregion

	#region props for private fields

	/// <summary> Value must be in range (0; 1]. </summary>
	public static double MinimalAccessTokenLifespanPercent
	{
		get
		{
			return _minimalAccessTokenLifespanPercent;
		}
		set
		{
			if(0.001 > value || value > 1)
				throw new ArgumentOutOfRangeException("Value must be in range (0; 1].");

			_minimalAccessTokenLifespanPercent = value;

			Log($"{nameof(MinimalAccessTokenLifespanPercent)} set to {MinimalAccessTokenLifespanPercent}");
		}
	}

	/// <summary> Value must be in range (0; 1]. </summary>
	public static double MinimalRefreshTokenLifespanPercent
	{
		get
		{
			return _minimalRefreshTokenLifespanPercent;
		}
		set
		{
			if(0.001 > value || value > 1)
				throw new ArgumentOutOfRangeException("Value must be in range (0; 1].");

			_minimalRefreshTokenLifespanPercent = value;

			Log($"{nameof(MinimalRefreshTokenLifespanPercent)} set to {MinimalRefreshTokenLifespanPercent}");
		}
	}

	public static TimeSpan? MinimalAccessTokenLifespan
	{
		get
		{
			return _minimalAccessTokenLifespan;
		}
		set
		{
			_minimalAccessTokenLifespan = value;

			Log($"{nameof(MinimalAccessTokenLifespan)} set to {MinimalAccessTokenLifespan}");
		}
	}

	public static TimeSpan? MinimalRefreshTokenLifespan
	{
		get
		{
			return _minimalRefreshTokenLifespan;
		}
		set
		{
			_minimalRefreshTokenLifespan = value;

			Log($"{nameof(MinimalRefreshTokenLifespan)} set to {MinimalRefreshTokenLifespan}");
		}
	}

	public static TimeSpan HttpClientTimeout
	{
		get
		{
			return _httpClient.Timeout;
		}
		set
		{
			if(1 > value.TotalMilliseconds || value.TotalMilliseconds > 120 * 1000)
				throw new ArgumentOutOfRangeException("Value must be in range (0; 120] seconds.");

			_httpClient.Timeout = value;

			Log($"{nameof(HttpClientTimeout)} set to {HttpClientTimeout.TotalSeconds.ToString("0.00")} seconds");
		}
	}

	#endregion

	#region public props

	public static int LastStatusCode { get; private set; }
	public static UserInfo CurrentUser { get; private set; }

	#endregion

	#region calculated props

	private static TimeSpan _minimalAccessTokenLifespanResolved
	{
		get
		{
			if(_accessTokenLifespan is null) throw new NullReferenceException();

			var byPercent = _accessTokenLifespan.Value * _minimalAccessTokenLifespanPercent;

			var byTime = _minimalAccessTokenLifespan ?? TimeSpan.MinValue;

			return byPercent > byTime ? byPercent : byTime;
		}
	}

	private static TimeSpan _minimalRefreshTokenLifespanResolved
	{
		get
		{
			if(_refreshTokenLifespan is null) throw new NullReferenceException();

			var byPercent = _refreshTokenLifespan.Value * _minimalRefreshTokenLifespanPercent;

			var byTime = _minimalRefreshTokenLifespan ?? TimeSpan.MinValue;

			return byPercent > byTime ? byPercent : byTime;
		}
	}

	public static bool IsAuthenticated =>
		_refreshToken is not null && _refreshValidTo.HasValue &&
		(_refreshValidTo - DateTime.UtcNow) > _minimalRefreshTokenLifespanResolved;

	#endregion

	#region events

	//public delegate void StatusCodeHandler(int statusCode);
	//public delegate void UserInfoHandler(UserInfo userInfoReceivedFromServer);

	//public static event StatusCodeHandler? RequestSucceeded;
	//public static event UserInfoHandler? AuthenticationSucceeded;

	//public static event StatusCodeHandler? RequestFailed;
	//public static event StatusCodeHandler? RefreshFailed;
	//public static event StatusCodeHandler? AuthenticationFailed;

	public static event Action<string>? AccessTokenChanged;

	#endregion

	#region private

	static AuthTokenProvider()
	{
		Log("static ctor started");

		_minimalAccessTokenLifespanPercent = 0.1;
		_minimalRefreshTokenLifespanPercent = 0.1;

		_maxRetries = 3;
		_retryDelay = TimeSpan.FromSeconds(1);

		_httpHandler = new()
		{
			SslProtocols = Environment.OSVersion.Version.Major > 10
				? SslProtocols.Tls13 : SslProtocols.Tls12
		};

		_httpClient = new(_httpHandler);
		_httpClient.Timeout = TimeSpan.FromSeconds(15);

		_jsonOpts = new(JsonSerializerDefaults.Web);

		WriteTokensInfo(null, TokenFilesHelper.ReadRefreshToken());

		Log("static ctor completed");
	}

	private static void WriteTokensInfo(string? access, string? refresh)
	{
		if(!string.IsNullOrWhiteSpace(access))
		{
			var decoded = JwtService.ValidateJwtToken(access,
				out var validTo, out var notBefore, out var issuedAt)!;

			_accessToken = access;
			_accessValidTo = validTo;
			_accessTokenLifespan = validTo - notBefore;

			AccessTokenChanged?.Invoke(_accessToken);

			Log("wrote new access token");
		}

		if(!string.IsNullOrWhiteSpace(refresh))
		{
			var decoded = JwtService.ValidateJwtToken(refresh,
				out var validTo, out var notBefore, out var issuedAt)!;

			_refreshToken = refresh;
			_refreshValidTo = validTo;
			_refreshTokenLifespan = validTo - notBefore;

			TokenFilesHelper.WriteRefreshToken(refresh);

			Log("wrote new refresh token");
		}
	}

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

		return new()
		{
			JwtInfo = jwtInfo,
			UserInfo = userInfo,
			AccessToken = accessToken,
			RefreshToken = refreshToken,
		};
	}

	#endregion

	#region auth methods

	public static async Task<AuthResult?> AuthenticateAsync(string login, string password)
		=> await AuthenticateAsync(new() { Email = login, Password = password });
	public static async Task<AuthResult?> AuthenticateAsync(LoginRequest request, int retryCount = 0)
	{
		if(retryCount > _maxRetries) return null;
		if(retryCount > 0) await Task.Delay(_retryDelay);

		Log($"{nameof(AuthenticateAsync)} started");

		var path = HostPathTls + ApiBasePath + AuthPath + QueryStartString
				+ RefreshJwtInBodyQuery + QueryAndString + RedirectToLoginQuery;

		try
		{
			var response = await _httpClient.PutAsync(path,
				JsonContent.Create(request, options: _jsonOpts));

			LogRequest(path, (int)response.StatusCode);

			if(!response.IsSuccessStatusCode)
				return await AuthenticateAsync(request, retryCount + 1);

			var jwtResponse = (await response.Content.ReadFromJsonAsync<JwtResponse>(_jsonOpts))!;

			WriteTokensInfo(jwtResponse.AccessToken, jwtResponse.RefreshToken);

			var result = DecodeJwtResponse(jwtResponse);
			CurrentUser = result.UserInfo;

			Log($"{nameof(AuthenticateAsync)} completed");
			return result;
		}
		catch(Exception e)
		{
			Log($"{nameof(AuthenticateAsync)} failed: {e.Message}");
			return await AuthenticateAsync(request, retryCount + 1);
		}
	}

	private static async Task RefreshAsync(int retryCount = 0)
	{
		if(retryCount > _maxRetries) return;
		if(retryCount > 0) await Task.Delay(_retryDelay);

		Log($"{nameof(RefreshAsync)} started");

		if(_refreshToken is null) throw new
				NullReferenceException("Attempt to refresh using null token.");

		var path = HostPathTls + ApiBasePath + AuthPath
			+ QueryStartString + RefreshJwtInBodyQuery;

		try
		{
			var response = await _httpClient.PatchAsync(path, JsonContent.Create(
				new RefreshJwtRequest { RefreshToken = _refreshToken }, options: _jsonOpts));

			LogRequest(path, (int)response.StatusCode);

			if(!response.IsSuccessStatusCode)
			{
				await RefreshAsync(retryCount + 1);
				return;
			}

			var jwtResponse = (await response.Content.ReadFromJsonAsync<JwtResponse>(_jsonOpts))!;

			WriteTokensInfo(jwtResponse.AccessToken, jwtResponse.RefreshToken);

			var result = DecodeJwtResponse(jwtResponse);
			CurrentUser = result.UserInfo;

			Log($"{nameof(RefreshAsync)} completed");
		}
		catch(Exception e)
		{
			Trace.WriteLine($"{nameof(RefreshAsync)} failed: {e.Message}.");
			await RefreshAsync(retryCount + 1);
			return;
		}
	}

	public static async Task LogoutAsync()
	{

	}

	#endregion

	public static async Task<string?> GetAccessToken()
	{
		if(_accessToken is not null && _accessTokenLifespan is not null &&
			(_accessValidTo - DateTime.UtcNow) > _minimalAccessTokenLifespanResolved)
			return _accessToken;

		await RefreshAsync();

		if(string.IsNullOrWhiteSpace(_accessToken)) throw new
				AggregateException("Unable to refresh access token.");

		return _accessToken;
	}
}
