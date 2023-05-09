using main_server_api.Models.UserApi.Application.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Environment;

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
	/* IF YOU CHANGE THIS PATH IN THE NEXT RELEASE PROVIDE THE BACKWARD COPABILITY
	 */

	public static string ApplicationPath => Path.Join(GetFolderPath(SpecialFolder.Personal), @"Vdb");
	public static string ConfigPath => Path.Join(ApplicationPath, @"vdb0.conf");
	public static string KeyPath => Path.Join(ApplicationPath, @"vdb0.key");

	private KeyPair Keys;
	public string PublicKey => Keys.Public;

	public TunnelManager()
	{
		Keys = CreatePair();
	}

	public static KeyPair CreatePair()
	{
		KeyPair keys;
		try {
			var pk = File.ReadAllText(KeyPath).Trim("\r\n\t,; ".ToCharArray());
			var strictBytesCount = 256 / 8;
			if(string.IsNullOrWhiteSpace(pk) ||
				!Convert.TryFromBase64String(pk, new byte[pk.Length], out var bytesCount) ||
				bytesCount != strictBytesCount) throw new FormatException();

			keys = new(pk);
		} catch {
			keys = new KeyPair();
			Directory.CreateDirectory(ApplicationPath);
			File.WriteAllText(KeyPath, keys.Private);
		}

		return keys;
	}

	public void WriteConfig(ConnectDeviceResponse mainServerResponse)
	{
		File.WriteAllText(ConfigPath, ConfigGenerator.GenerateConfig(this.Keys.Private, mainServerResponse));
	}

	public async Task<bool> EstablishTunnel()
	{
		if(!File.Exists(ConfigPath)) throw new FileNotFoundException($"Configuration file was not found at {ConfigPath}.");

		var wgResponse = await CommandRunner.RunAsync($"wireguard /installtunnelservice {ConfigPath}");
		return string.IsNullOrWhiteSpace(wgResponse);
	}

	public async Task<bool> DeleteTunnel()
	{
		return string.IsNullOrWhiteSpace(await CommandRunner.RunAsync(
			$"wireguard /uninstalltunnelservice {Path.GetFileNameWithoutExtension(ConfigPath)}"));
	}
}
