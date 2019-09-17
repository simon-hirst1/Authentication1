using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Reflection;
using Zupa.Authentication.AuthService.AppInsights;
using Zupa.Authentication.AuthService.Configuration;
using Zupa.Authentication.AuthService.Data;
using Zupa.Authentication.AuthService.EmailSending;
using Zupa.Authentication.AuthService.Models.Account.Entities;
using Zupa.Authentication.AuthService.Models.Entity;
using Zupa.Authentication.AuthService.Security;
using Zupa.Authentication.AuthService.Services.FailedLoginAttempts;
using Zupa.Authentication.AuthService.Services.Password;
using Zupa.Authentication.AuthService.Services.Registration;
using Zupa.Authentication.AuthService.Validators.Password;
using Zupa.Authentication.AuthService.Validators.User;
using Zupa.Authentication.Common;
using Zupa.Authentication.Common.Data;
using Zupa.Authentication.Common.Data.Migrations.IdentityServer.ApplicationDb;
using Zupa.Libraries.CosmosTableStorageClient;
using Zupa.Libraries.ServiceBus.ServiceBusClient;
using Zupa.Libraries.ServiceBus.ServiceBusClient.Configuration;

namespace Zupa.Authentication.AuthService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IHostingEnvironment CurrentEnv { get; }

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            CurrentEnv = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var databaseConnectionString = Configuration.GetConnectionString("DefaultConnection");

            if (CurrentEnv.IsEnvironment("Testing"))
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("TestDb"));
            }
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(databaseConnectionString));
            }

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("apipolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                });
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddJwtBearer(options =>
            {
                var identityServerConfiguration = Configuration
                    .GetSection("IdentityServer")
                    .Get<IndentityServerConfiguration>();

                options.Audience = identityServerConfiguration.Audience;
                options.Authority = identityServerConfiguration.Authority;
                options.RequireHttpsMetadata = CurrentEnv.IsProduction();
            });

            services.AddApplicationInsightsTelemetry(Configuration);

            services.Configure<LockoutSettings>(options => Configuration.GetSection("LockoutRules").Bind(options));
            services.Configure<AppInsightsSettings>(options => Configuration.GetSection("ApplicationInsights").Bind(options));
            services.Configure<TransactionalTemplateConfiguration>(options => Configuration.GetSection("TransactionalTemplates").Bind(options));
            services.Configure<MessageConstants>(options => Configuration.GetSection("MessageConstants").Bind(options));
            services.Configure<BlacklistedPasswordsSettings>(options => Configuration.GetSection("PasswordRules:BlacklistedPasswords").Bind(options));
            services.Configure<PasswordRulesSettings>(options => Configuration.GetSection("PasswordRules").Bind(options));
            services.Configure<PasswordHashingConfiguration>(options => Configuration.GetSection("PasswordHashing").Bind(options));
            services.Configure<FailedLoginAttemptsTableStorageConfiguration>(options => Configuration.GetSection(nameof(FailedLoginAttemptsTableStorageConfiguration)).Bind(options));
            services.Configure<FailedLoginAttemptsSettings>(options => Configuration.GetSection(nameof(FailedLoginAttemptsSettings)).Bind(options));
            
            var failedLoginAttemptsConfiguration = Configuration
                .GetSection(nameof(FailedLoginAttemptsTableStorageConfiguration))
                .Get<FailedLoginAttemptsTableStorageConfiguration>();

            services.AddSingleton<ICosmosTableStorageClient<FailedAttemptEntity>>
            (provider => new CosmosTableStorageClient<FailedAttemptEntity>(
                new CosmosTableFactory(),
                failedLoginAttemptsConfiguration.Connection, failedLoginAttemptsConfiguration.TableName));
            services.AddTransient<ICosmosTableCommand<FailedAttemptEntity>, CosmosTableCommand<FailedAttemptEntity>>();
            services.AddTransient<ICosmosTableQuery<TableQuery<FailedAttemptEntity>>, CosmosTableQuery<FailedAttemptEntity>>();
            
            services.AddTransient<IFailedLoginAttemptsService, FailedLoginAttemptsService>();
            services.AddTransient<IFailedLoginAttemptsRepository, FailedLoginAttemptsRepository>();

            var whitelistTableStorageSettings = Configuration.GetSection(nameof(WhitelistTableStorageSettings))
                .Get<WhitelistTableStorageSettings>();

            services.AddSingleton<ICosmosTableStorageClient<WhitelistEntity>>(
                provider => new CosmosTableStorageClient<WhitelistEntity>(
                    new CosmosTableFactory(),
                    whitelistTableStorageSettings.Connection,
                    whitelistTableStorageSettings.TableName));
            services.AddTransient<ICosmosTableQuery<TableQuery<WhitelistEntity>>, CosmosTableQuery<WhitelistEntity>>();

            services.AddTransient<IWhitelistRepository, WhitelistRepository>();
            services.AddTransient<ISendEmail, SendGridEmailSender>();
            services.AddTransient<IRegisterService, RegisterService>();
            services.AddSingleton<ITrackTelemetry, TrackTelemetry>();
            services.TryAddSingleton<IServiceBusClientFactory, ServiceBusClientFactory>();
            services.AddScoped<IDatabaseInitialiser, DatabaseInitialiser>();
            services.AddSingleton(Configuration);
            services.AddScoped<IPasswordHasher<IdentityUser>, ArgonHash<IdentityUser>>();

            var authenticationTopicClientSettings = Configuration.GetSection("AuthenticationTopicClientSettings")
               .Get<TopicClientConfiguration>();

            services.TryAddTransient<IServiceBusClient<ITopicClient>>(provider =>
            {
                var factory = provider.GetRequiredService<IServiceBusClientFactory>();
                var telemetryTracker = provider.GetRequiredService<ITrackTelemetry>();
                return factory.GetClient(authenticationTopicClientSettings, telemetryTracker.TrackException);
            });

            services.AddHttpClient<IPwnedPasswordsService, PwnedPasswordsService>();
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = Configuration.GetValue<int>("PasswordRules:RequiredLength");

                    options.Lockout.MaxFailedAccessAttempts = Configuration.GetValue<int>("LockoutRules:MaxFailedAccessAttempts");
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(Configuration.GetValue<int>("LockoutRules:DefaultLockoutTimeSpan"));

                    options.SignIn.RequireConfirmedEmail = Configuration.GetValue<bool>("EmailRequirements:RequireConfirmedEmail");
                    options.User.RequireUniqueEmail = Configuration.GetValue<bool>("EmailRequirements:RequireUniqueEmail");
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddPasswordValidator<BlacklistedPasswordValidator<IdentityUser>>()
                .AddPasswordValidator<MaxLengthPasswordValidator<IdentityUser>>()
                .AddPasswordValidator<ExpectedInformationPasswordValidator<IdentityUser>>()
                .AddUserValidator<WhitelistedEmailValidator<IdentityUser>>();

            var migrationsAssembly = typeof(RoleConstants).GetTypeInfo().Assembly.GetName().Name;

            services.AddIdentityServer()
                .AddDeveloperSigningCredential(CurrentEnv.IsDevelopment())
                .AddAspNetIdentity<IdentityUser>()
                .AddConfigurationStore(options =>
                {
                    if (CurrentEnv.IsEnvironment("Testing"))
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseInMemoryDatabase("TestDb");
                    }
                    else
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseSqlServer(databaseConnectionString,
                                sql => sql.MigrationsAssembly(migrationsAssembly));
                    }
                })
                .AddOperationalStore(options =>
                {
                    if (CurrentEnv.IsEnvironment("Testing"))
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseInMemoryDatabase("TestDb");
                    }
                    else
                    {
                        options.ConfigureDbContext = builder =>
                            builder.UseSqlServer(databaseConnectionString,
                                sql => sql.MigrationsAssembly(migrationsAssembly));
                    }
                    options.EnableTokenCleanup = Configuration.GetValue<bool>("TokenCleanup:EnableTokenCleanup");
                    options.TokenCleanupInterval = TimeSpan.FromSeconds(Configuration.GetValue<int>("TokenCleanup:TokenCleanupInterval")).Seconds;
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, UserManager<IdentityUser> userManager, IDatabaseInitialiser databaseInitialiser)
        {
            app.UseHsts();

            databaseInitialiser.Initialise(app, env);
            app.UseMiddleware<AppInsightsRequestTrackingMiddleware>();

            app.UseIdentityServer();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}
