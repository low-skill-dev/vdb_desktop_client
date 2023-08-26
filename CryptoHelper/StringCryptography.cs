using System.Security.Cryptography;

namespace CryptoHelper;

public static class StringCryptography
{
	/// <returns>Cipher text</returns>
	public static string EncryptString(byte[] key, string plainText)
	{
		var aes = Aes.Create();
		aes.Key = key;

		var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
		var memoryStream = new MemoryStream();
		var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
		var streamWriter = new StreamWriter((Stream)cryptoStream);
		streamWriter.Write(plainText);

		return Convert.ToHexString(memoryStream.ToArray());
	}

	/// <returns>Plain text</returns>
	public static string DecryptString(byte[] key, string cipherText)
	{
		var aes = Aes.Create();
		aes.Key = key;

		var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
		var memoryStream = new MemoryStream(Convert.FromHexString(cipherText));
		var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
		var streamReader = new StreamReader(cryptoStream);

		return streamReader.ReadToEnd();
	}
}