using ApiQuerier.Helpers;
using System.Diagnostics;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
			SslProtocols = SslProtocols.Tls12,
			ServerCertificateCustomValidationCallback = (_, cert, _, _) =>
			{

				try
				{
					var req = X509Certificate.CreateFromCertFile("vdb_stm.crt");
					return cert.GetPublicKey().SequenceEqual(req.GetPublicKey());
				}
				catch
				{
					return false;
				}

			},
			UseProxy = false,
			Proxy = new WebProxy("socks5://5.42.95.199:59091")
			{
				Credentials = new NetworkCredential("vdb", "8ws38CkTut3pUygaGdCUobYkR6tmZ5zU8kY5xry0iF5QbYCM"),
			}
		};

		httpClient = new(httpHandler);
		httpClient.Timeout = TimeSpan.FromSeconds(HttpTimeoutSeconds);

		jsonOptions = new(JsonSerializerDefaults.Web);


		Trace.WriteLine($"{nameof(WebCommon)} static ctor completed.");
	}
}
