namespace ApiQuerier.Models;

// refactor 27-08-2023

public class UserInfo
{
	public enum AccessLevels
	{
		Unconfirmed, // email
		Free,
		Payed,
		Admin
	}

	public required int Id { get; init; }
	public required bool IsAdmin { get; init; }
	public required string Email { get; init; }
	public required bool IsEmailConfirmed { get; init; }
	public required DateTime PayedUntilUtc { get; init; }

	public AccessLevels GetAccessLevel()
	{
		if(this.IsAdmin) return AccessLevels.Admin;
		if(this.PayedUntilUtc > DateTime.UtcNow) return AccessLevels.Payed;
		if(this.IsEmailConfirmed) return AccessLevels.Free;

		return AccessLevels.Unconfirmed;
	}
}
