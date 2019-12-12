using crowbar;
using Newtonsoft.Json.Linq;
using System.IO;
using Xunit;

namespace XUnitTests
{
    public class UnitTests
    {
        [Fact]
        public void TestSaveCredentials()
        {
            if (File.Exists(Path.Combine(Utils.CountryRoads(), "crowbar.json")))
            {
                File.Delete(Path.Combine(Utils.CountryRoads(), "crowbar.json"));
            }
            Utils.SaveCredentials("test_user", "test_pass");
            string content = File.ReadAllText(Path.Combine(Utils.CountryRoads(), "crowbar.json"));
            JObject json = JObject.Parse(content);
            string username = (string)json["username"];
            string password = (string)json["password"];
            Assert.Equal("test_user", username);
            Assert.Equal("test_pass", password);
            if (File.Exists(Path.Combine(Utils.CountryRoads(), "crowbar.json")))
            {
                File.Delete(Path.Combine(Utils.CountryRoads(), "crowbar.json"));
            }
        }

        [Fact]
        public void TestGetCredentails()
        {
            if (File.Exists(Path.Combine(Utils.CountryRoads(), "crowbar.json")))
            {
                File.Delete(Path.Combine(Utils.CountryRoads(), "crowbar.json"));
            }
            Utils.SaveCredentials("test_user", "test_pass");
            JObject json = Utils.GetSavedCredentials();
            string username = (string)json["username"];
            string password = (string)json["password"];
            Assert.Equal("test_user", username);
            Assert.Equal("test_pass", password);
            if (File.Exists(Path.Combine(Utils.CountryRoads(), "crowbar.json")))
            {
                File.Delete(Path.Combine(Utils.CountryRoads(), "crowbar.json"));
            }
        }

        [Fact]
        public void TestSaveMetadata()
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".crowbar.json")))
            {
                File.Delete(Path.Combine(Directory.GetCurrentDirectory(), ".crowbar.json"));
            }
            Utils.SaveMetadata("test_repo");
            string content = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ".crowbar.json"));
            JObject json = JObject.Parse(content);
            string repo = (string)json["repo"];
            Assert.Equal("test_repo", repo);
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".crowbar.json")))
            {
                File.Delete(Path.Combine(Directory.GetCurrentDirectory(), ".crowbar.json"));
            }
        }

        [Fact]
        public void TestGetMetadata()
        {
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".crowbar.json")))
            {
                File.Delete(Path.Combine(Directory.GetCurrentDirectory(), ".crowbar.json"));
            }
            Utils.SaveMetadata("test_repo", Directory.GetCurrentDirectory().ToString());
            (JObject json, string path) = Utils.GetSavedMetadata();
            string repo = (string)json["repo"];
            Assert.Equal("test_repo", repo);
            Assert.Equal(Directory.GetCurrentDirectory().ToString(), path);
            if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".crowbar.json")))
            {
                File.Delete(Path.Combine(Directory.GetCurrentDirectory(), ".crowbar.json"));
            }
        }

        [Fact]
        public void TestGetMetadataRecursive()
        {
            if (File.Exists(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), ".crowbar.json")))
            {
                File.Delete(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), ".crowbar.json"));
            }
            Utils.SaveMetadata("test_repo", Directory.GetParent(Directory.GetCurrentDirectory()).ToString());
            (JObject json, string path) = Utils.GetSavedMetadata();
            string repo = (string)json["repo"];
            Assert.Equal("test_repo", repo);
            string temp = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
            Assert.Equal(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), path);
            if (File.Exists(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), ".crowbar.json")))
            {
                File.Delete(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).ToString(), ".crowbar.json"));
            }
        }
    }
}
