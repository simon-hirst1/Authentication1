using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zupa.Authentication.Common.Data.Migrations.IdentityServer.ApplicationDb;

namespace Zupa.Authentication.Common.Data
{
    public class DatabaseInitialiser : IDatabaseInitialiser
    {
        private readonly ApplicationDbContext _context;
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly PersistedGrantDbContext _persistedGrantDbContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DatabaseInitialiser(
            ApplicationDbContext context,
            ConfigurationDbContext configurationDbContext,
            PersistedGrantDbContext persistedGrantDbContext,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _configurationDbContext = configurationDbContext ?? throw new ArgumentNullException(nameof(configurationDbContext));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _persistedGrantDbContext = persistedGrantDbContext ?? throw new ArgumentNullException(nameof(persistedGrantDbContext));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        public void Initialise(IApplicationBuilder app, IHostingEnvironment environment)
        {
            if (environment.IsEnvironment("Testing"))
            {
                PopulateDatabase(_userManager, _roleManager);
            }
            else if (environment.IsEnvironment("Development"))
            {
                DropAndMigrateDatabase();
                PopulateDatabase(_userManager, _roleManager);
            }
            else
            {
                MigrateDatabase();
                CreateAdminRoleAsync(_roleManager).Wait();
            }
        }

        private static async Task CreateAdminRoleAsync(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(RoleConstants.AdminRole))
            {
                var role = new IdentityRole { Name = RoleConstants.AdminRole };
                await roleManager.CreateAsync(role);
            }
        }

        private void DropAndMigrateDatabase()
        {
            _context.Database.EnsureDeleted();
            _configurationDbContext.Database.EnsureDeleted();
            _persistedGrantDbContext.Database.EnsureDeleted();

            MigrateDatabase();
        }

        private void MigrateDatabase()
        {
            _context.Database.Migrate();
            _configurationDbContext.Database.Migrate();
            _persistedGrantDbContext.Database.Migrate();
        }

        private void PopulateDatabase(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            CreateAdminRoleAsync(roleManager).Wait();

            if (!_context.Users.Any())
                CreateUsersAsync(userManager).Wait();

            if (!_configurationDbContext.IdentityResources.Any())
            {
                foreach (var resource in GetIdentityResources())
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());

                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                foreach (var resource in GetApiResources())
                    _configurationDbContext.ApiResources.Add(resource.ToEntity());

                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.Clients.Any())
            {
                foreach (var resource in GetClients())
                    _configurationDbContext.Clients.Add(resource.ToEntity());

                _configurationDbContext.SaveChanges();
            }
        }

