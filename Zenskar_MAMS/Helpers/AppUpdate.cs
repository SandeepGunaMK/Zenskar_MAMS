using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Zenskar_MAMS.Helpers
{
    internal class AppUpdate
    {
        public static async Task CheckForUpdateAsync()
        {
            string installPath = AppDomain.CurrentDomain.BaseDirectory;
            string localVersionPath = Path.Combine(installPath, "version.txt");                               

            try
            {
                string localVersion = File.ReadAllText(localVersionPath).Trim();
                string remoteVersion = await GetRemoteVersionAsync();

                if (remoteVersion != localVersion)
                {
                    var res = MessageBox.Show(
                        $"Update Available!\n\nCurrent: {localVersion}\nLatest: {remoteVersion}\nClick OK to Update" , "Update App?", MessageBoxButton.OKCancel);
                    if (res == MessageBoxResult.OK) { UpdateApp(); }
                }
                else{ MessageBox.Show($"You already got the Latest Version:\n  {localVersion}", "Up To Date"); }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking update:\n" + ex.Message, "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static async Task<string> GetRemoteVersionAsync()
        {
            //string url = "https://raw.githubusercontent.com/SandeepGunaMK/ZenskarApp/main/version.txt";
            string url = "https://raw.githubusercontent.com/SandeepGunaMK/ZenskarApp/main/version.txt?t={DateTime.UtcNow.Ticks}";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "ZenskarApp");

                string version = await client.GetStringAsync(url);
                return version.Trim();
            }
        }

        public static void UpdateApp()
        {
            string installPath = AppDomain.CurrentDomain.BaseDirectory;
            string repoUrl = "https://github.com/SandeepGunaMK/ZenskarApp.git";
            string branch = "main";
            //string token = ConfigurationManager.AppSettings["GitHubToken"] ?? throw new Exception("GitHub token not found in app settings.");
            string token = "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";

            string updaterPath = Path.Combine(installPath, "Updater.ps1");

            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
            };
            psi.ArgumentList.Add("-ExecutionPolicy");
            psi.ArgumentList.Add("Bypass");
            psi.ArgumentList.Add("-File");
            psi.ArgumentList.Add(updaterPath);
            psi.ArgumentList.Add(installPath);
            psi.ArgumentList.Add(repoUrl);
            psi.ArgumentList.Add(branch);
            psi.ArgumentList.Add(token);

            Process.Start(psi);

            Application.Current.Shutdown();
        }

    }
}
