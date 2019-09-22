using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace crowbar
{
    public abstract class Utils
    {
        public static string GetAPIsURL()
        {
            // get APIs URL
            string api = ConfigurationManager.AppSettings.Get("APIsURL");
            api = api.Trim(new[] { '/' });
            return api;
        }

        public static JObject GetSavedCredentials()
        {
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "crowbar.json")))
            {
                return null;
            }
            string text = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "crowbar.json"));
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
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "crowbar.json"), text);
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
