using System;
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
            if (!Services.AuthenticateUser(UserName, Password))
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
                Password = Utils.SaltAndHashPassword(Password);
                Services.UpdatePassword((string)user["username"], (string)user["password"], Password);
            }
            if (!string.IsNullOrEmpty(Email))
            {
                Services.UpdateEmail((string)user["username"], (string)user["password"], Email);
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
            if (!Services.CreateRepo((string)user["username"], Repo))
            {
                throw new Exception($"Repo already exists.");
            }
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
                Services.AddCollaborator((string)user["username"], (string)user["password"], Repo, Collaborator);
            }
            return 0;
        }
    }

    [Verb("uninvite", HelpText = "Uninvite a user to a repo as an owner or a collaborator.")]
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
            if (!string.IsNullOrEmpty(Owner))
            {
                if (Services.GetUser(Owner) == null)
                {
                    throw new Exception($"User doesn't exists.");
                }
                Services.RemoveOwner((string)user["username"], (string)user["password"], Repo, Owner);
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
}
