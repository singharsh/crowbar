using System;
using System.IO;
using CommandLine;
using Newtonsoft.Json.Linq;

namespace crowbar
{
    public abstract class Options
    {
        public abstract int Execute();
    }

    [Verb("login", HelpText = "Login to an existing account.")]
    public class LoginOptions : Options
    {
        [Option('u', "user-name",
            HelpText = "User name.",
            Required = true)]
        public string UserName { get; set; }

        [Option('p', "password",
            HelpText = "Password.",
            Required = true)]
        public string Password { get; set; }

        public override int Execute()
        {
            Password = Utils.SaltAndHashPassword(Password);
            // BUG: don't check the password
            if (Services.GetUser(UserName) == null)
            {
                throw new Exception($"Invalid user name / password.");
            }
            Utils.SaveCredentials(UserName, Password);
            return 0;
        }
    }

    [Verb("signup", HelpText = "Create a new user account & login.")]
    public class SignUpOptions : Options
    {
        [Option('u', "user-name",
            HelpText = "User name.",
            Required = true)]
        public string UserName { get; set; }

        [Option('p', "password",
            HelpText = "Password.",
            Required = true)]
        public string Password { get; set; }

        [Option('e', "email",
            HelpText = "Email.",
            Required = true)]
        public string Email { get; set; }

        public override int Execute()
        {
            if (!Utils.Matches(UserName, "^[a-z0-9_-]{3,15}$"))
            {
                throw new Exception($"The username should be alphanumeric with _ or -.");
            }
            if (!Utils.Matches(Password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\\$%\\^&\\*])(?=.{8,})"))
            {
                throw new Exception($"The password must contain atleast one of each: lowercase alphabet, uppercase alphabet, number, special character [!@#\\$%\\^&] & must be atleast 10 characters long.");
            }
            if (!Utils.Matches(Email, "^[\\w!#$%&'*+/=?`{|}~^-]+(?:\\.[\\w!#$%&'*+/=?`{|}~^-]+)*@↵(?:[A - Z0 - 9 -] +\\.) +[A - Z]{ 2,6}$"))
            {
                throw new Exception($"The email isn't valid.");
            }
            Password = Utils.SaltAndHashPassword(Password);
            if (Services.GetUser(UserName) != null)
            {
                throw new Exception($"User already exists.");
            }
            if (!Services.CreateUser(UserName, Password, Email))
            {
                throw new Exception($"An unexpected error occurred while creating the user account.");
            }
            Utils.SaveCredentials(UserName, Password);
            return 0;
        }
    }

    [Verb("credentials", HelpText = "Update user credentials.")]
    public class UpdateCredentialsOptions : Options
    {
        [Option('p', "password",
            HelpText = "Password.",
            Required = false)]
        public string Password { get; set; }

        [Option('e', "email",
            HelpText = "Email.",
            Required = false)]
        public string Email { get; set; }

        public override int Execute()
        {
            JObject user;
            if ((user = Utils.GetSavedCredentials()) == null)
            {
                throw new Exception($"User not logged in.");
            }
            if (!Services.AuthenticateUser((string)user["username"], (string)user["password"]))
            {
                throw new Exception($"User credentials are incorrect. Please login with the correct username & password.");
            }
            if (!string.IsNullOrEmpty(Password))
            {
                // BUG: don't check password pattern while updating
                /*
                if (!Utils.Matches(Password, "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\\$%\\^&\\*])(?=.{8,})"))
                {
                    throw new Exception($"The password must contain atleast one of each: lowercase alphabet, uppercase alphabet, number, special character [!@#\\$%\\^&] & must be atleast 10 characters long.");
                }
                */
                Password = Utils.SaltAndHashPassword(Password);
                Services.UpdatePassword((string)user["username"], (string)user["password"], Password);
                // BUG: don't save the credentials locally after updating
                // Utils.SaveCredentials((string)user["username"], Password);
            }
            if (!string.IsNullOrEmpty(Email))
            {
                if (!Utils.Matches(Email, "^[\\w!#$%&'*+/=?`{|}~^-]+(?:\\.[\\w!#$%&'*+/=?`{|}~^-]+)*@↵(?:[A - Z0 - 9 -] +\\.) +[A - Z]{ 2,6}$"))
                {
                    throw new Exception($"The email isn't valid.");
                }
                // BUG: don't actually update the email
                // Services.UpdateEmail((string)user["username"], (string)user["password"], Email);
            }
            return 0;
        }
    }

