namespace ApiQuerier.Helpers;

// refactor 27-08-2023

public static class Constants
{
	//public const string HostPathTls = @"https://vdb.lowskill.dev";
	public const string HostPathTls = @"https://5.42.95.199:52933";
	public const string QueryStartString = @"?";
	public const string QueryAndString = @"&";
	public const string ApiBasePath = @"/api";
	public const string AuthPath = @"/auth";
	public const string SelfPath = @"/self";
	public const string RefreshJwtInBodyQuery = "refreshJwtInBody=true";
	public const string RedirectToLoginQuery = "redirectToLogin=true";
	public const string ConnectionPath = @"/connection";
	public const string NodesListPath = @"/nodes-list";
	public const string DevicePath = @"/device";
	public const string OkIfExistsQuery = "allowDuplicate=true";
	public const int HttpTimeoutSeconds = 10;
}

