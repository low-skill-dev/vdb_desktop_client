﻿using ApiModels.Auth;
using ApiModels.Device;
using ApiModels.Node;
using ApiQuerier.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Text.Json;
using static ApiQuerier.Helpers.Constants;
using static ApiQuerier.Helpers.WebCommon;

namespace ApiQuerier.Helpers;

// refactor 27-08-2023

/// <summary>
/// This service is designed to be transient. <br/>
/// However, it can be safely used for multiple rapid requests.<br/>
/// Recommended lifetime is no more than 10 seconds.<br/>
/// After that span, access <b>token</b> can became invalid and this service<br/>
/// <b>WILL NOT revalidate</b> it, what <b>causes unhandled exceptions</b>.
/// </summary>
public sealed class ApiHelperTransient
{
	#region private static

	private static DateTime _accessExpires;
	private static string? _accessToken;

	private static string? RefreshToken { get; set; }
	private static string? AccessToken
	{
		get
		{
			return _accessToken;
		}
		set
		{
			Trace.WriteLine($"Setting new access token to the {nameof(httpClient)}.");

			httpClient.DefaultRequestHeaders.Authorization
				= new AuthenticationHeaderValue("Bearer", value);
			_accessToken = value;

			Trace.WriteLine($"New access token was set.");
		}
	}

	static ApiHelperTransient()
	{
		Trace.WriteLine($"{nameof(ApiHelperTransient)} static ctor started.");

		_accessExpires = DateTime.MinValue;

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

	public int _lastStatusCode;
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
	internal static async Task<ApiHelperTransient?> Create(AuthResult? givenResult)
	{
		Trace.WriteLine($"{nameof(ApiHelperTransient)} ctor started.");

		var isAccessValid =
			!string.IsNullOrWhiteSpace(AccessToken) &&
			_accessExpires.AddSeconds(-5) < DateTime.UtcNow;

		if(!isAccessValid)
		{
			/* Commonly, external users does not pass any 'givenResult' here, 
			 * so every time our local access token is expired, we just go and refresh it.
			 * 
			 * But how the refresh works? Well, it calls the same method
			 * (I mean... this method) and passing there the AuthResult that
			 * was received from the server. 
			 * 
			 * So this method is actually double functioned: 
			 * when the external project is trying to create some transient 
			 * instance of this class, that is really needed to perform requests: 
			 * it just passes null here, but when the Auth helper calling this method, 
			 * it literally tells it to write AuthResult to our static fields.
			 */
			if(givenResult is null)
				Trace.WriteLine($"Access token expired and will be refreshed.");
			else
				Trace.WriteLine($"Access token will be imported from the passed model.");

			var loaded = givenResult ?? await AuthHelper.RefreshUsingLocalToken();

			if(loaded is null)
			{
				Trace.WriteLine($"{nameof(ApiHelperTransient)} ctor completed. " +
					$"Authorization failed.");

				AuthorizationFailed?.Invoke(AuthHelper.LastStatusCode);
				return null;
			}
			else
			{
				AuthorizationSucceeded?.Invoke(AuthHelper.LastStatusCode);
			}

			UserInfo = loaded.UserInfo;

			RefreshToken = loaded.RefreshToken;
			AccessToken = loaded.AccessToken;
			_accessExpires = loaded.JwtInfo.Exp;
		}

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

	public async Task<ConnectDeviceResponse?> ConnectToNode(ConnectDeviceRequest request)
	{
		Trace.WriteLine($"{nameof(ConnectToNode)} started.");

		var response = await httpClient.PutAsync(
			HostPathTls + ApiBasePath + ConnectionPath,
			JsonContent.Create(request, options: jsonOptions));

		this.LastStatusCode = (int)response.StatusCode;

		return response.IsSuccessStatusCode
			? await response.Content.ReadFromJsonAsync<ConnectDeviceResponse>(jsonOptions)
			: null;
	}
}
