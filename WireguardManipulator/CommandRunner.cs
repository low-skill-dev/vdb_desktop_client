
using System.Diagnostics;

namespace WireguardManipulator;

internal static class CommandRunner
{
	/// <returns>stdout cmd response.</returns>
	/// <exception cref="AggregateException"></exception>
	public static async Task<string> RunAsync(string command)
	{
		try
		{
			var proc = new Process();
			var info = new ProcessStartInfo();
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			info.UseShellExecute = false;
			info.CreateNoWindow = true;

			info.FileName = "cmd.exe";
			info.Arguments = "/c " + command;
			proc.StartInfo = info;
			_ = proc.Start();
			await proc.WaitForExitAsync();

			var error = await proc.StandardError.ReadToEndAsync();
			if(!string.IsNullOrWhiteSpace(error))
				throw new AggregateException(error);

			return await proc.StandardOutput.ReadToEndAsync();
		}
		catch(Exception ex)
		{
			return ex.Message;
		}
	}

	/// <inheritdoc cref="RunAsync" />
	public static string Run(string command)
	{
		try
		{
			var proc = new Process();
			var info = new ProcessStartInfo();
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			info.UseShellExecute = false;
			info.CreateNoWindow = true;

			info.FileName = "cmd.exe";
			info.Arguments = "/c " + command;
			proc.StartInfo = info;
			_ = proc.Start();
			proc.WaitForExit();

			var error = proc.StandardError.ReadToEnd();
			if(!string.IsNullOrWhiteSpace(error))
				throw new AggregateException(error);

			return proc.StandardOutput.ReadToEnd();
		}
		catch(Exception ex)
		{
			return ex.Message;
		}
	}
}