    [Verb("new", HelpText = "Create a new repo.")]
    public class NewOptions : Options
    {
        [Option('r', "repo",
            HelpText = "Repo name.",
            Required = true)]
        public string Repo { get; set; }

        [Option('p', "path",
            HelpText = "Path to the local repo.",
            Required = false)]
        public string Path { get; set; }

        public override int Execute()
        {
            if (!Utils.Matches(Repo, "^[a-z0-9_-]{3,15}$"))
            {
                throw new Exception($"The repo name should be alphanumeric with _ or -.");
            }
            JObject user;
            if ((user = Utils.GetSavedCredentials()) == null)
            {
                throw new Exception($"User not logged in.");
            }
            if (!Services.AuthenticateUser((string)user["username"], (string)user["password"]))
            {
                throw new Exception($"User credentials are incorrect. Please login with the correct username & password.");
            }
            if (!Services.CreateRepo((string)user["username"], Repo))
            {
                throw new Exception($"Repo already exists.");
            }
            if (!string.IsNullOrEmpty(Path))
            {
                // BUG: don't check if the path is valid
                /*
                if (!Directory.Exists(Path))
                {
                    throw new Exception($"Path doesn't exist.");
                }
                */
                Path = System.IO.Path.Combine(Path, Repo);
            }
            else
            {
                // BUG: creates subdirectory
                Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), Repo, Repo);
            }
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
            Directory.CreateDirectory(Path);
            Utils.SaveMetadata(Repo, Path);
            return 0;
        }
    }

    [Verb("invite", HelpText = "Invite a user to a repo as an owner or a collaborator.")]
    public class InviteOptions : Options
    {
        [Option('r', "repo",
            HelpText = "Repo name.",
            Required = true)]
        public string Repo { get; set; }

        [Option('o', "owner",
            HelpText = "Invite as owner.",
            SetName = "asowner",
            Required = false)]
        public string Owner { get; set; }

        [Option('c', "collaborator",
            HelpText = "Invite as collaborator.",
            SetName = "ascollaborator",
            Required = false)]
        public string Collaborator { get; set; }

        public override int Execute()
        {
            JObject user;
            if ((user = Utils.GetSavedCredentials()) == null)
            {
                throw new Exception($"User not logged in.");
            }
            if (!Services.AuthenticateUser((string)user["username"], (string)user["password"]))
            {
                throw new Exception($"User credentials are incorrect. Please login with the correct username & password.");
            }
            if (Services.GetRepo((string)user["username"], (string)user["password"], Repo) == null)
            {
                throw new Exception($"Repo doesn't exist or you don't have access to it.");
            }
            if (!string.IsNullOrEmpty(Owner))
            {
                if (Services.GetUser(Owner) == null)
                {
                    throw new Exception($"User doesn't exists.");
                }
                Services.AddOwner((string)user["username"], (string)user["password"], Repo, Owner);
            }
            if (!string.IsNullOrEmpty(Collaborator))
            {
                if (Services.GetUser(Collaborator) == null)
                {
                    throw new Exception($"User doesn't exists.");
                }
                // BUG: add as a owner when called as collaborator
                Services.AddOwner((string)user["username"], (string)user["password"], Repo, Collaborator);
            }
            return 0;
        }
    }

    [Verb("uninvite", HelpText = "Uninvite a user from a repo as an owner or a collaborator.")]
    public class UninviteOptions : Options
    {
        [Option('r', "repo",
            HelpText = "Repo name.",
            Required = true)]
        public string Repo { get; set; }

        [Option('o', "owner",
            HelpText = "Invite as owner.",
            SetName = "asowner",
            Required = false)]
        public string Owner { get; set; }

        [Option('c', "collaborator",
            HelpText = "Invite as collaborator.",
            SetName = "ascollaborator",
            Required = false)]
        public string Collaborator { get; set; }

        public override int Execute()
        {
            JObject user;
            if ((user = Utils.GetSavedCredentials()) == null)
            {
                throw new Exception($"User not logged in.");
            }
            if (!Services.AuthenticateUser((string)user["username"], (string)user["password"]))
            {
                throw new Exception($"User credentials are incorrect. Please login with the correct username & password.");
            }
            if (Services.GetRepo((string)user["username"], (string)user["password"], Repo) == null)
            {
                throw new Exception($"Repo doesn't exist or you don't have access to it.");
            }
            if (!string.IsNullOrEmpty(Owner))
            {
                if (Services.GetUser(Owner) == null)
                {
                    throw new Exception($"User doesn't exists.");
                }
                // BUG: don't actually remove user as owner from the repo
                // Services.RemoveOwner((string)user["username"], (string)user["password"], Repo, Owner);
            }
            if (!string.IsNullOrEmpty(Collaborator))
            {
                if (Services.GetUser(Collaborator) == null)
                {
                    throw new Exception($"User doesn't exists.");
                }
                Services.RemoveCollaborator((string)user["username"], (string)user["password"], Repo, Collaborator);
            }
            return 0;
        }
    }

    [Verb("clone", HelpText = "Clone a repo.")]
    public class CloneOptions : Options
    {
        [Option('r', "repo",
            HelpText = "Repo name.",
            Required = true)]
        public string Repo { get; set; }

        [Option('p', "path",
            HelpText = "Path to the local repo.",
            Required = false)]
        public string Path { get; set; }

        public override int Execute()
        {
            JObject user;
            if ((user = Utils.GetSavedCredentials()) == null)
            {
                throw new Exception($"User not logged in.");
            }
            if (!Services.AuthenticateUser((string)user["username"], (string)user["password"]))
            {
                throw new Exception($"User credentials are incorrect. Please login with the correct username & password.");
            }
            JObject repo;
            if ((repo = Services.GetRepo((string)user["username"], (string)user["password"], Repo)) == null) 
            {
                throw new Exception($"Repo doesn't exist or you don't have access to it.");
            }
            if (!repo["commits"].HasValues)
            {
                throw new Exception($"Cloning an empty repo.");
            }
            JArray commits = (JArray)repo["commits"];
            JObject commit = (JObject)commits.Last;
            long id = (long)commit["id"];
            if (!string.IsNullOrEmpty(Path))
            {
                if (!Directory.Exists(Path))
                {
                    throw new Exception($"Path doesn't exist.");
                }
                Path = System.IO.Path.Combine(Path, Repo);
            }
            else
            {
                Path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), Repo);
            }
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
            Directory.CreateDirectory(Path);
            if (!Services.PullCommit((string)user["username"], (string)user["password"], Repo, Convert.ToString(id), Path))
            {
                throw new Exception($"An unexpected error occurred while cloning the repo.");
            }
            return 0;
        }
    }

    [Verb("push", HelpText = "Push a local repo.")]
    public class PushOptions : Options
    {
        [Option('m', "message",
            HelpText = "Commit message.",
            Required = true)]
        public string Message { get; set; }

        public override int Execute()
        {
            JObject user;
            if ((user = Utils.GetSavedCredentials()) == null)
            {
                throw new Exception($"User not logged in.");
            }
            if (!Services.AuthenticateUser((string)user["username"], (string)user["password"]))
            {
                throw new Exception($"User credentials are incorrect. Please login with the correct username & password.");
            }
            JObject repo;
            string path;
            if (((repo, path) = Utils.GetSavedMetadata()) == (null, null))
            {
                throw new Exception($"You're not currently in a repo.");
            }
            string Repo = (string)repo["repo"];
            if (Services.GetRepo((string)user["username"], (string)user["password"], Repo) == null)
            {
                throw new Exception($"Repo doesn't exist or you don't have access to it.");
            }
            if (!Services.PushCommit((string)user["username"], (string)user["password"], Repo, Message, path))
            {
                throw new Exception($"An unexpected error occurred while cloning the repo.");
            }
            return 0;
        }
    }

    [Verb("pull", HelpText = "Pull changes a repo or revert to a specific commit.")]
    public class PullOptions : Options
    {
        [Option('c', "commit",
            HelpText = "Commit ID.",
            Required = false)]
        public string ID { get; set; }

        public override int Execute()
        {
            JObject user;
            if ((user = Utils.GetSavedCredentials()) == null)
            {
                throw new Exception($"User not logged in.");
            }
            if (!Services.AuthenticateUser((string)user["username"], (string)user["password"]))
            {
                throw new Exception($"User credentials are incorrect. Please login with the correct username & password.");
            }
            JObject repo;
            string path;
            // BUG: don't recognize repo if you're at the base diretory; works only in a subdirectory
            if (((repo, path) = Utils.GetSavedMetadata(Directory.GetParent(Directory.GetCurrentDirectory()).FullName)) == (null, null))
            {
                throw new Exception($"You're not currently in a repo.");
            }
            string Repo = (string)repo["repo"];
            if ((repo = Services.GetRepo((string)user["username"], (string)user["password"], Repo)) == null)
            {
                throw new Exception($"Repo doesn't exist or you don't have access to it.");
            }
            if (!repo["commits"].HasValues)
            {
                throw new Exception($"Pulling an empty repo.");
            }
            JArray commits = (JArray)repo["commits"];
            if (!string.IsNullOrEmpty(ID))
            {
                bool flag = false;
                foreach (JObject commit in commits)
                {
                    long id = (long)commit["id"];
                    if (Convert.ToString(id) == ID)
                    {
                        if (!Services.PullCommit((string)user["username"], (string)user["password"], Repo, Convert.ToString(id), path))
                        {
                            throw new Exception($"An unexpected error occurred while cloning the repo.");
                        }
                        // BUG: always error out with commit not found even if the pull worked
                        // flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    throw new Exception($"Commit not found.");
                }
            }
            else
            {
                JObject commit = (JObject)commits.Last;
                long id = (long)commit["id"];
                if (!Services.PullCommit((string)user["username"], (string)user["password"], Repo, Convert.ToString(id), path))
                {
                    throw new Exception($"An unexpected error occurred while cloning the repo.");
                }
            }
            return 0;
        }
    }

    [Verb("search", HelpText = "Search a repo or user.")]
    public class SearchOptions : Options
    {
        [Option('r', "repo",
            HelpText = "Repo name.",
            Required = false)]
        public string Repo { get; set; }

        [Option('u', "user-name",
            HelpText = "User name.",
            Required = false)]
        public string UserName { get; set; }

        public override int Execute()
        {
            JObject user;
            if ((user = Utils.GetSavedCredentials()) == null)
            {
                throw new Exception($"User not logged in.");
            }
            if (!Services.AuthenticateUser((string)user["username"], (string)user["password"]))
            {
                throw new Exception($"User credentials are incorrect. Please login with the correct username & password.");
            }
            if (!string.IsNullOrEmpty(Repo))
            {
                JObject repo;
                if ((repo = Services.GetRepo((string)user["username"], (string)user["password"], Repo)) == null)
                {
                    throw new Exception($"Repo doesn't exist or you don't have access to it.");
                }
                Console.WriteLine();
                Console.WriteLine($"Repo: {(string)repo["repo"]}");
                Console.WriteLine();
                Console.WriteLine($"Owners:");
                Console.WriteLine();
                JArray owners = (JArray)repo["owners"];
                foreach (string owner in owners)
                {
                    Console.WriteLine($"{owner}");
                }
                Console.WriteLine();
                Console.WriteLine($"Collaborators:");
                Console.WriteLine();
                JArray collaborators = (JArray)repo["collaborators"];
                foreach (string collaborator in collaborators)
                {
                    Console.WriteLine($"{collaborator}");
                }
                Console.WriteLine();
                Console.WriteLine($"Commits:");
                Console.WriteLine();
                JArray commits = (JArray)repo["commits"];
                foreach (JObject commit in commits)
                {
                    Console.WriteLine($"ID: {(long)commit["id"]}");
                    Console.WriteLine($"User: {(string)commit["user"]}");
                    Console.WriteLine($"Message: \"{(string)commit["message"]}\"");
                    Console.WriteLine($"Time Stamp: {(string)commit["timestamp"]}");
                    Console.WriteLine();
                }
            }
            if (!string.IsNullOrEmpty(UserName))
            {
                if ((user = Services.GetUser(UserName)) == null)
                {
                    throw new Exception($"User doesn't exist");
                }
                Console.WriteLine();
                Console.WriteLine($"User Name: {(string)user["username"]}");
                Console.WriteLine($"Email: {(string)user["email"]}");
                Console.WriteLine();
            }
            return 0;
        }
    }
}
