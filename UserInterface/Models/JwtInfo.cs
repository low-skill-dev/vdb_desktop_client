using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInterface.Models;
internal class JwtInfo
{
    public string AccessJwt { get; set; }
    public string RefreshJwt { get; set; }

    public DateTime AccessValidUntil { get; set; }
    public DateTime RefreshValidUntil { get; set; }
}
