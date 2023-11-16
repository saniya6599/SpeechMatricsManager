using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechMatrixManager
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    CheckArguments(args);
                }
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {

            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        public static void CheckArguments(string[] args)
        {
            foreach (var i in args)
            {
                if (i == "-v" || i == "--version")
                {
                    Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.PaddedZeroFormat());
                    Environment.Exit(0);
                }
                else if (i == "-h" || i == "--help")
                {
                    Console.WriteLine(@"
-v or --version   -> Prints the version
-h or --help      -> Prints this screen 
                    ");
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Invalid argument");
                    Environment.Exit(1);
                }
            }
        }

        public static string PaddedZeroFormat(this Version ver)
        {
            return string.Format("{0:00}.{1:00}.{2:00}.{3:00}", ver.Major, ver.Minor, ver.Build, ver.Revision);
        }
    }
}
