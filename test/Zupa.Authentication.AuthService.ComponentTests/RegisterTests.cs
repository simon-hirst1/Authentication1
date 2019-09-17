using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Zupa.Authentication.AuthService.ComponentTests
{
    public class RegisterTests : IDisposable
    {
        private readonly HttpResponseMessage _httpResponseMessage;
        private readonly string _antiForgToken;
        private readonly HttpClient _client;

        public RegisterTests()
        {
            _client = TestHelpers.GetTestClient();

            _httpResponseMessage = _client.GetAsync("/Account/Register").Result;
            _httpResponseMessage.EnsureSuccessStatusCode();
            
            _antiForgToken = TestHelpers.ExtractAntiForgeryTokenFromResponseAsync(_httpResponseMessage).Result;
        }
        
        [Theory(DisplayName = "Registration with Zupa accounts")]
        [InlineData(TestHelpers.NewZupaTechEmail, "test1", TestHelpers.ValidPassword, true)]
        [InlineData(TestHelpers.NewZupaTechEmail2, "test2", TestHelpers.ValidPassword, true)]
        [InlineData(TestHelpers.InvalidEmail, "test3", TestHelpers.InvalidPassword, false)]
       
        public async Task TestUserRegistrationWithZupaAccount(string email, string username, string password, bool shouldSucceedRegistration)
        {
            var data = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", _antiForgToken},
                {"Email", email},
                {"Username", username},
                {"Password", password},
                {"ConfirmPassword", password}
            };

            var requestMessage = TestHelpers.CreatePostRequestWithCookiesFromResponse("/Account/Register", data, _httpResponseMessage);

            var postResponse = await _client.SendAsync(requestMessage);

            postResponse.EnsureSuccessStatusCode();

            if (!shouldSucceedRegistration)
            { 
                var responseHtml = await postResponse.Content.ReadAsStringAsync();
                responseHtml.Should().Contain("The Email field is not a valid e-mail address.");
            }
        }

        [Theory(DisplayName = "Should not register with an existing account")]
        [InlineData(TestHelpers.TestZupaTechEmail, TestHelpers.TestZupaTechEmail, TestHelpers.ValidPassword)]
        public async Task ShouldNotRegisterWithExistingAccount(string email,string username,string password)
        {
            var data = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", _antiForgToken},
                {"Email", email},
                {"Username", username},
                {"Password", password},
                {"ConfirmPassword", password}
            };

            var requestMessage = TestHelpers.CreatePostRequestWithCookiesFromResponse("/Account/Register", data, _httpResponseMessage);

            var postResponse = await _client.SendAsync(requestMessage);

            postResponse.EnsureSuccessStatusCode();

            var responseHtml = await postResponse.Content.ReadAsStringAsync();
            responseHtml.Should().Contain("is already in use");
        }

        [Theory(DisplayName = 
            "Given a user tries to register an account when they enter a non zupa email then the registration should not be allowed.`")]
        [InlineData("test@test.com", "Test User", TestHelpers.ValidPassword)]
        [InlineData("test@zupatech.com", "Test User", TestHelpers.ValidPassword)]
        [InlineData("test@zupa.com", "Test User", TestHelpers.ValidPassword)]
        public async Task Register_NotZupatechEmail_RegistrationNotAllowed(string email, string username, string password)
        {
            var data = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", _antiForgToken},
                {"Email", email},
                {"Username", username},
                {"Password", password},
                {"ConfirmPassword", password}
            };

            var requestMessage = TestHelpers.CreatePostRequestWithCookiesFromResponse("/Account/Register", data, _httpResponseMessage);

            var postResponse = await _client.SendAsync(requestMessage);

            postResponse.EnsureSuccessStatusCode();

            var responseHtml = await postResponse.Content.ReadAsStringAsync();
            responseHtml.Should().Contain("Registration is currently limited.");
        }

        [Theory(DisplayName =
            "Given a user tries to register an account when they enter a zupatech email then the registration should be allowed.`")]
        [InlineData("someone@zupa.co.uk", "TestUser", TestHelpers.ValidPassword)]
        [InlineData("test._%+-!#$&'*/=?^`{|}~@zupa.co.uk", "TestUser2", TestHelpers.ValidPassword)]
        public async Task Register_ZupatechEmail_RegistrationAllowed(string email, string username, string password)
        {
            var data = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", _antiForgToken},
                {"Email", email},
                {"Username", username},
                {"Password", password},
                {"ConfirmPassword", password}
            };

            var requestMessage = TestHelpers.CreatePostRequestWithCookiesFromResponse("/Account/Register", data, _httpResponseMessage);

            var postResponse = await _client.SendAsync(requestMessage);

            postResponse.EnsureSuccessStatusCode();

            var responseHtml = await postResponse.Content.ReadAsStringAsync();
            responseHtml.Should().NotContain("Registration is currently limited.");
        }

        public void Dispose() => _client.Dispose();
    }
}
