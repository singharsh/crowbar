using System;
using CommandLine;

namespace crowbar
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Parser.Default.ParseArguments<LoginOptions, SignUpOptions, UpdateCredentialsOptions, NewOptions, InviteOptions, UninviteOptions>(args)
                    .MapResult(
                        (Options opts) => opts.Execute(),
                        err => Utils.HandleError());
            }
            catch (Exception e)
            {
                Console.WriteLine($"crowbar error - {e.Message}");
            }
        }
    }
}
