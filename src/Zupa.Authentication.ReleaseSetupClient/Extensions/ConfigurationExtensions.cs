using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Zupa.Authentication.Common.Data.Migrations.IdentityServer.ApplicationDb;
using Zupa.Authentication.ReleaseSetupClient.Helpers;
using Zupa.Authentication.ReleaseSetupClient.Models;

namespace Zupa.Authentication.ReleaseSetupClient.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IConfiguration InitialiseApplicationContext(this IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            using (var applicationDbContext = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                applicationDbContext.Database.Migrate();

                InitialiseApplicationHelpers.CreateAdminRoleAsync(roleManager).Wait();

                var users = configuration.GetSection("UserDetails").Get<IEnumerable<UserModel>>();

                if (users != null)
                    foreach (var user in users)
                    {
                        if (!applicationDbContext.Users.Any(u => u.UserName == user.Email))
                            InitialiseApplicationHelpers.CreateUserAsync(user, userManager).Wait();
                    }

                applicationDbContext.SaveChanges();
            }

            return configuration;
        }

        public static IConfiguration InitialiseConfigurationContext(this IConfiguration configuration, IServiceProvider serviceProvider)
        {
            var clients = configuration
                .GetSection("Clients")
                .Get<IEnumerable<ClientModel>>();

            var apiResources = configuration
                .GetSection("ApiResources")
                .Get<IEnumerable<ApiResourceModel>>();

            var identityResources = configuration
                .GetSection("IdentityResources")
                .Get<IEnumerable<string>>();


            using (var configurationDbContext = serviceProvider.GetRequiredService<ConfigurationDbContext>())
            {
                configurationDbContext.Database.Migrate();

                if (clients != null)
                    foreach (var client in clients)
                    {
                        if (!configurationDbContext.Clients.Any(c => c.ClientId == client.ClientId))
                        {
                            Console.WriteLine($"Adding Client: {client.ClientName}");
                            configurationDbContext.Add(InitialiseConfigurationHelpers.MapClient(client)
                                .ToEntity());
                        }
                    }

                if (apiResources != null)
                    foreach (var apiResource in apiResources)
                    {
                        if (!configurationDbContext.ApiResources.Any(a => a.Name == apiResource.Name))
                        {
                            Console.WriteLine($"Adding API Resource: {apiResource.Name}");
                            configurationDbContext.Add(InitialiseConfigurationHelpers.MapApiResource(apiResource)
                                .ToEntity());
                        }
                    }

                if (identityResources != null)
                    foreach (var identityResource in identityResources)
                    {
                        if (!configurationDbContext.IdentityResources.Any(i => i.Name == identityResource))
                        {
                            Console.WriteLine($"Adding Identity Resource: {identityResource}");
                            configurationDbContext.Add(InitialiseConfigurationHelpers
                                .MapIdentityResource(identityResource).ToEntity());
                        }
                    }

                configurationDbContext.SaveChanges();
            }

            return configuration;
        }
    }
}
