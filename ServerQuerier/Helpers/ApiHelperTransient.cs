using ServerQuerier.Models.Auth;
using ServerQuerier.Models.Device;
using ServerQuerier.Models.Nodes;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text.Json;
using static ServerQuerier.Helpers.Constants;

namespace ServerQuerier.Helpers;

/* This service is designed to be transient.
 * However, it can be safely used for multiple rapid requests.
 * Recommended lifetime is no more than 30 seconds. This service is not
 */

/// <summary>
/// This service is designed to be transient. <br/>
/// However, it can be safely used for multiple rapid requests.<br/>
/// Recommended lifetime is no more than 30 seconds.<br/>
/// After that span, access <b>token</b> can became invalid and this service<br/>
/// <b>WILL NOT revalidate</b> it, what <b>causes unhandled exceptions</b>.
/// </summary>
public sealed class ApiHelperTransient
{
	#region private static
	private static readonly JsonSerializerOptions jsonOptions;
	private static readonly HttpClientHandler httpHandler;
	private static readonly HttpClient httpClient;

	private static DateTime AccessExpires;
	private static string? _accessToken;
	private static string? AccessToken {
		get {
			return _accessToken;
		}
		set {
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
			_accessToken = value;
		}
	}
	private static string? RefreshToken { get; set; }

	static ApiHelperTransient()
	{
		#region create clients

		httpHandler = new() {
			SslProtocols = Environment.OSVersion.Version.Major > 10
				? SslProtocols.Tls13
				: SslProtocols.Tls12,
		};

		httpClient = new(httpHandler);
		httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

		jsonOptions = new(JsonSerializerDefaults.Web);

		#endregion

		#region init fields

		AccessExpires = DateTime.MinValue;

		#endregion
	}
	#endregion

	public static UserInfo UserInfo { get; private set; }

	public static event Action? AuthorizationFailed;

	/* We are doing like transient, but actually we are going
	 * to change static fields, so we need async factory.
	 */
	private ApiHelperTransient() { }

	public int LastStatusCode { get; private set; }

	private static bool IsAccessValid =>
		AccessToken is not null &&
		AccessExpires.AddSeconds(-1 * HttpTimeoutSeconds) > DateTime.UtcNow;
	public static async Task<ApiHelperTransient?> Create(AuthResult? givenResult = null)
	{
		Console.WriteLine($"Creating {nameof(ApiHelperTransient)}.");

		if(!IsAccessValid) {
			if(givenResult is null)
				Console.WriteLine($"Access token expired and will be refreshed.");
			else
				Console.WriteLine($"Access token will be imported from the passed model.");
			var loaded = givenResult ?? await AuthHelper.RefreshUsingLocalToken();
			if(loaded is null) {
				AuthorizationFailed?.Invoke();
				return null;
			}

			ApiHelperTransient.UserInfo = loaded.UserInfo;

			RefreshToken = loaded.RefreshToken;
			AccessToken = loaded.AccessToken;
			AccessExpires = loaded.JwtInfo.Exp;
		}

		return new ApiHelperTransient();
	}

	public async Task<bool> RegisterDevice(AddDeviceRequest request)
	{
		var response = await httpClient.PutAsync(
			HostPathTls + ApiBasePath + DevicePath + QueryStartString + OkIfExistsQuery,
			JsonContent.Create(request, options: jsonOptions));

		this.LastStatusCode = (int)response.StatusCode;
		return response.IsSuccessStatusCode;
	}

	public async Task<bool> UnregisterDevice(AddDeviceRequest request)
	{
		var response = await httpClient.PatchAsync(
			HostPathTls + ApiBasePath + DevicePath,
			JsonContent.Create(request, options: jsonOptions));

		this.LastStatusCode = (int)response.StatusCode;
		return response.IsSuccessStatusCode;
	}

	public async Task<bool> ServerLogout()
	{
		var response = await httpClient.DeleteAsync(
			HostPathTls + ApiBasePath + AuthPath + SelfPath + $"/{RefreshToken}");

		this.LastStatusCode = (int)response.StatusCode;
		return response.IsSuccessStatusCode;
	}

	public async Task<PublicNodeInfo[]?> GetNodesList()
	{
		var response = await httpClient
			.GetAsync(HostPathTls + ApiBasePath + ConnectionPath + NodesListPath);

		this.LastStatusCode = (int)response.StatusCode;
		return await response.Content.ReadFromJsonAsync<PublicNodeInfo[]>();
	}

	public async Task<ConnectDeviceResponse?> ConnectToNode(ConnectDeviceRequest request)
	{
		var response = await httpClient.PutAsync(
			HostPathTls + ApiBasePath + ConnectionPath,
			JsonContent.Create(request, options: jsonOptions));

		this.LastStatusCode = (int)response.StatusCode;
		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<ConnectDeviceResponse>(jsonOptions)
			: null;
	}
}
