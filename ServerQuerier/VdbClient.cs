using main_server_api.Models.Auth;
using main_server_api.Models.UserApi.Application.Device;
using ServerQuerier;
using ServerQuerier.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Environment;
using ServicesLayer.Models.Services;

namespace ServerQuerier;
public sealed class VdbClient
{
	#region paths
	public static string ApplicationPath => Path.Join(GetFolderPath(SpecialFolder.Personal), @"Vdb");
	public static string TokenPath => Path.Join(ApplicationPath, @"refresh.token");

	public string HostPathTls { get; set; } = @"https://vdb.bruhcontent.ru";
	public string QueryStartString { get; set; } = @"?";
	public string ApiBasePath { get; set; } = @"/api";
	public string AuthPath { get; set; } = @"/auth";
	public string RefreshJwtInBodyQuery { get; set; } = "refreshJwtInBody=true";
	public string ConnectionPath { get; set; } = @"/connection";
	public string NodesListPath { get; set; } = @"/nodes-list";
	public string DevicePath { get; set; } = @"/device";
	public string OkIfExistsQuery { get; set; } = "okIfExists=true";
	#endregion

	private readonly JsonSerializerOptions jsonOptions;
	private readonly HttpClientHandler httpHandler;
	private readonly HttpClient httpClient;
	public int LastStatusCode { get; private set; }


	private string? _AccessToken;
	public string? RefreshToken { get; private set; }
	public string? AccessToken {
		get {
			return _AccessToken;
		}
		private set {
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
			_AccessToken = value;
		}
	}



	public bool IsLoggedIn => RefreshToken is null && AccessToken is not null;

	private DateTime RefreshValidToUtc;
	private DateTime AccessValidToUtc;

	public VdbClient()
	{
		httpHandler = new() {
			SslProtocols = Environment.OSVersion.Version.Major > 10
				? SslProtocols.Tls13
				: SslProtocols.Tls12,
		};

		httpClient = new(httpHandler);
		httpClient.Timeout = TimeSpan.FromSeconds(10);

		jsonOptions = new(JsonSerializerDefaults.Web);
	}

	public async Task onRequest()
	{
		if(AccessValidToUtc.AddSeconds(-10) < DateTime.UtcNow) {
			await Refresh();
		}
	}

	public void HandleJwtResponse(JwtResponse response)
	{
		this.RefreshToken = response.RefreshToken;
		this.AccessToken = response.AccessToken;

		var validatedR = JwtService.ValidateJwtToken(RefreshToken!, out var ValidToR);
		var validatedA = JwtService.ValidateJwtToken(AccessToken, out var ValidToA);

		this.RefreshValidToUtc = ValidToR;
		this.AccessValidToUtc = ValidToA;

		Console.WriteLine("New refresh token was saved.");
		File.WriteAllText(TokenPath, response.RefreshToken);
	}

	public async Task<bool> TryLoadUser()
	{
		try {
			var token = File.ReadAllText(TokenPath);
			var validated = JwtService.ValidateJwtToken(token, out var ValidTo);
			if(validated is null) {
				return false;
			}

			var result = await Refresh(new() { RefreshToken = token });
			if(result) {
				Console.WriteLine("Token loaded from local and refreshed successfully");
			} else {
				Console.WriteLine("Server refused token from local storage.");
			}
			return result;
		} catch {
			return false;
		}

		return this.RefreshToken is not null;
	}

	public bool LogOut()
	{
		try {
			File.Delete(TokenPath);
			return true;
		} catch {
			return false;
		}
	}


	public async Task<bool> Authenticate(LoginRequest request)
	{
		var response = await httpClient.PostAsync(
			HostPathTls + ApiBasePath + AuthPath + QueryStartString + RefreshJwtInBodyQuery,
			JsonContent.Create(request, options: jsonOptions));
		LastStatusCode = (int)response.StatusCode;

		if(!response.IsSuccessStatusCode) return false;
		try {
			var result = await response.Content.ReadFromJsonAsync<JwtResponse>(jsonOptions);
			if(result is null) return false;
			HandleJwtResponse(result);
		} catch {
			return false;
		}

		return this.AccessToken is not null;
	}

	public async Task<bool> Refresh()
	{
		return await this.Refresh(new() { RefreshToken = this.RefreshToken! });
	}
	public async Task<bool> Refresh(RefreshJwtRequest request)
	{
		Console.WriteLine("Refreshing token...");
		var response = await httpClient.PatchAsync(
			HostPathTls + ApiBasePath + AuthPath + QueryStartString + RefreshJwtInBodyQuery,
			JsonContent.Create(request, options: jsonOptions));
		LastStatusCode = (int)response.StatusCode;


		if(!response.IsSuccessStatusCode) return false;
		try {
			var result = await response.Content.ReadFromJsonAsync<JwtResponse>(jsonOptions);
			if(result is null) return false;
			HandleJwtResponse(result);
		} catch {
			return false;
		}

		return this.AccessToken is not null;
	}

	public async Task<bool> RegisterDevice(AddDeviceRequest request)
	{
		var response = await httpClient.PutAsync(
			HostPathTls + ApiBasePath + DevicePath + QueryStartString + OkIfExistsQuery,
			JsonContent.Create(request, options: jsonOptions));
		LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode;
	}

	public async Task<PublicNodeInfo[]?> GetNodesList()
	{
		return await (await httpClient
			.GetAsync(HostPathTls + ApiBasePath + ConnectionPath + NodesListPath))
			.Content.ReadFromJsonAsync<PublicNodeInfo[]>();
	}

	public async Task<ConnectDeviceResponse?> ConnectToNode(ConnectDeviceRequest request)
	{
		var response = await httpClient.PutAsync(
			HostPathTls + ApiBasePath + ConnectionPath,
			JsonContent.Create(request, options: jsonOptions));
		LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<ConnectDeviceResponse>(jsonOptions)
			: null;
	}
}
