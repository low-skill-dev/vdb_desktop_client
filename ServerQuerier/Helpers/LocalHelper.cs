using static ServerQuerier.Helpers.Constants;

namespace ServerQuerier.Helpers;
public static class LocalHelper
{
	public static string TokenPath => Path.Join(WorkingDirectory, @"refresh.token");

	public static async Task WriteRefreshToken(string token)
	{
		await File.WriteAllTextAsync(TokenPath, token);
	}

	public static async Task<string> ReadRefreshToken()
	{
		return await File.ReadAllTextAsync(TokenPath);
	}

	public static void DeleteRefreshToken()
	{
		File.Delete(TokenPath);
	}
}
