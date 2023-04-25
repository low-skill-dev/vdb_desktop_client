using main_server_api.Models.Auth;
using main_server_api.Models.UserApi.Application.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Net.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerQuerier;
public class VdbClient
{
	public int LastStatusCode { get; protected set; }

	public string HostPathTls { get; set; } = @"https://vdb.bruhcontent.ru";
	public string ApiBasePath { get; set; } = @"/api";
	public string AuthPath { get; set; } = @"/auth";
	public string QueryStartString { get; set; } = @"?";
	public string RefreshJwtInBodyQuery { get; set; } = "refreshJwtInBody=true";
	public string ConnectionPath { get; set; } = @"/connection";
	public string DevicePath { get; set; } = @"/device";

	private readonly JsonSerializerOptions jsonOptions;
	private readonly HttpClientHandler httpHandler;
	private readonly HttpClient httpClient;


	public VdbClient()
	{
		httpHandler = new HttpClientHandler();
		httpHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls13;

		httpClient = new(httpHandler);
		httpClient.Timeout = TimeSpan.FromSeconds(10);

		jsonOptions = new(JsonSerializerDefaults.Web);
	}

	public async Task<JwtResponse?> Authenticate(LoginRequest request)
	{
		var response = await httpClient.PostAsync(
			HostPathTls + ApiBasePath + AuthPath + QueryStartString + RefreshJwtInBodyQuery,
			JsonContent.Create(request, options: jsonOptions));
		LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<JwtResponse>(jsonOptions)
			: null;
	}

	public async Task<AddDeviceResponse?> RegisterDevice(AddDeviceRequest request)
	{
		var response = await httpClient.PutAsync(
			HostPathTls + ApiBasePath + DevicePath,
			JsonContent.Create(request, options: jsonOptions));
		LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<AddDeviceResponse>(jsonOptions)
			: null;
	}

	public async Task<AddDeviceResponse?> ConnectToNode(ConnectDeviceRequest request)
	{
		var response = await httpClient.PutAsync(
			HostPathTls + ApiBasePath + ConnectionPath,
			JsonContent.Create(request, options: jsonOptions));
		LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<AddDeviceResponse>(jsonOptions)
			: null;
	}
}
