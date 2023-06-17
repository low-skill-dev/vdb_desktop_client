//using main_server_api.Models.Auth;
//using main_server_api.Models.UserApi.Application.Device;
//using ServerQuerier;
//using ServerQuerier.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http.Headers;
//using System.Net.Http.Json;
//using System.Net.Security;
//using System.Security.Authentication;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using static System.Environment;
//using ServicesLayer.Models.Services;

//namespace ServerQuerier;
//public sealed class VdbClient
//{
//	#region paths
//	//public static string ApplicationPath => Path.Join(GetFolderPath(SpecialFolder.Personal), @"Vdb");
//	//public static string TokenPath => Path.Join(ApplicationPath, @"refresh.token");

//	//public string HostPathTls { get; set; } = @"https://vdb.bruhcontent.ru";
//	//public string QueryStartString { get; set; } = @"?";
//	//public string ApiBasePath { get; set; } = @"/api";
//	//public string AuthPath { get; set; } = @"/auth";
//	//public string SelfPath { get; set; } = @"/self";
//	//public string RefreshJwtInBodyQuery { get; set; } = "refreshJwtInBody=true";
//	//public string ConnectionPath { get; set; } = @"/connection";
//	//public string NodesListPath { get; set; } = @"/nodes-list";
//	//public string DevicePath { get; set; } = @"/device";
//	//public string OkIfExistsQuery { get; set; } = "allowDuplicate=true";
//	#endregion

//	//private  JsonSerializerOptions jsonOptions;
//	//private  HttpClientHandler httpHandler;
//	//private  HttpClient httpClient;
//	//public int LastStatusCode { get; private set; }


//	//private string? _AccessToken;
//	//public string? RefreshToken { get; private set; }
//	//public string? AccessToken {
//	//	get {
//	//		return _AccessToken;
//	//	}
//	//	private set {
//	//		httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", value);
//	//		_AccessToken = value;
//	//	}
//	//}

//	//public bool IsLoggedIn => RefreshToken is null && AccessToken is not null;

//	//private DateTime RefreshValidToUtc;
//	//private DateTime AccessValidToUtc;

//	public VdbClient()
//	{
//		httpHandler = new() {
//			SslProtocols = Environment.OSVersion.Version.Major > 10
//				? SslProtocols.Tls13
//				: SslProtocols.Tls12,
//		};

//		httpClient = new(httpHandler);
//		httpClient.Timeout = TimeSpan.FromSeconds(10);

//		jsonOptions = new(JsonSerializerDefaults.Web);
//	}

//	public async Task<bool> TryLoadUser()
//	{
//		Console.WriteLine("TryLoadUser()");
//		try {
//			var token = File.ReadAllText(TokenPath);
//			var validated = JwtService.ValidateJwtToken(token, out var ValidTo);
//			if(validated is null) {
//				return false;
//			}

//			var result = await Refresh(new() { RefreshToken = token });
//			if(result) {
//				Console.WriteLine("Token loaded from local and refreshed successfully");
//			} else {
//				Console.WriteLine("Server refused token from local storage.");
//			}
//			return result;
//		} catch {
//			return false;
//		}

//		return this.RefreshToken is not null;
//	}

//	//public bool LogOut()
//	//{
//	//	try {
//	//		File.Delete(TokenPath);
//	//		return true;
//	//	} catch {
//	//		return false;
//	//	}
//	//}


//	//public async Task<bool> Authenticate(LoginRequest request)
//	//{
//	//	Console.WriteLine("Calling Authenticate().");
//	//	Console.WriteLine("Sending put request to the server...");
//	//	Console.WriteLine($"Sending to url: {HostPathTls + ApiBasePath + AuthPath + QueryStartString + RefreshJwtInBodyQuery}");
//	//	Console.WriteLine($"Sending body: {await JsonContent.Create(request, options: jsonOptions).ReadAsStringAsync()}");
//	//	var response = await httpClient.PostAsync(
//	//		HostPathTls + ApiBasePath + AuthPath + QueryStartString + RefreshJwtInBodyQuery,
//	//		JsonContent.Create(request, options: jsonOptions));
//	//	LastStatusCode = (int)response.StatusCode;

