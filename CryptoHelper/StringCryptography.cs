using System;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using static System.Text.Encoding;
using Convert = System.Convert;

namespace CryptoHelper;

/// <summary>
/// Provides aes256gcm encryption for strings up to 64k chars long.
/// </summary>
public static class StringCryptography
{
	private const int keySizeInBytes = 256 / 8;
	private const int tagSizeInBytes = 128 / 8;
	private const int nonceSizeInBytes = 96 / 8;
	private const int blockSizeInBytes = 128 / 8;
	private const int HexToBytesLengthRatio = 2 / 1;
	private const int maxAcceptedStringLength = 66 * 1024;

	#region encrypt

	public class StringEncryptionResult
	{
		public /*required*/ string HexAesGcmCipher { get; init; }
		public /*required*/ string HexAesGcmNonce { get; init; }
		public /*required*/ string HexAesGcmTag { get; init; }
	}

	/// <returns>In-stack encrypted string.</returns>
	public static StringEncryptionResult EncryptString(ReadOnlySpan<byte> keyBytes, string plainText)
	{
		if(plainText.Length > maxAcceptedStringLength)
			throw new OutOfMemoryException("Too long string was passed to the method.");

		Span<byte> key = stackalloc byte[keySizeInBytes];
		SHA256.HashData(keyBytes, key);

		var rnd = RandomNumberGenerator.Create();
#if NET8_0_OR_GREATER
		var aes = new AesGcm(key, tagSizeInBytes);
#else
		var aes = new AesGcm(key);
#endif
		Span<byte> plain = stackalloc byte[UTF8.GetByteCount(plainText)];
		Span<byte> cipher = stackalloc byte[plainText.Length];
		Span<byte> nonce = stackalloc byte[nonceSizeInBytes];
		Span<byte> tag = stackalloc byte[tagSizeInBytes];
		UTF8.GetBytes(plainText, plain);
		rnd.GetBytes(nonce);

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
		if(hexCipher.Length > maxAcceptedStringLength)
			throw new OutOfMemoryException("Too long string was passed to the method.");

		Span<byte> key = stackalloc byte[keySizeInBytes];
		Span<byte> plain = stackalloc byte[hexCipher.Length / 2];
		Span<byte> cipher = stackalloc byte[hexCipher.Length / 2];
		Span<byte> nonce = stackalloc byte[hexNonce.Length / 2];
		Span<byte> tag = stackalloc byte[hexTag.Length / 2];
		Utf8HexToBytes(hexCipher, cipher);
		Utf8HexToBytes(hexNonce, nonce);
		Utf8HexToBytes(hexTag, tag);
		SHA256.HashData(keyBytes, key);

#if NET8_0_OR_GREATER
		var aes = new AesGcm(key, tagSizeInBytes);
#else
		var aes = new AesGcm(key);
#endif

		aes.Decrypt(nonce, cipher, tag, plain);

		return UTF8.GetString(plain);
	}

	public static string DecryptString(ReadOnlySpan<byte> keyBytes, StringEncryptionResult encrypted)
		=> DecryptString(keyBytes, encrypted.HexAesGcmCipher, encrypted.HexAesGcmNonce, encrypted.HexAesGcmTag);

#endregion
}