        private static async Task CreateUsersAsync(UserManager<IdentityUser> userManager)
        {
            const string validPassword = "Password0-";

            var emailAndUsername = "test@zupa.co.uk";
            var adminUser = new IdentityUser
            {
                Id = "513D4B88-D066-41F9-8F58-8142BDE8B828",
                UserName = emailAndUsername,
                Email = emailAndUsername,
                EmailConfirmed = true,
            };

            await userManager.CreateAsync(adminUser, validPassword);
            await userManager.AddToRoleAsync(adminUser, RoleConstants.AdminRole);

            emailAndUsername = "test2@zupa.co.uk";
            await userManager.CreateAsync(new IdentityUser
            {
                Id = "AF2FD56C-1B84-474E-B4A6-D9C193F07B33",
                UserName = emailAndUsername,
                Email = emailAndUsername,
                EmailConfirmed = true,
            }, validPassword);

            emailAndUsername = "lockedOut@zupa.co.uk";
            await userManager.CreateAsync(new IdentityUser
            {
                Id = "F325A630-E352-4F13-9601-F94F55E846BD",
                UserName = emailAndUsername,
                Email = emailAndUsername,
                EmailConfirmed = true,
                LockoutEnabled = true,
                LockoutEnd = DateTime.UtcNow.AddMinutes(15)
            }, validPassword);

            emailAndUsername = "lockedOut2@zupa.co.uk";
            await userManager.CreateAsync(new IdentityUser
            {
                Id = "AFBD5C95-43B4-4372-8CC9-E05B8F505876",
                UserName = emailAndUsername,
                Email = emailAndUsername,
                EmailConfirmed = true,
                AccessFailedCount = 8
            }, validPassword);

            emailAndUsername = "aapple@zupa.co.uk";
            await userManager.CreateAsync(new IdentityUser
            {
                Id = "43E382E8-D370-4377-A340-D96F6AD5C250",
                UserName = emailAndUsername,
                Email = emailAndUsername,
                EmailConfirmed = true
            }, validPassword);

            emailAndUsername = "bbanana@zupa.co.uk";
            await userManager.CreateAsync(new IdentityUser
            {
                Id = "CEB9082D-7660-4C89-B9DC-04981CDE148B",
                UserName = emailAndUsername,
                Email = emailAndUsername,
                EmailConfirmed = true
            }, validPassword);

            emailAndUsername = "ccherry@zupa.co.uk";
            await userManager.CreateAsync(new IdentityUser
            {
                Id = "C4FA3286-0BF8-41DC-949A-FD72A1D41917",
                UserName = emailAndUsername,
                Email = emailAndUsername,
                EmailConfirmed = true
            }, validPassword);

            emailAndUsername = "ddamson@zupa.co.uk";
            await userManager.CreateAsync(new IdentityUser
            {
                Id = "CD4B14B7-783C-4D6A-8D6F-55F55487C8D9",
                UserName = emailAndUsername,
                Email = emailAndUsername,
                EmailConfirmed = true
            }, validPassword);

            emailAndUsername = "eelderberry@zupa.co.uk";
            await userManager.CreateAsync(new IdentityUser
            {
                Id = "0BD65564-F8BA-48B3-96F1-6CC587EBD9D0",
                UserName = emailAndUsername,
                Email = emailAndUsername,
                EmailConfirmed = true
            }, validPassword);
        }

