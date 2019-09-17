using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Zupa.Authentication.Common;
using Zupa.Authentication.Common.Data.Migrations.IdentityServer.ApplicationDb;
using Zupa.Authentication.ReleaseSetupClient.Extensions;

namespace Zupa.Authentication.ReleaseSetupClient
{
    public class Program
    {
        private static void Main(string[] args)
        {
            const string connectionStringFlag = "ConnectionString";
            const string serverConfigFlag = "ServerConfig";
            const string userConfigFlag = "UserConfig";

            if (!args.Any())
            {
                Console.WriteLine("Usage: specify arguments as 'ArgName=[value]'");
                Console.WriteLine($"Valid arguments include '{connectionStringFlag}' (required), '{serverConfigFlag}', '{userConfigFlag}'");
                return;
            }

            var connectionString = string.Empty;
            var serverConfigFile = "serverconfig.json";
            var userConfigFile = "userconfig.json";

            foreach (var arg in args)
            {
                var parts = arg.Split("=", 2);
                if (parts.Length < 2)
                    continue;

                switch (parts[0])
                {
                    case connectionStringFlag:
                        connectionString = parts[1];
                        break;
                    case serverConfigFlag:
                        serverConfigFile = parts[1];
                        break;
                    case userConfigFlag:
                        userConfigFile = parts[1];
                        break;
                }
            }

            if (String.IsNullOrEmpty(connectionString))
                throw new ApplicationException("Connection String argument not found.");

            Console.WriteLine("Initialising");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.GetFullPath(serverConfigFile), optional: false)
                .AddJsonFile(Path.GetFullPath(userConfigFile), optional: false)
                .Build();

            var serviceCollection = new ServiceCollection();
            
            Console.WriteLine("Configuring Services");
            ConfigureServices(serviceCollection, connectionString);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            Console.WriteLine("Setting up contexts");
            configuration
                .InitialiseApplicationContext(serviceProvider)
                .InitialiseConfigurationContext(serviceProvider);

            Console.WriteLine("Done!");
        }

        private static void ConfigureServices(IServiceCollection services, string connectionString)
        {
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var migrationsAssembly = typeof(RoleConstants).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddIdentityServer()
                .AddAspNetIdentity<IdentityUser>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                });
        }
    }
}
