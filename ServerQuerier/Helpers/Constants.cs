namespace ServerQuerier.Helpers;
public static class Constants
{
	public static string WorkingDirectory => Environment.CurrentDirectory;

	public const string HostPathTls = @"https://vdb.bruhcontent.ru";
	public const string QueryStartString = @"?";
	public const string ApiBasePath = @"/api";
	public const string AuthPath = @"/auth";
	public const string SelfPath = @"/self";
	public const string RefreshJwtInBodyQuery = "refreshJwtInBody=true";
	public const string ConnectionPath = @"/connection";
	public const string NodesListPath = @"/nodes-list";
	public const string DevicePath = @"/device";
	public const string OkIfExistsQuery = "allowDuplicate=true";
	public const int HttpTimeoutSeconds = 10;
}

