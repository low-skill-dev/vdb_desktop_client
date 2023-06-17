using System.ComponentModel.DataAnnotations;

namespace ServerQuerier.Models.Auth;

public class RefreshJwtRequest
{
	[Required]
	public string RefreshToken { get; set; }
}
