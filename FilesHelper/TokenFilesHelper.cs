using DeviceId;
using System.Text;
using System.Security.Cryptography;
using CryptoHelper;
using System.Reflection;

namespace FilesHelper;

public static class TokenFilesHelper
{
	private static string WorkingDirectoryPath => Environment.CurrentDirectory;
	private static string OldRefreshTokenPath => Path.Join(WorkingDirectoryPath, @"refresh.token");
	private static string RefreshTokenPath => Path.Join(WorkingDirectoryPath, @"refresh.key");
	private static string RandomSaltPath => Path.Join(WorkingDirectoryPath, @"salt.key");

	private static byte[] GetEncryptionKey()
	{
		return SHA512.HashData(Encoding.UTF8.GetBytes(
			new DeviceIdBuilder()
				.OnWindows(windows => windows
					.AddMotherboardSerialNumber()
					.AddSystemDriveSerialNumber())
				.OnLinux(linux => linux
					.AddMotherboardSerialNumber()
					.AddSystemDriveSerialNumber())
				.OnMac(mac => mac
					.AddPlatformSerialNumber()
					.AddSystemDriveSerialNumber())
				.AddFileToken(RandomSaltPath)
				.ToString()
		));
	}

	public static void WriteRefreshToken(string token)
	{
		DeleteRefreshToken();

		var key = GetEncryptionKey();
		var encryptedToken = StringCryptography.EncryptString(key, token);

		File.WriteAllText(RefreshTokenPath, encryptedToken);
	}

	public static string? ReadRefreshToken()
	{
		// This block is for old program versions migration (1.1.0 and lower)
		{
			var oldToken = File.Exists(OldRefreshTokenPath)
				? File.ReadAllText(OldRefreshTokenPath)
				: null;

			const string oldTokenStart = @"eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.";

			if(!string.IsNullOrWhiteSpace(oldToken) && oldToken.StartsWith(oldTokenStart)) {
				WriteRefreshToken(oldToken);
				File.Delete(oldToken);
			}
		}

		if(!File.Exists(RefreshTokenPath)) return null;
		if(!File.Exists(RandomSaltPath)) return null;

		var encryptedToken = File.ReadAllText(RefreshTokenPath);
		var encryptionSalt = File.ReadAllText(RandomSaltPath);

		if(string.IsNullOrEmpty(encryptedToken)) return null;
		if(string.IsNullOrEmpty(encryptionSalt)) return null;

		var key = GetEncryptionKey();
		var token = StringCryptography.DecryptString(key, encryptedToken);

		return token;
	}

	public static void DeleteRefreshToken()
	{
		File.Delete(RefreshTokenPath);
		File.Delete(RandomSaltPath);
	}
}
