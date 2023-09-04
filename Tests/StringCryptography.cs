using CryptoHelper;
using DeviceId;
using System.Security.Cryptography;
using System.Text;
using FilesHelper;

namespace Tests;

public class StringCryptography
{
	private const string testToken = @"eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9." +
		@"eyJJZCI6MSwiSXNBZG1pbiI6dHJ1ZSwiRW1haWwiOiI2NWJpc28yQGdtYWlsLmNvbS" +
		@"IsIklzRW1haWxDb25maXJtZWQiOmZhbHNlLCJQYXllZFVudGlsVXRjIjoiMDAwMS0w" +
		@"MS0wMVQwMDowMDowMC4wMDAwMDAwIiwibmJmIjoxNjkzNjAxMTg4LCJleHAiOjE2OT" +
		@"M2MDE0ODksImlhdCI6MTY5MzYwMTE4OX0.xDdJaLIeIZAtvIdgsTWqgZffyN3hk1Ra" +
		@"J6rPJKn9kIDaRKk5x7pkugiYPBVDh2DrWFdYziXelRNYloChVYifDA";

	[Fact]
	public void CanEncryptAndDecrypt()
	{
		var testTokenFile = Path.GetTempFileName();
		var testKeyFile = Path.GetTempFileName();

		var key = SHA512.HashData(Encoding.UTF8.GetBytes(
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
				.AddFileToken(testKeyFile)
				.ToString()
		));

		var encrypted = CryptoHelper.StringCryptography.EncryptString(key, testToken);

		var dectypted = CryptoHelper.StringCryptography.DecryptString(key, encrypted);

		Assert.Equal(testToken, dectypted);
	}

	[Fact]
	public void CanEncryptAndDecryptFromFile()
	{
		TokenFilesHelper.WriteRefreshToken(testToken);

		var read = TokenFilesHelper.ReadRefreshToken();

		Assert.Equal(testToken, read);
	}

	[Fact]
	public void CanEncryptAndDecrypRandom()
	{
		var rnd = RandomNumberGenerator.Create();

		var len = RandomNumberGenerator.GetInt32(128,2048);

		var bytes = new object[len].Select(x=> Convert.ToByte(RandomNumberGenerator.GetInt32(48,91))).ToArray();

		var str = Encoding.UTF8.GetString(Encoding.Convert(Encoding.UTF8,Encoding.ASCII, bytes));

		var testToken = str;

		var testTokenFile = Path.GetTempFileName();
		var testKeyFile = Path.GetTempFileName();

		var key = SHA512.HashData(Encoding.UTF8.GetBytes(
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
				.AddFileToken(testKeyFile)
				.ToString()
		));

		var encrypted = CryptoHelper.StringCryptography.EncryptString(key, testToken);

		var dectypted = CryptoHelper.StringCryptography.DecryptString(key, encrypted);

		Assert.Equal(testToken, dectypted);
	}

}