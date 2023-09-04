﻿using BenchmarkDotNet.Attributes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;
using System.Runtime.Intrinsics.Arm;
using CryptoHelper;
using System.Text.Unicode;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System.Text;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using Perfolizer.Horology;

namespace Benchmarks;

public class Program
{
	public static void Main()
	{
		BenchmarkRunner.Run<Aes256gcm_StackAllocVsHeapAlloc1KByteString>();
		BenchmarkRunner.Run<Aes256gcm_StackAllocVsHeapAlloc4KByteString>();
		BenchmarkRunner.Run<Aes256gcm_StackAllocVsHeapAlloc16KByteString>();
		BenchmarkRunner.Run<Aes256gcm_StackAllocVsHeapAlloc64KByteString>();
		BenchmarkRunner.Run<Aes256gcm_StackAllocVsHeapAlloc256KByteString>();

		Console.ReadLine();
	}
}



[MemoryDiagnoser]
public class Aes256gcm_StackAllocVsHeapAlloc1KByteString
{
	private static int stringSizeBytes = 1024*1;

	private byte[] key;
	private string toEncrypt;

	[GlobalSetup]
	public void Setup()
	{
		key = RandomNumberGenerator.GetBytes(256 / 8);
		toEncrypt = Encoding.UTF8.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII, 
			new object[stringSizeBytes/2].Select(x => Convert.ToByte(RandomNumberGenerator.GetInt32(48, 91))).ToArray()));
	}

	[Benchmark]
	public void AesStackAlloc()
	{
		StringCryptography.DecryptString(key,
			StringCryptography.EncryptString(
				key, toEncrypt
			));
	}

	[Benchmark]
	public void AesHeapAlloc()
	{
		StringCryptocraphyNoStackAlloc.DecryptString(key,
			StringCryptocraphyNoStackAlloc.EncryptString(
				key, toEncrypt
			));
	}
}

[MemoryDiagnoser]
public class Aes256gcm_StackAllocVsHeapAlloc4KByteString
{
	private static int stringSizeBytes = 1024*4;

	private byte[] key;
	private string toEncrypt;

	[GlobalSetup]
	public void Setup()
	{
		key = RandomNumberGenerator.GetBytes(256 / 8);
		toEncrypt = Encoding.UTF8.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII,
			new object[stringSizeBytes / 2].Select(x => Convert.ToByte(RandomNumberGenerator.GetInt32(48, 91))).ToArray()));
	}

	[Benchmark]
	public void AesStackAlloc()
	{
		StringCryptography.DecryptString(key,
			StringCryptography.EncryptString(
				key, toEncrypt
			));
	}

	[Benchmark]
	public void AesHeapAlloc()
	{
		StringCryptocraphyNoStackAlloc.DecryptString(key,
			StringCryptocraphyNoStackAlloc.EncryptString(
				key, toEncrypt
			));
	}
}

[MemoryDiagnoser]
public class Aes256gcm_StackAllocVsHeapAlloc16KByteString
{
	private static int stringSizeBytes = 1024*16;

	private byte[] key;
	private string toEncrypt;

	[GlobalSetup]
	public void Setup()
	{
		key = RandomNumberGenerator.GetBytes(256 / 8);
		toEncrypt = Encoding.UTF8.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII,
			new object[stringSizeBytes / 2].Select(x => Convert.ToByte(RandomNumberGenerator.GetInt32(48, 91))).ToArray()));
	}

	[Benchmark]
	public void AesStackAlloc()
	{
		StringCryptography.DecryptString(key,
			StringCryptography.EncryptString(
				key, toEncrypt
			));
	}

	[Benchmark]
	public void AesHeapAlloc()
	{
		StringCryptocraphyNoStackAlloc.DecryptString(key,
			StringCryptocraphyNoStackAlloc.EncryptString(
				key, toEncrypt
			));
	}
}

[MemoryDiagnoser]
public class Aes256gcm_StackAllocVsHeapAlloc64KByteString
{
	private static int stringSizeBytes = 1024*64;

	private byte[] key;
	private string toEncrypt;

	[GlobalSetup]
	public void Setup()
	{
		key = RandomNumberGenerator.GetBytes(256 / 8);
		toEncrypt = Encoding.UTF8.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII,
			new object[stringSizeBytes / 2].Select(x => Convert.ToByte(RandomNumberGenerator.GetInt32(48, 91))).ToArray()));
	}

	[Benchmark]
	public void AesStackAlloc()
	{
		StringCryptography.DecryptString(key,
			StringCryptography.EncryptString(
				key, toEncrypt
			));
	}

	[Benchmark]
	public void AesHeapAlloc()
	{
		StringCryptocraphyNoStackAlloc.DecryptString(key,
			StringCryptocraphyNoStackAlloc.EncryptString(
				key, toEncrypt
			));
	}
}

[MemoryDiagnoser]
public class Aes256gcm_StackAllocVsHeapAlloc256KByteString
{
	private static int stringSizeBytes = 1024*256;

	private byte[] key;
	private string toEncrypt;

	[GlobalSetup]
	public void Setup()
	{
		key = RandomNumberGenerator.GetBytes(256 / 8);
		toEncrypt = Encoding.UTF8.GetString(Encoding.Convert(Encoding.UTF8, Encoding.ASCII,
			new object[stringSizeBytes / 2].Select(x => Convert.ToByte(RandomNumberGenerator.GetInt32(48, 91))).ToArray()));
	}

	[Benchmark]
	public void AesStackAlloc()
	{
		StringCryptography.DecryptString(key,
			StringCryptography.EncryptString(
				key, toEncrypt
			));
	}

	[Benchmark]
	public void AesHeapAlloc()
	{
		StringCryptocraphyNoStackAlloc.DecryptString(key,
			StringCryptocraphyNoStackAlloc.EncryptString(
				key, toEncrypt
			));
	}
}