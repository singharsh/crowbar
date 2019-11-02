using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace crowbar
{
    public abstract class Utils
    {
        public static void Execute(string command, string path = ".")
        {
            try
            {
                using (Process process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WorkingDirectory = path;
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        startInfo.FileName = $"cmd.exe";
                        startInfo.Arguments = $"/c {command}";
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        startInfo.FileName = $"/bin/bash";
                        startInfo.Arguments = $"-c \"{command}\"";
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        startInfo.FileName = $"/bin/bash";
                        startInfo.Arguments = $"-c \"{command}\"";
                    }
                    else
                    {
                        throw new Exception($"Unsupported Platform!");
                    }
                    process.StartInfo = startInfo;
                    process.Start();
                    process.WaitForExit();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"{e.Message}");
            }
        }

        public static string CountryRoads()
        {
            string home;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                home = Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                home = Environment.GetEnvironmentVariable("HOME");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                home = Environment.GetEnvironmentVariable("HOME");
            }
            else
            {
                throw new Exception($"Unsupported Platform!");
            }
            return home;
        }

        public static string GetAPIsURL()
        {
            // get APIs URL
            string api = ConfigurationManager.AppSettings.Get("APIsURL");
            api = api.Trim(new[] { '/' });
            return api;
        }

        public static JObject GetSavedCredentials()
        {
            if (!File.Exists(Path.Combine(CountryRoads(), "crowbar.json")))
            {
                return null;
            }
            string text = File.ReadAllText(Path.Combine(CountryRoads(), "crowbar.json"));
            JObject json = JObject.Parse(text);
            if (json.ContainsKey("username") && json.ContainsKey("password"))
            {
                return json;
            }
            return null;
        }

        public static void SaveCredentials(string username, string password)
        {
            string text = $"{{ \"username\": \"{username}\", \"password\": \"{password}\" }}";
            File.WriteAllText(Path.Combine(CountryRoads(), "crowbar.json"), text);
        }

        public static (JObject, string) GetSavedMetadata(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Directory.GetCurrentDirectory();
            }
            if (!File.Exists(Path.Combine(path, ".crowbar.json")))
            {
                if (Directory.GetParent(path) == null)
                {
                    return (null, null);
                }
                string next = Directory.GetParent(path).FullName;
                if (Directory.Exists(next))
                {
                    return GetSavedMetadata(next);
                }
                else
                {
                    return (null, null);
                }
            }
            else
            {
                string text = File.ReadAllText(Path.Combine(path, ".crowbar.json"));
                JObject json = JObject.Parse(text);
                if (json.ContainsKey("repo"))
                {
                    return (json, path);
                }
                return (null, null);
            }
        }

        public static void SaveMetadata(string repo, string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory());
            }
            string text = $"{{ \"repo\": \"{repo}\" }}";
            File.WriteAllText(Path.Combine(path, ".crowbar.json"), text);
        }

        public static string SaltAndHashPassword(string password)
        {
            // SHA 256
            HashAlgorithm algorithm = SHA256.Create();
            return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        public static int HandleError()
        {
            return 1;
        }
    }
}
