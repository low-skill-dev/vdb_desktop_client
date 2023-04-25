using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace main_server_api.Models.UserApi.Website.Common;

public class UserInfo
{
	public int Id { get; init; }
	public bool IsAdmin { get; init; }
	public string Email { get; init; }
	public bool IsEmailConfirmed { get; init; }
	public DateTime PayedUntilUtc { get; init; }
}
