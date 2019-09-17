using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Net.Http.Headers;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.AppInsights;
using Zupa.Authentication.AuthService.ComponentTests.Middlewares;
using Zupa.Authentication.AuthService.Configuration;
using Zupa.Authentication.AuthService.Models.Account.Entities;
using Zupa.Libraries.CosmosTableStorageClient;
using Zupa.Libraries.ServiceBus.ServiceBusClient;

namespace Zupa.Authentication.AuthService.ComponentTests
{
    public class TestHelpers
    {
        public const string TestZupaTechEmail = "test@zupa.co.uk";
        public const string Test2ZupaTechEmail = "test2@zupa.co.uk";
        public const string LockedoutZupaTechEmail = "lockedout@zupa.co.uk";
        public const string Lockedout2ZupaTechEmail = "lockedout2@zupa.co.uk";
        public const string NonExisitingZupaTechEmail = "badEmail@zupa.co.uk";
        public const string NewZupaTechEmail = "new@zupa.co.uk";
        public const string NewZupaTechEmail2 = "new2@zupa.co.uk";
        public const string InvalidEmail = "emailzupa.co.uk";
        public const string ValidPassword = "Password0-";
        public const string InvalidPassword = "BadPassword";

        private static readonly string antiForgeryTokenHttpString = @"\<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" \/\>";

        public static string FindSolutionParentFromExecutingChildProject(string solutionName)
        {
            var currentproj = PlatformServices.Default.Application.ApplicationBasePath;
            var currentdirectory = new DirectoryInfo(currentproj);
            do
            {
                var solutionFileInfo = new FileInfo(Path.Combine(currentdirectory.FullName, solutionName));
                if (solutionFileInfo.Exists)
                    return currentdirectory.FullName;
                currentdirectory = currentdirectory.Parent;
            }
            while (currentdirectory.Parent != null);
            throw new FileNotFoundException("Unable to find the given solution from the current project");
        }

        public static HttpClient GetTestClient()
        {
            var solution = FindSolutionParentFromExecutingChildProject("Zupa.Authentication.sln");
            var mainProject = Path.Combine(solution, "src", "Zupa.Authentication.AuthService");

            var server = new TestServer(new WebHostBuilder()
                .ConfigureServices(services => {
                    services.AddTransient<ISenderClient, ISenderClient>(provider => new Mock<ISenderClient>().SetupAllProperties().Object);
                    services.TryAddTransient(provider => Mock.Of<IServiceBusClient<ITopicClient>>());
                    services.TryAddSingleton(provider => Mock.Of<IServiceBusClientFactory>());
                    services.AddTransient<ITrackTelemetry, ITrackTelemetry>(provider => new Mock<ITrackTelemetry>().SetupAllProperties().Object);
                    services.AddSingleton<IStartupFilter, FakeIpAddressStartupFilter>();
                })
                .UseContentRoot(mainProject)
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(mainProject)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddUserSecrets<Program>()
                    .Build())
                .UseEnvironment("Testing")
                .UseStartup<Startup>()
                .ConfigureTestServices(services => {
                    services.Configure<FailedLoginAttemptsSettings>(options =>
                        options.MaxRequestsCount = TestConstants.MaxRequestsCount); 
                    services.Configure<FailedLoginAttemptsSettings>(options =>
                        options.TimeIntervalInSeconds = TestConstants.TimeIntervalInSeconds); 

                    services.AddSingleton<ICosmosTableStorageClient<FailedAttemptEntity>>
                    (provider => new CosmosTableStorageClient<FailedAttemptEntity>(
                        new CosmosTableFactory(),
                        TestConstants.FailedAttemptsConnectionString, TestConstants.FailedAttemptsTableName));
                }));
            return server.CreateClient();
        }

        public static async Task<string> ExtractAntiForgeryTokenFromResponseAsync(HttpResponseMessage response)
        {
            if (response == null) throw new ArgumentNullException(nameof(response));
            if (response.Content == null) throw new ArgumentNullException(nameof(response.Content));

            var responseAsString = await response.Content.ReadAsStringAsync();

            if (responseAsString == null) throw new NullReferenceException(nameof(responseAsString));

            var match = Regex.Match(responseAsString, antiForgeryTokenHttpString);
            return match.Success ? match.Groups[1].Captures[0].Value : null;
        }

        public static HttpRequestMessage CreatePostRequestWithCookiesFromResponse(string path, Dictionary<string, string> formPostBodyData, HttpResponseMessage response)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = new FormUrlEncodedContent(formPostBodyData)
            };

            if (response.Headers.TryGetValues("Set-Cookie", out var responseValues))
            {
                SetCookieHeaderValue.ParseList(responseValues.ToList()).ToList().ForEach(cookie =>
                {
                    httpRequestMessage.Headers.Add("Cookie", new CookieHeaderValue(cookie.Name.Value, cookie.Value.Value).ToString());
                });
            }

            return httpRequestMessage;
        }
    }
}
