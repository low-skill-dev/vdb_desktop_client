using ApiQuerier.Helpers;
using System.Diagnostics;
using System.Security.Authentication;
using System.Text.Json;
using static ApiQuerier.Helpers.Constants;

namespace ApiQuerier.Helpers;

internal static class WebCommon
{
	private static readonly HttpClientHandler httpHandler;

	public static readonly JsonSerializerOptions jsonOptions;
	public static readonly HttpClient httpClient;

	static WebCommon()
	{
		Trace.WriteLine($"{nameof(WebCommon)} static ctor started.");


		httpHandler = new()
		{
			SslProtocols = Environment.OSVersion.Version.Major > 10
				? SslProtocols.Tls13 : SslProtocols.Tls12 
		};

		httpClient = new(httpHandler);
		httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

		jsonOptions = new(JsonSerializerDefaults.Web);


		Trace.WriteLine($"{nameof(WebCommon)} static ctor completed.");
	}
}
