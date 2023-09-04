using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using static System.Text.Encoding;
using Convert = System.Convert;


namespace CryptoHelper;


public static class StringCryptocraphyNoStackAlloc
{
	private const int keySizeInBytes = 256 / 8;
	private const int tagSizeInBytes = 128 / 8;
	private const int blockSizeBytes = 128 / 8;
	private const int nonceSizeInBytes = 96 / 8;
	private const int HexToBytesLengthRatio = 2 / 1;

	#region encrypt

	public class StringEncryptionResult
	{
		public required string HexAesGcmCipher { get; init; }
		public required string HexAesGcmNonce { get; init; }
		public required string HexAesGcmTag { get; init; }
	}

	/// <returns>In-stack encrypted string.</returns>
	public static StringEncryptionResult EncryptString(ReadOnlySpan<byte> keyBytes, string plainText)
	{
		byte[] key = SHA256.HashData(keyBytes);

		var aes = new AesGcm(key, tagSizeInBytes);

		byte[] plain = UTF8.GetBytes(plainText);
		byte[] cipher = new byte[plainText.Length];
		byte[] nonce = RandomNumberGenerator.GetBytes(nonceSizeInBytes);
		byte[] tag = new byte[tagSizeInBytes];

		aes.Encrypt(nonce, plain, cipher, tag);

		return new StringEncryptionResult
		{
			HexAesGcmCipher = Convert.ToHexString(cipher),
			HexAesGcmNonce = Convert.ToHexString(nonce),
			HexAesGcmTag = Convert.ToHexString(tag),
		};
	}

	#endregion

	#region decrypt

	private static void Utf8HexToBytes(ReadOnlySpan<char> hex, Span<byte> buf)
	{
		for(int i = 0; i < hex.Length; i += 2)
			buf[i / 2] = Convert.ToByte((((0 << 4)
				| (hex[i] - (hex[i] <= '9' ? '0' : ('A' - 10))) << 4)
				| (hex[i + 1] - (hex[i + 1] <= '9' ? '0' : ('A' - 10)))));
	}

	public static string DecryptString(ReadOnlySpan<byte> keyBytes, string hexCipher, string hexNonce, string hexTag)
	{
		Span<byte> key = SHA256.HashData(keyBytes);
		Span<byte> plain = Convert.FromHexString(hexCipher);
		Span<byte> cipher = Convert.FromHexString(hexCipher);
		Span<byte> nonce = Convert.FromHexString(hexNonce);
		Span<byte> tag = Convert.FromHexString(hexTag);

		var aes = new AesGcm(key, tagSizeInBytes);

		aes.Decrypt(nonce, cipher, tag, plain);

		return UTF8.GetString(plain);
	}

	public static string DecryptString(ReadOnlySpan<byte> keyBytes, StringEncryptionResult encrypted)
		=> DecryptString(keyBytes, encrypted.HexAesGcmCipher, encrypted.HexAesGcmNonce, encrypted.HexAesGcmTag);

	#endregion
}
