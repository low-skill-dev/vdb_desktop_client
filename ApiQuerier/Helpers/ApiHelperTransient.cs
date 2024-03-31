using ApiModels.Device;
using ApiModels.Node;
using ApiQuerier.Models;
using ApiQuerier.Services;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using static ApiQuerier.Helpers.Constants;
using static ApiQuerier.Helpers.WebCommon;

namespace ApiQuerier.Helpers;

/// <summary>
/// This service is designed to be transient. <br/>
/// However, it can be safely used for multiple rapid requests.<br/>
/// Recommended lifetime is no more than 5 seconds.<br/>
/// After that span, access <b>token</b> can became invalid and this service<br/>
/// <b>WILL NOT revalidate</b> it, what <b>causes unhandled exceptions</b>.
/// </summary>
public sealed class ApiHelperTransient
{
	#region private static

	private static DateTime _accessExpires;
	private static string? _accessToken;

	private static DateTime _refreshExpires;
	private static string? _refreshToken;

	private static string? RefreshToken
	{
		get
		{
			return _refreshToken;
		}
		set
		{
			Trace.WriteLine($"Setting new refresh token.");

			if(value is null)
			{
				Trace.WriteLine($"Token was null.");
				return;
			}

			JwtService.ValidateJwtToken(value, out _refreshExpires, out _, out _);

			_refreshToken = value;

			Trace.WriteLine($"New refresh token was set.");
		}
	}
	private static string? AccessToken
	{
		get
		{
			return _accessToken;
		}
		set
		{
			Trace.WriteLine($"Setting new access token to the {nameof(HttpClient)}.");

			if(value is null)
			{
				Trace.WriteLine($"Token was null.");
				return;
			}

			JwtService.ValidateJwtToken(value, out _accessExpires, out _, out _);

			httpClient.DefaultRequestHeaders.Authorization
				= new AuthenticationHeaderValue("Bearer", value);
			_accessToken = value;

			Trace.WriteLine($"New access token was set.");
		}
	}

	static ApiHelperTransient()
	{
		Trace.WriteLine($"{nameof(ApiHelperTransient)} static ctor started.");

		_refreshExpires = _accessExpires = DateTime.MinValue;

		Trace.WriteLine($"{nameof(ApiHelperTransient)} static ctor completed.");
	}

	#endregion

	#region public static

	/* In this class we actually do need this static events to make 
	 * it possible for UI to know that any of the clients failed 
	 * during some request. The common use is to show the notification,
	 * but also, if the 403 code was returned, to make user log in again.
	 */
	public delegate void RequestStatusCodeHandler(int statusCode);
	public static event RequestStatusCodeHandler? AuthorizationFailed;
	public static event RequestStatusCodeHandler? AuthorizationSucceeded;
	public static event RequestStatusCodeHandler? RequestFailed;
	public static event RequestStatusCodeHandler? RequestSucceeded;

	public static UserInfo? UserInfo { get; private set; }

	#endregion


	private int _lastStatusCode;
	public int LastStatusCode
	{
		get
		{
			return _lastStatusCode;
		}
		private set
		{
			if(value < 200 || value > 299)
			{
				Trace.WriteLine($"Request failed: HTTP_{value}.");
				RequestFailed?.Invoke(value);
			}
			else
			{
				Trace.WriteLine($"Request succeeded: HTTP_{value}.");
				RequestSucceeded?.Invoke(value);
			}

			_lastStatusCode = value;
		}
	}

	/* We are doing like transient, but actually we are going
	 * to change static fields, so we need async factory.
	 */
	private ApiHelperTransient() { }
	public static async Task<ApiHelperTransient?> Create() => await Create(null);
	public static async Task<ApiHelperTransient?> Create(AuthResult? givenResult)
	{
		Trace.WriteLine($"{nameof(ApiHelperTransient)} ctor started.");

		if((await AuthTokenProvider.Create()).IsAuthenticated) AccessToken =
				await (await AuthTokenProvider.Create()).GetAccessToken();
		else
			return null;

		(await AuthTokenProvider.Create()).AccessTokenChanged += 
			(tkn) => AccessToken = tkn;

		Trace.WriteLine($"{nameof(ApiHelperTransient)} ctor completed.");
		return new ApiHelperTransient();
	}

	public async Task<bool> RegisterDevice(AddDeviceRequest request)
	{
		Trace.WriteLine($"{nameof(RegisterDevice)} started.");

		var response = await httpClient.PutAsync(
			HostPathTls + ApiBasePath + DevicePath + QueryStartString + OkIfExistsQuery,
			JsonContent.Create(request, options: jsonOptions));

		this.LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode;
	}

	public async Task<bool> UnregisterDevice(AddDeviceRequest request)
	{
		Trace.WriteLine($"{nameof(UnregisterDevice)} started.");

		var response = await httpClient.PatchAsync(
			HostPathTls + ApiBasePath + DevicePath,
			JsonContent.Create(request, options: jsonOptions));

		this.LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode;
	}

	public async Task<bool> ServerLogout()
	{
		Trace.WriteLine($"{nameof(ServerLogout)} started.");

		var response = await httpClient.DeleteAsync(
			HostPathTls + ApiBasePath + AuthPath + SelfPath + $"/{RefreshToken}");

		this.LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode;
	}

	public async Task<PublicNodeInfo[]?> GetNodesList()
	{
		Trace.WriteLine($"{nameof(GetNodesList)} started.");

		var response = await httpClient
			.GetAsync(HostPathTls + ApiBasePath + ConnectionPath + NodesListPath);

		this.LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<PublicNodeInfo[]>()
			: null;
	}

	public async Task<ConnectDeviceResponse?> ConnectToNode(ConnectDeviceRequest request, bool doNotRetry = false)
	{
		Trace.WriteLine($"{nameof(ConnectToNode)} started.");

		HttpResponseMessage response;
		try
		{
			response = await httpClient.PutAsync(
				HostPathTls + ApiBasePath + ConnectionPath,
				JsonContent.Create(request, options: jsonOptions));
		}
		catch(Exception ex) when (ex.Message.Contains("No such host is known", StringComparison.InvariantCultureIgnoreCase))
		{
			return null;
		}

		this.LastStatusCode = (int)response.StatusCode;
#if DEBUG
		if(DateTime.UtcNow < new DateTime(2023,10,25)) this.LastStatusCode = 401;
#endif
		if(!doNotRetry && this.LastStatusCode == 401)
		{
			return await ConnectToNode(request, true);
		}

		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<ConnectDeviceResponse>(jsonOptions)
			: null;
	}
}
