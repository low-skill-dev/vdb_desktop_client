using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesHelper;
public static class RuntimeInfoHelper
{
	private static string WorkingDirectoryPath => Environment.CurrentDirectory;
	private static string LastConnectedNode => Path.Join(WorkingDirectoryPath, @"last_connected_node.ini");

	public static async Task WriteLastConnectedNode(int nodeId)
	{
		await File.WriteAllTextAsync(LastConnectedNode, nodeId.ToString());
	}
	public static async Task<int> ReadLastConnectedNode()
	{
		try
		{
			return int.Parse(await File.ReadAllTextAsync(LastConnectedNode));
		}
		catch
		{
			return 0;
		}
	}
}