//	//	Console.WriteLine($"Responded with {response.StatusCode}");

//	//	if(!response.IsSuccessStatusCode) return false;
//	//	try {
//	//		var result = await response.Content.ReadFromJsonAsync<JwtResponse>(jsonOptions);
//	//		if(result is null) return false;
//	//		HandleJwtResponse(result);
//	//	} catch {
//	//		return false;
//	//	}

//	//	return this.AccessToken is not null;
//	//}

//	//public async Task<bool> Refresh()
//	//{
//	//	return await this.Refresh(new() { RefreshToken = this.RefreshToken! });
//	//}
//	//public async Task<bool> Refresh(RefreshJwtRequest request)
//	//{
//	//	Console.WriteLine("Refreshing token...");
//	//	var response = await httpClient.PatchAsync(
//	//		HostPathTls + ApiBasePath + AuthPath + QueryStartString + RefreshJwtInBodyQuery,
//	//		JsonContent.Create(request, options: jsonOptions));
//	//	LastStatusCode = (int)response.StatusCode;


//	//	if(!response.IsSuccessStatusCode) return false;
//	//	try {
//	//		var result = await response.Content.ReadFromJsonAsync<JwtResponse>(jsonOptions);
//	//		if(result is null) return false;
//	//		HandleJwtResponse(result);
//	//	} catch {
//	//		return false;
//	//	}

//	//	return this.AccessToken is not null;
//	//}

//	//public async Task<bool> RegisterDevice(AddDeviceRequest request)
//	//{
//	//	Console.WriteLine("Calling RegisterDevice().");
//	//	await onRequest();

//	//	Console.WriteLine("Sending put request to the server...");
//	//	Console.WriteLine($"Sending to url: {HostPathTls + ApiBasePath + DevicePath + QueryStartString + OkIfExistsQuery}");
//	//	Console.WriteLine($"Sending body: {await JsonContent.Create(request, options: jsonOptions).ReadAsStringAsync()}");
//	//	Console.WriteLine($"Sending headers: {httpClient.DefaultRequestHeaders.Authorization}");
//	//	var response = await httpClient.PutAsync(
//	//		HostPathTls + ApiBasePath + DevicePath + QueryStartString + OkIfExistsQuery,
//	//		JsonContent.Create(request, options: jsonOptions));
//	//	LastStatusCode = (int)response.StatusCode;

//	//	Console.WriteLine($"Responded with {response.StatusCode}");

//	//	return response.IsSuccessStatusCode;
//	//}

//	//public async Task<bool> UnregisterDevice(AddDeviceRequest request)
//	//{
//	//	await onRequest();

//	//	var response = await httpClient.PatchAsync(
//	//		HostPathTls + ApiBasePath + DevicePath,
//	//		JsonContent.Create(request, options: jsonOptions));
//	//	LastStatusCode = (int)response.StatusCode;

//	//	return response.IsSuccessStatusCode;
//	//}
//	//public async Task<bool> TerminateSelf()
//	//{

//	//	var response = await httpClient.DeleteAsync(
//	//		HostPathTls + ApiBasePath + AuthPath + SelfPath + $"/{this.RefreshToken}");
//	//	LastStatusCode = (int)response.StatusCode;

//	//	return response.IsSuccessStatusCode;
//	//}


//	//public async Task<PublicNodeInfo[]?> GetNodesList()
//	//{
//	//	return await (await httpClient
//	//		.GetAsync(HostPathTls + ApiBasePath + ConnectionPath + NodesListPath))
//	//		.Content.ReadFromJsonAsync<PublicNodeInfo[]>();
//	//}

//	//public async Task<ConnectDeviceResponse?> ConnectToNode(ConnectDeviceRequest request)
//	//{
//	//	await onRequest();

//	//	var response = await httpClient.PutAsync(
//	//		HostPathTls + ApiBasePath + ConnectionPath,
//	//		JsonContent.Create(request, options: jsonOptions));
//	//	LastStatusCode = (int)response.StatusCode;

//	//	return response.IsSuccessStatusCode
//	//		? await response.Content.ReadFromJsonAsync<ConnectDeviceResponse>(jsonOptions)
//	//		: null;
//	//}
//}
