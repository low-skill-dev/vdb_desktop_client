using ApiQuerier.Helpers;

namespace WireguardManipulator;

/* Сначала данный класс должен проверить, существует ли файл приватного ключа.
 * Если да - загрузить ключ, нет - сгенерировать. Далее на основании полученного
 * ответа от сервера генерируется конфиг файл. Файл ключа является долгоживущим,
 * файл конфига может генерировать сколь угодно часто. Метод WriteConfig должен
 * быть обязательно вызван до установления туннеля.
 * 
 */
public class TunnelManager
{
	public static string ApplicationPath => Path.Join(Constants.WorkingDirectory);
	public static string PersonalPath => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Vdb VPN");
	public static string ConfigPath => Path.Join(PersonalPath, @"vdb0.conf");
	public static string KeyPath => Path.Join(ApplicationPath, @"vdb0.key");

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
			var pk = File.ReadAllText(KeyPath).Trim("\r\n\t,; ".ToCharArray());
			var strictBytesCount = 256 / 8;
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
		File.WriteAllText(ConfigPath, ConfigGenerator.GenerateConfig(this.Keys.Private, mainServerResponse));
	}
	public void DeleteAllFiles()
	{
		File.Delete(ConfigPath);
		File.Delete(KeyPath);
	}
	public void DeleteConfigFile()
	{
		try
		{
			File.Delete(ConfigPath);
		}
		catch { }
	}

	public async Task<bool> EstablishTunnel()
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
