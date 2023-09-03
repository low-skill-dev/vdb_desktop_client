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
	private static string AesNoncePath => Path.Join(WorkingDirectoryPath, "nonce.key");
	private static string AesTagPath => Path.Join(WorkingDirectoryPath, "tag.key");

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
		var encrypted = StringCryptography.EncryptString(key, token);

		File.WriteAllText(RefreshTokenPath, encrypted.HexAesGcmCipher);
		File.WriteAllText(AesNoncePath, encrypted.HexAesGcmNonce);
		File.WriteAllText(AesTagPath, encrypted.HexAesGcmTag);
	}

	public static string? ReadRefreshToken()
	{
		{ // This block is for old program versions migration (1.1.0 and lower)
			var oldToken = File.Exists(OldRefreshTokenPath)
				? File.ReadAllText(OldRefreshTokenPath)
				: null;

			const string oldTokenStart = @"eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.";

			if(!string.IsNullOrWhiteSpace(oldToken) && oldToken.StartsWith(oldTokenStart))
			{
				WriteRefreshToken(oldToken);
				File.Delete(oldToken);
			}
		}

		if(!File.Exists(RefreshTokenPath)) return null;
		if(!File.Exists(RandomSaltPath)) return null;
		if(!File.Exists(AesNoncePath)) return null;
		if(!File.Exists(AesTagPath)) return null;

		var cipher = File.ReadAllText(RefreshTokenPath);
		var salt = File.ReadAllText(RandomSaltPath);
		var nonce = File.ReadAllText(AesNoncePath);
		var tag = File.ReadAllText(AesTagPath);

		if(string.IsNullOrEmpty(cipher)) return null;
		if(string.IsNullOrEmpty(nonce)) return null;
		if(string.IsNullOrEmpty(salt)) return null;
		if(string.IsNullOrEmpty(tag)) return null;

		var key = GetEncryptionKey();
		var token = StringCryptography.DecryptString(key, cipher, nonce,tag);

		return token;
	}

	public static void DeleteRefreshToken()
	{
		File.Delete(RefreshTokenPath);
		File.Delete(RandomSaltPath);
	}
}
