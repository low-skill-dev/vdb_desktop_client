namespace ServerQuerier.Models.Auth;

public class UserInfo
{
	public enum AccessLevels
	{
		Unconfirmed, // email
		Free,
		Payed,
		Admin
	}

	public AccessLevels GetAccessLevel()
	{
		if(this.IsAdmin) return AccessLevels.Admin;
		if(this.PayedUntilUtc > DateTime.UtcNow) return AccessLevels.Payed;
		if(this.IsEmailConfirmed) return AccessLevels.Free;

		return AccessLevels.Unconfirmed;
	}
	public string GetAccessLevelString()
	{
		switch(this.GetAccessLevel()) {
			case AccessLevels.Admin: return nameof(AccessLevels.Admin);
			case AccessLevels.Payed: return nameof(AccessLevels.Payed) + " User";
			case AccessLevels.Free: return nameof(AccessLevels.Free) + " User";
			case AccessLevels.Unconfirmed: return nameof(AccessLevels.Free) + " User";
			default:
#if RELEASE
				return "unknown";
#else
				throw new NotImplementedException(nameof(AccessLevels));
#endif
		}
	}


	public int Id { get; init; }
	public bool IsAdmin { get; init; }
	public string Email { get; init; }
	public bool IsEmailConfirmed { get; init; }
	public DateTime PayedUntilUtc { get; init; }
}
