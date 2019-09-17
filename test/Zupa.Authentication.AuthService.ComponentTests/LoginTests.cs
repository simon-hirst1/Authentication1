using FluentAssertions;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Zupa.Authentication.AuthService.ComponentTests.Collections;
using Zupa.Authentication.AuthService.ComponentTests.Fixtures;
using Zupa.Authentication.AuthService.Models.Account.Entities;

namespace Zupa.Authentication.AuthService.ComponentTests
{
    [Collection(nameof(FailedAttemptsTableCollection))]
    public class LoginTests : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _antiForgToken;
        private readonly CloudTable _fixtureTable;
        private readonly HttpResponseMessage _httpResponseMessage;

        public LoginTests(FailedAttemptsTableFixture fixture)
        {
            _client = TestHelpers.GetTestClient();
            _fixtureTable = fixture.Table;

            _httpResponseMessage = _client.GetAsync("/Account/Login").Result;
            _httpResponseMessage.EnsureSuccessStatusCode();

            _antiForgToken = TestHelpers.ExtractAntiForgeryTokenFromResponseAsync(_httpResponseMessage).Result;
        }
        
        [Theory(DisplayName = "User logins")]
        [InlineData(TestHelpers.TestZupaTechEmail, TestHelpers.InvalidPassword, false)]
        [InlineData(TestHelpers.TestZupaTechEmail, TestHelpers.ValidPassword, true)]
        [InlineData(TestHelpers.NonExisitingZupaTechEmail, TestHelpers.ValidPassword, false)]
        [InlineData(TestHelpers.NonExisitingZupaTechEmail, TestHelpers.InvalidPassword, false)]
        [InlineData(TestHelpers.Test2ZupaTechEmail, TestHelpers.InvalidPassword, false)]
        [InlineData(TestHelpers.Test2ZupaTechEmail, TestHelpers.ValidPassword, true)]
        public async Task TestUserLoginWithEmailAndPasswordAsync(string email, string password, bool shouldSucceedLogin)
        {
            var data = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", _antiForgToken},
                {"Email", email},
                {"Password", password}
            };

            var requestMessage = TestHelpers.CreatePostRequestWithCookiesFromResponse("/Account/Login", data, _httpResponseMessage);

            var postResponse = await _client.SendAsync(requestMessage);

            if (shouldSucceedLogin)
            {
                postResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
            }
            else
            {
                postResponse.EnsureSuccessStatusCode();
                
                var responseHtml = await postResponse.Content.ReadAsStringAsync();
                responseHtml.Should().Contain("Invalid login attempt.");
            }
        }

        [Fact(DisplayName = "GIVEN too many requests from same ip address, WHEN a user POSTs from the same ip address, THEN the user get redirected to GET page.")]
        public async Task TooManyRequests_POSTWithSameIpAddress_RedirectUser()
        {
            var data = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", _antiForgToken},
                {"Email", TestHelpers.TestZupaTechEmail},
                {"Password", TestHelpers.InvalidPassword}
            };

            await AddFailedLoginAttempts(TestConstants.MaxRequestsCount);
            
            var requestMessage = TestHelpers.CreatePostRequestWithCookiesFromResponse("/Account/Login", data, _httpResponseMessage);
            var postResponse = await _client.SendAsync(requestMessage);
            
            postResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        }
        
        [Fact(DisplayName = "GIVEN there are NOT too many requests from same ip address, WHEN a user POSTs from the same ip address, THEN Ok is returned.")]
        public async Task NotTooManyRequests_POSTWithSameIpAddress_ReturnsOk()
        {
            var data = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", _antiForgToken},
                {"Email", TestHelpers.TestZupaTechEmail},
                {"Password", TestHelpers.InvalidPassword}
            };

            await AddFailedLoginAttempts(TestConstants.MaxRequestsCount / 2);

            var requestMessage = TestHelpers.CreatePostRequestWithCookiesFromResponse("/Account/Login", data, _httpResponseMessage);
            var postResponse = await _client.SendAsync(requestMessage);
            
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        
        [Fact(DisplayName = "GIVEN there are NOT too many requests from same ip address, WHEN a user GETs from the same ip address, THEN Login form is displayed.")]
        public async Task NotTooManyRequests_GETWithSameIpAddress_LoginFormDisplayed()
        {
            await AddFailedLoginAttempts(TestConstants.MaxRequestsCount / 2);

            var getResponse = _client.GetAsync("/Account/Login").Result;
            getResponse.EnsureSuccessStatusCode();
            
            var responseHtml = await getResponse.Content.ReadAsStringAsync();
            responseHtml.Should().Contain("form method=\"post\"");
        }
        
        [Fact(DisplayName = "GIVEN there are too many requests from same ip address, WHEN a user GETs from the same ip address, THEN too many requests message is returned.")]
        public async Task TooManyRequests_GETWithSameIpAddress_MessageDisplayed()
        {
            await AddFailedLoginAttempts(TestConstants.MaxRequestsCount + 2);

            var getResponse = _client.GetAsync("/Account/Login").Result;
            getResponse.EnsureSuccessStatusCode();
            
            var responseHtml = await getResponse.Content.ReadAsStringAsync();
            responseHtml.Should().Contain("There have been several failed attempts to sign in from this IP address. Please try again later.");
        }

        public void Dispose()
        {
            _client.Dispose();
            
            var batchOperation = new TableBatchOperation();
            var entries = new TableQuery<FailedAttemptEntity>()
                .Where(TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    TestConstants.FakeIpAddress)
                );
            
            foreach (var e in _fixtureTable.ExecuteQuery(entries))
                batchOperation.Delete(e);

            if (batchOperation.Count > 0)
                _fixtureTable.ExecuteBatch(batchOperation);
        }
        
        private async Task AddFailedLoginAttempts(int requestsCount)
        {
            for (var i = 0; i < requestsCount; i++)
            {
                var failedAttempt = new FailedAttemptEntity
                {
                    PartitionKey = TestConstants.FakeIpAddress,
                    RowKey = DateTimeOffset.Now.Ticks.ToString()
                };

                await _fixtureTable.ExecuteAsync(TableOperation.Insert(failedAttempt));
            }
        }
    }
}
