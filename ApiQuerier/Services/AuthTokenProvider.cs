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
using System.Net;
using RestSharp;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;

namespace ApiQuerier.Services;

/// <summary>
/// A singleton gateway class for all authentication operations.
/// The role is to provide access token with auto refresh
/// to other classes.
/// </summary>
public class AuthTokenProvider
{
	private static AuthTokenProvider _instance;

	#region logging

	private static void Log(string message)
		=> Trace.WriteLine($"{nameof(AuthTokenProvider)}: {message}.");

	private static void LogRequest(string path, int code)
	{
		_instance.LastStatusCode = code;

		string msg = string.Empty;

		msg += $"request to {path} ";

		msg += (200 <= code && code <= 299)
			? "succeeded" : "failed";

		msg += code > 0
			? " with exception" : $": HTTP_{code}";

		Log(msg);
	}

	#endregion

	#region private fields

	private string? _accessToken;
	private string? _refreshToken;

	private TimeSpan? _accessTokenLifespan;
	private TimeSpan? _refreshTokenLifespan;

	private DateTime? _accessValidTo;
	private DateTime? _refreshValidTo;

	private double _minimalAccessTokenLifespanPercent;
	private double _minimalRefreshTokenLifespanPercent;

	private TimeSpan? _minimalAccessTokenLifespan;
	private TimeSpan? _minimalRefreshTokenLifespan;

	private readonly HttpClient _httpClient;
	private readonly HttpClientHandler _httpHandler;
	private readonly JsonSerializerOptions _jsonOpts;

	#endregion

	#region props for private fields

	/// <summary> Value must be in range (0; 1]. </summary>
	public double MinimalAccessTokenLifespanPercent
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
	public double MinimalRefreshTokenLifespanPercent
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

	public TimeSpan? MinimalAccessTokenLifespan
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

	public TimeSpan? MinimalRefreshTokenLifespan
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

	public TimeSpan HttpClientTimeout
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

	public int LastStatusCode { get; private set; }
	public UserInfo CurrentUser { get; private set; }

	#endregion

	#region calculated props

	private TimeSpan _minimalAccessTokenLifespanResolved
	{
		get
		{
			if(_accessTokenLifespan is null) throw new NullReferenceException();

			var byPercent = _accessTokenLifespan.Value * _minimalAccessTokenLifespanPercent;

			var byTime = _minimalAccessTokenLifespan ?? TimeSpan.MinValue;

			return byPercent > byTime ? byPercent : byTime;
		}
	}

	private TimeSpan _minimalRefreshTokenLifespanResolved
	{
		get
		{
			if(_refreshTokenLifespan is null) throw new NullReferenceException();

			var byPercent = _refreshTokenLifespan.Value * _minimalRefreshTokenLifespanPercent;

			var byTime = _minimalRefreshTokenLifespan ?? TimeSpan.MinValue;

			return byPercent > byTime ? byPercent : byTime;
		}
	}

	public bool IsAuthenticated => _accessToken is not null &&
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

	public event Action<string>? AccessTokenChanged;

	#endregion

	#region private

	/// <summary>
	/// Initializes HttpClient
	/// </summary>
	private AuthTokenProvider()
	{
		Log("ctor started");

		_minimalAccessTokenLifespanPercent = 0.1;
		_minimalRefreshTokenLifespanPercent = 0.1;


		_httpHandler = new()
		{
			SslProtocols = SslProtocols.Tls12,
			UseProxy = false,
			//ServerCertificateCustomValidationCallback = (_, cert, _, _) =>
			//{
			//	try
			//	{
			//		var crt = cert as X509Certificate2;
			//		var req = X509Certificate2.CreateFromCertFile("vdb_stm.crt");
			//		return crt.GetPublicKey().SequenceEqual(req.GetPublicKey());
			//	}
			//	catch
			//	{
			//		return false;
			//	}
			//},
			//Proxy = new WebProxy("socks5://5.42.95.199:59091")
			//{
			//	Credentials = new NetworkCredential("vdb", "8ws38CkTut3pUygaGdCUobYkR6tmZ5zU8kY5xry0iF5QbYCM"),
			//},
		};


		_httpClient = new(_httpHandler);
		_httpClient.Timeout = TimeSpan.FromSeconds(15);
		//_httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:125.0) Gecko/20100101 Firefox/125.0");
		//_httpClient.DefaultRequestVersion = new(2, 0);
		//_httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;


		_jsonOpts = new(JsonSerializerDefaults.Web);

		Log("ctor completed");
	}

	public static async Task<AuthTokenProvider> Create()
	{
		Log($"{nameof(Create)} started");

		if(_instance is null)
		{
			_instance = new AuthTokenProvider();

			_instance._refreshToken = TokenFilesHelper.ReadRefreshToken();
			if(_instance._refreshToken is not null) await _instance.RefreshAsync();
		}

		await _instance.RefreshIfNeededAsync();

		Log($"{nameof(Create)} completed");
		return _instance;
	}

	private void WriteTokensInfo(string? access, string? refresh)
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

	public async Task<AuthResult?> AuthenticateAsync(string login, string password)
		=> await AuthenticateAsync(new() { Email = login, Password = password });
	public async Task<AuthResult?> AuthenticateAsync(LoginRequest request)
	{
		Log($"{nameof(AuthenticateAsync)} started");

		var path = HostPathTls + ApiBasePath + AuthPath + QueryStartString
				+ RefreshJwtInBodyQuery + QueryAndString + RedirectToLoginQuery;

		try
		{
			_httpClient.DefaultRequestHeaders.Accept.Clear();
			var response = await _httpClient.PutAsync(path,
				JsonContent.Create(request, options: _jsonOpts));

			LogRequest(path, (int)response.StatusCode);

			if(!response.IsSuccessStatusCode)
				return null;

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
			return null;
		}
	}

	private async Task<bool> RefreshAsync()
	{
		Log($"{nameof(RefreshAsync)} started");

		if(_refreshToken is null) throw new
				NullReferenceException("Attempt to refresh using null token.");

		_httpClient.DefaultRequestHeaders.Accept.Clear();
		var path = HostPathTls + ApiBasePath + AuthPath
			+ QueryStartString + RefreshJwtInBodyQuery;

		try
		{
			var response = await _httpClient.PatchAsync(path, JsonContent.Create(
				new RefreshJwtRequest { RefreshToken = _refreshToken }, options: _jsonOpts));

			LogRequest(path, (int)response.StatusCode);

			if(!response.IsSuccessStatusCode) return false;

			var jwtResponse = (await response.Content.ReadFromJsonAsync<JwtResponse>(_jsonOpts))!;

			WriteTokensInfo(jwtResponse.AccessToken, jwtResponse.RefreshToken);

			var result = DecodeJwtResponse(jwtResponse);
			CurrentUser = result.UserInfo;

			Log($"{nameof(RefreshAsync)} completed");
			return true;
		}
		catch(Exception e)
		{
			Log($"{nameof(RefreshAsync)} failed: {e.Message}.");
			return false;
		}
	}

	private async Task<bool> RefreshIfNeededAsync()
	{
		if(_accessToken is null || !_accessTokenLifespan.HasValue || !_accessValidTo.HasValue ||
			(_accessValidTo - DateTime.UtcNow) < _minimalAccessTokenLifespanResolved)
			if(_refreshToken is not null) return await RefreshAsync();

		return true;
	}

	public static async Task LogoutAsync()
	{

	}

	#endregion

	public async Task<string?> GetAccessToken()
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