        private static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };
        }

        private static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("Zupa.Messaging", "Zupa Messaging"),
                new ApiResource("Zupa.Media", "Zupa Media"),
                new ApiResource("Zupa.Safe", "Zupa Safe"),
                new ApiResource("Zupa.GDPR.Portability", "Zupa GDPR Portability"),
                new ApiResource("Zupa.GDPR.Forget", "Zupa GDPR Forget"),
                new ApiResource("Zupa.Contacts", "Zupa Contacts"),
                new ApiResource("Zupa.Groups", "Zupa Groups"),
                new ApiResource("Zupa.Occasions", "Zupa Occasions"),
                new ApiResource("Zupa.Organisations", "Zupa Organisations"),
                new ApiResource("Zupa.Recipes", "Zupa Recipes"),
                new ApiResource("Zupa.Products", "Zupa Products"),
                new ApiResource("Zupa.Orders", "Zupa Orders"),
                new ApiResource("Zupa.Stock", "Zupa Stock"),
                new ApiResource("Zupa.Authentication", "Zupa Authentication"),
                new ApiResource("Zupa.Agreements", "Zupa Agreements")
            };
        }

        private static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "Zupa.Apps.Chat.Web",
                    ClientName = "Zupa Apps Chat Web",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris = {"http://localhost:33156/callback.html"},
                    PostLogoutRedirectUris = {"http://localhost:33156/index.html"},
                    AllowedCorsOrigins = {"http://localhost:33156"},
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "Zupa.Messaging",
                        "Zupa.Media",
                        "Zupa.Contacts",
                        "Zupa.Groups",
                        "Zupa.Occasions"
                    }
                },
                new Client
                {
                    ClientId = "Zupa.Poc.Alerts",
                    ClientName = "Zupa.Poc.Alerts",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris = {"http://localhost:35156/callback.html"},
                    PostLogoutRedirectUris = {"http://localhost:35156/index.html"},
                    AllowedCorsOrigins = {"http://localhost:35156"},
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                    }
                },
                new Client
                {
                    ClientId = "Zupa.Apps.Market.Web",
                    ClientName = "Zupa Apps Market Web",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris = {"http://localhost:55435/callback.html"},
                    PostLogoutRedirectUris = {"http://localhost:55435/index.html"},
                    AllowedCorsOrigins = {"http://localhost:55435"},
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "Zupa.Organisations",
                        "Zupa.Contacts",
                        "Zupa.Recipes",
                        "Zupa.Products",
                        "Zupa.Media",
                        "Zupa.Orders",
                        "Zupa.Stock",
                        "Zupa.Safe",
                        "Zupa.Occasions",
                        "Zupa.Groups",
                        "Zupa.Messaging",
                        "Zupa.Authentication",
                        "Zupa.Agreements"
                    }
                },
                new Client
                {
                    ClientId = "Zupa.Safe",
                    ClientName = "Zupa Safe",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris = {"http://localhost:4937/callback.html"},
                    PostLogoutRedirectUris = {"http://localhost:4937/"},
                    AllowedCorsOrigins = {"http://localhost:4937"},
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "Zupa.Safe",
                        "Zupa.GDPR.Portability",
                        "Zupa.GDPR.Forget"
                    }
                },
                new Client
                {
                    ClientId = "Zupa.GDPR.Forget",
                    ClientName = "Zupa GDPR Forget",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris = {"http://localhost:3964/forget/callback.html"},
                    PostLogoutRedirectUris = {"http://localhost:3964/forget/index.html"},
                    AllowedCorsOrigins = {"http://localhost:3964"},
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "Zupa.GDPR.Forget"
                    }
                },
                new Client
                {
                    ClientId = "Zupa.Recipes.Web",
                    ClientName = "Zupa Recipes Web",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    RedirectUris = {"http://localhost:40470/callback.html"},
                    PostLogoutRedirectUris = {"http://localhost:40470/index.html"},
                    AllowedCorsOrigins = {"http://localhost:40470"},
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "Zupa.Recipes",
                        "Zupa.Products"
                    }
                },
                new Client
                {
                    ClientId = "TokenClient",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secretToken".Sha256())
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "TokenClient",
                        "Zupa.Messaging",
                        "Zupa.Contacts",
                        "Zupa.Groups",
                        "Zupa.Media",
                        "Zupa.GDPR.Forget",
                        "Zupa.Safe",
                        "Zupa.GDPR.Portability",
                        "Zupa.Recipes",
                        "Zupa.Products",
                        "Zupa.Organisations",
                        "Zupa.Orders",
                        "Zupa.Stock",
                        "Zupa.Authentication"
                    }
                },
                new Client
                {
                    ClientId = "TestClient",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = {"Zupa.Messaging"}
                },
                new Client
                {
                    ClientId = "ProductsUpdatedServiceBus",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secretToken".Sha256())
                    },
                    AllowedScopes =
                    {
                        "Zupa.Products"
                    }
                },
                new Client
                {
                    ClientId = "Zupa.Organisations.API",
                    ClientName = "Zupa Organisations API",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("testSecret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "Zupa.Authentication"
                    }
                },
                new Client
                {
                    ClientId = "Zupa.Organisations.Function",
                    ClientName = "Zupa Organisation Function",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("testSecret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "Zupa.Authentication",
                        "Zupa.Organisations"
                    }
                },
                new Client
                {
                    ClientId = "Zupa.Products.API",
                    ClientName = "Zupa Products API",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("testSecret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "Zupa.Organisations",
                        "Zupa.Agreements"
                    }
                },
                new Client
                {
                    ClientId = "Zupa.Agreements.API",
                    ClientName = "Zupa Agreements API",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("testSecret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "Zupa.Organisations",
                        "Zupa.Products"
                    }
                },
                new Client
                {
                    ClientId = "Zupa.Messaging.API",
                    ClientName = "Zupa Messaging API",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("testSecret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "Zupa.Authentication",
                        "Zupa.Organisations"
                    }
                },
            };
        }
    }
}
