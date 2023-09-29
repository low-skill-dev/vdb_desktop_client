using ApiModels.Device;
//using ApiQuerier.Helpers;
//using static ApiQuerier.Helpers.Constants;

namespace WireguardManipulator;

/* Сначала данный класс должен проверить, существует ли файл приватного ключа.
 * Если да - загрузить ключ, нет - сгенерировать. Далее на основании полученного
 * ответа от сервера генерируется конфиг файл. Файл ключа является долгоживущим,
 * файл конфига может генерироваться сколь угодно часто. Метод WriteConfig должен
 * быть обязательно вызван до установления туннеля.
 */
public sealed class TunnelManager
{
	private static string ApplicationPath => Environment.CurrentDirectory;
	//private static string PersonalPath => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
	private static string PersonalPath => Path.Join(
		Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Vdb VPN");
	private static string ConfigPath => Path.Join(PersonalPath, @"vdb0.conf");
	private static string KeyPath => Path.Join(ApplicationPath, @"vdb0.key");

	private readonly KeyPair Keys;
	public string PublicKey => this.Keys.Public;

	public TunnelManager()
	{
		this.Keys = CreatePair();
	}

	private static KeyPair CreatePair()
	{
		KeyPair keys;
		try
		{
			const int strictBytesCount = 256 / 8;

			var pk = File.ReadAllText(KeyPath).Trim("\r\n\t,; ".ToCharArray());
			if(string.IsNullOrWhiteSpace(pk) ||
				!Convert.TryFromBase64String(pk, new byte[pk.Length], out var bytesCount) ||
				bytesCount != strictBytesCount) throw new FormatException();

			keys = new(pk);
		}
		catch
		{
			keys = new KeyPair();
			File.WriteAllText(KeyPath, keys.Private);
		}

		return keys;
	}

	public void WriteConfig(ConnectDeviceResponse mainServerResponse)
	{
		Directory.CreateDirectory(PersonalPath);
		File.WriteAllText(ConfigPath, ConfigGenerator
			.GenerateConfig(this.Keys.Private, mainServerResponse));
	}
	public void DeleteAllFiles()
	{
		try
		{
			File.Delete(ConfigPath);
			File.Delete(KeyPath);
			Directory.Delete(PersonalPath);
		}
		catch { }
	}
	public void DeleteConfigFile()
	{
		try
		{
			File.Delete(ConfigPath);
			Directory.Delete(PersonalPath);
		}
		catch { }
	}

	public async Task<bool> EstablishTunnel(bool disableDeleting = false)
	{
		if(!File.Exists(ConfigPath)) throw new FileNotFoundException($"Configuration file was not found at {ConfigPath}.");

		var wgResponse = await CommandRunner.RunAsync($"wireguard /installtunnelservice \"{ConfigPath}\"");

		return string.IsNullOrWhiteSpace(wgResponse);
	}

	public async Task<bool> DeleteTunnel()
	{
		return string.IsNullOrWhiteSpace(await CommandRunner.RunAsync(
			$"wireguard /uninstalltunnelservice \"{Path.GetFileNameWithoutExtension(ConfigPath)}\""));
	}
}
