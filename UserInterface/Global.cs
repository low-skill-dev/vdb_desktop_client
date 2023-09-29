using ApiModels.Node;
using ApiQuerier.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInterface;
internal static class Global
{
	public static string CurrentWindowNameof { get; set; }
	public static UserInfo? CurrentUser { get; set; }
	public static PublicNodeInfo? CurrentConnection { get; set; }
}
