namespace ApiQuerier.Models;

public class UserInfo
{
	public required int Id { get; init; }
	public required bool IsAdmin { get; init; }
	public required string Email { get; init; }
	public required bool IsEmailConfirmed { get; init; }
	public required DateTime PayedUntilUtc { get; init; }


	public enum AccessLevels
	{
		Unconfirmed, // email
		Free,
		Payed,
		Admin
	}

	public AccessLevels GetAccessLevel()
	{
		AccessLevels result = AccessLevels.Free;

		if(this.IsEmailConfirmed)
			result = AccessLevels.Free;

		if(this.PayedUntilUtc > DateTime.UtcNow)
			result = AccessLevels.Payed;

		if(this.IsAdmin)
			result = AccessLevels.Admin;

		return result;
	}
}
