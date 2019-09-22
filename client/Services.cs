using System;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json.Linq;

namespace crowbar
{
    public abstract class Services
    {
        public static bool AuthenticateUser(string username, string password)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent($"");
                    HttpResponseMessage response = request.PostAsync($"{Utils.GetAPIsURL()}/authenticate?username={username}&password={password}", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                    
                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static JObject GetUser(string username)
        {
            username = HttpUtility.UrlEncode(username);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = request.GetAsync($"{Utils.GetAPIsURL()}/users?username={username}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string body = response.Content.ReadAsStringAsync().Result;
                        JObject json = JObject.Parse(body);
                        return json;
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static bool CreateUser(string username, string password, string email)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password);
            email = HttpUtility.UrlEncode(email);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent($"");
                    HttpResponseMessage response = request.PostAsync($"{Utils.GetAPIsURL()}/users?username={username}&password={password}&email={email}", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static bool UpdatePassword(string username, string password, string updatedpassword)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password);
            updatedpassword = HttpUtility.UrlEncode(updatedpassword);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent($"");
                    HttpResponseMessage response = request.PutAsync($"{Utils.GetAPIsURL()}/users?username={username}&password={password}&updatedpassword={updatedpassword}", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static bool UpdateEmail(string username, string password, string email)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password);
            email = HttpUtility.UrlEncode(email);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent($"");
                    HttpResponseMessage response = request.PutAsync($"{Utils.GetAPIsURL()}/users?username={username}&password={password}&email={email}", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static JObject GetRepo(string username, string password, string repo)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password);
            repo = HttpUtility.UrlEncode(repo);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = request.GetAsync($"{Utils.GetAPIsURL()}/repos?username={username}&password={password}&repo={repo}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string body = response.Content.ReadAsStringAsync().Result;
                        JObject json = JObject.Parse(body);
                        return json;
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static bool CreateRepo(string username, string repo)
        {
            username = HttpUtility.UrlEncode(username);
            repo = HttpUtility.UrlEncode(repo);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent($"");
                    HttpResponseMessage response = request.PostAsync($"{Utils.GetAPIsURL()}/repos?username={username}&repo={repo}", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static bool AddOwner(string username, string password, string repo, string owner)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password);
            repo = HttpUtility.UrlEncode(repo);
            owner = HttpUtility.UrlEncode(owner);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent($"");
                    HttpResponseMessage response = request.PutAsync($"{Utils.GetAPIsURL()}/repos?username={username}&password={password}&repo={repo}&owner={owner}", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static bool AddCollaborator(string username, string password, string repo, string collaborator)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password);
            repo = HttpUtility.UrlEncode(repo);
            collaborator = HttpUtility.UrlEncode(collaborator);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    StringContent content = new StringContent($"");
                    HttpResponseMessage response = request.PutAsync($"{Utils.GetAPIsURL()}/repos?username={username}&password={password}&repo={repo}&collaborator={collaborator}", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static bool RemoveOwner(string username, string password, string repo, string owner)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password);
            repo = HttpUtility.UrlEncode(repo);
            owner = HttpUtility.UrlEncode(owner);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = request.DeleteAsync($"{Utils.GetAPIsURL()}/repos?username={username}&password={password}&repo={repo}&owner={owner}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }

        public static bool RemoveCollaborator(string username, string password, string repo, string collaborator)
        {
            username = HttpUtility.UrlEncode(username);
            password = HttpUtility.UrlEncode(password);
            repo = HttpUtility.UrlEncode(repo);
            collaborator = HttpUtility.UrlEncode(collaborator);
            using (HttpClient request = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = request.DeleteAsync($"{Utils.GetAPIsURL()}/repos?username={username}&password={password}&repo={repo}&collaborator={collaborator}").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                catch (HttpRequestException e)
                {
                    throw new Exception($"APIs exception - {e.Message}");
                }
            }
        }
    }
}
