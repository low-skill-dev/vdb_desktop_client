using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using System.Management.Automation;

namespace WireguardManipulator;
internal static class CommandRunner
{
	public static async Task<string> RunAsync(string command)
	{
		var proc = new Process();
		var info = new ProcessStartInfo();
		info.RedirectStandardOutput = true;
		info.RedirectStandardError = true;
		info.UseShellExecute = false;
		info.CreateNoWindow = true;

		info.FileName = "cmd.exe";
		info.Arguments = "/c "+command;
		proc.StartInfo = info;
		proc.Start();
		await proc.WaitForExitAsync();

		var error = await proc.StandardError.ReadToEndAsync();
		if (!string.IsNullOrWhiteSpace(error))
			throw new AggregateException(error);

		return await proc.StandardOutput.ReadToEndAsync();
	}
}
