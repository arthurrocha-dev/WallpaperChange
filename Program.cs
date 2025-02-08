    using System;
using System.Diagnostics;
using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Microsoft.Win32;

    class Program
    {
        private const string imageUrl = "https://arthurrocha.dev/wallpaper/wallpaper.jpeg";
        private static readonly string installDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WallpaperChanger");
        private static readonly string imagePath = Path.Combine(installDir, "wallpaper.jpg");
        private static readonly string logFilePath = Path.Combine(installDir, "log.txt");

        static async Task Main()
        {
            try
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now}: Iniciando WallpaperChanger...\n");

                HideConsole();
                Log("Iniciando WallpaperChanger...");

                AddToStartup();

            while (true)
            {
                try
                {
                    bool hasInternet = await CheckInternetConnectionAsync();
                    bool imageExists = File.Exists(imagePath);
                    bool wallpaperSet = IsWallpaperSet();

                    if (!imageExists || !wallpaperSet || hasInternet)
                    {
                        Log("Verificando necessidade de atualização...");

                        if (hasInternet)
                        {
                            Log("Conexão com a internet disponível. Verificando atualização da imagem...");

                            string tempImagePath = Path.Combine(installDir, "temp_wallpaper.jpg");

                            if (await DownloadImage(tempImagePath))
                            {
                                if (!File.Exists(imagePath) || !ImagesAreEqual(tempImagePath, imagePath))
                                {
                                    Log("Nova imagem detectada. Atualizando wallpaper...");
                                    File.Copy(tempImagePath, imagePath, true);
                                    SetWallpaper(imagePath);
                                }
                                else
                                {
                                    Log("A imagem já está atualizada.");
                                }

                                File.Delete(tempImagePath); // Remove a imagem temporária
                            }
                        }
                        else
                        {
                            Log("Sem conexão. Tentando novamente em 30 segundos...");
                        }
                    }
                    else
                    {
                        Log("Wallpaper já está definido corretamente.");
                    }
                }
                catch (Exception ex)
                {
                    Log("Erro no loop de verificação: " + ex.Message);
                }

                await Task.Delay(30000);
            }

        }
        catch (Exception ex)
            {
                File.AppendAllText(logFilePath, $"{DateTime.Now}: Erro crítico - {ex.Message}\n");
                Log("Erro crítico: " + ex.Message);
            }
        }

    static void AddToStartup()
    {
        try
        {
            string vbsPath = Path.Combine(installDir, "start.vbs");
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null)
            {
                Log("Não foi possível abrir a chave de inicialização do Registro.");
                return;
            }

            key.SetValue("WallpaperChanger", $"\"{vbsPath}\"");
            Log("Programa adicionado à inicialização do Windows sem terminal.");
        }
        catch (Exception ex)
        {
            Log("Erro ao adicionar o programa à inicialização: " + ex.Message);
        }
    }


    static async Task<bool> CheckInternetConnectionAsync()
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await client.GetAsync("http://www.google.com");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
        }
    }

    static async Task<bool> DownloadImage(string targetPath)
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();

            await using var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fs);

            Log("Imagem baixada com sucesso.");
            return true;
        }
        catch (Exception ex)
        {
            Log("Erro ao baixar a imagem: " + ex.Message);
            return false;
        }
    }

    static bool ImagesAreEqual(string path1, string path2)
    {
        try
        {
            using var fs1 = new FileStream(path1, FileMode.Open, FileAccess.Read);
            using var fs2 = new FileStream(path2, FileMode.Open, FileAccess.Read);

            if (fs1.Length != fs2.Length)
                return false;

            int byte1, byte2;
            do
            {
                byte1 = fs1.ReadByte();
                byte2 = fs2.ReadByte();
                if (byte1 != byte2)
                    return false; 
            } while (byte1 != -1);

            return true;
        }
        catch (Exception ex)
        {
            Log("Erro ao comparar imagens: " + ex.Message);
            return false;
        }
    }


    static void SetWallpaper(string path)
        {
            try
            {
                SystemParametersInfo(0x0014, 0, path, 0x01 | 0x02);
                Log("Wallpaper atualizado.");
            }
            catch (Exception ex)
            {
                Log("Erro ao definir wallpaper: " + ex.Message);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        static bool IsWallpaperSet()
        {
            try
            {
                using RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", false);
                if (key != null)
                {
                    string currentWallpaper = key.GetValue("Wallpaper") as string;
                    return currentWallpaper != null && currentWallpaper.Equals(imagePath, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception ex)
            {
                Log("Erro ao verificar o wallpaper: " + ex.Message);
            }
            return false;
        }

        static void HideConsole()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0);
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    static void Log(string message)
    {
        try { 
            if (!Directory.Exists(installDir))
            {
                Directory.CreateDirectory(installDir);
            }
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao escrever no log: {ex.Message}");
        }
    }

}
