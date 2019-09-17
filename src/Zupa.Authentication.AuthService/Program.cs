using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zupa.Authentication.AuthService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Auth Service";

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddUserSecrets<Program>()
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .ConfigureServices(services =>
                {
                    services.AddTransient<ISenderClient, QueueClient>(provider => new QueueClient(
                        config.GetSection("ServiceBusConnection:ServiceBusConnectionString").Value,
                        config.GetSection("ServiceBusConnection:QueueName").Value));
                });

        }
    }
}
