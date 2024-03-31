using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UserInterface.Windows;

namespace UserInterface;

internal class Program
{

	//[STAThread]
	//public static void Main()
	//{
	//	if(!VerifySingleInstance()) return;

	//	var application = new AuthWindow();
	//	application.ShowDialog();
	//}


	[DllImport("kernel32")] private static extern bool AllocConsole();
	[DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	[DllImport("User32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
	private static bool VerifySingleInstance()
	{
		//ServicePointManager.ServerCertificateValidationCallback +=
		//	(_, _, _, _) => true;

		try
		{
			string procName = Process.GetCurrentProcess().ProcessName;

			Process[] processes = Process.GetProcessesByName(procName);

			if(processes.Length > 1)
			{
				foreach(Process process in processes)
				{
					const int SW_SHOWNORMAL = 1;
					ShowWindow(process.MainWindowHandle, SW_SHOWNORMAL);
					SetForegroundWindow(process.MainWindowHandle);
				}
				return false;
			}
			return true;
		}
		catch
		{
			return true; // can't verify
		}
	}
}
