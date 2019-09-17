using IdentityModel.Client;
using System;

namespace TokenClient
{
    internal class Program
    {
        private static void Main()
        {
            Console.Write($"Please enter the Authority URL, press enter to use dev default or QUIT to exit{Environment.NewLine}>> ");
            var authorityUrl = Console.ReadLine();

            while (!authorityUrl.ToLower().Equals("quit"))
            {
                Console.Write($"Please enter the space separated API Scope(s){Environment.NewLine}>> ");
                var apiScope = Console.ReadLine();

                var discoveryResponse = DiscoveryClient.GetAsync(string.IsNullOrWhiteSpace(authorityUrl) ? "http://localhost:2662" : authorityUrl).Result;
                if (discoveryResponse.IsError)
                {
                    Console.WriteLine(
                        $"ERROR: {discoveryResponse.Error}{Environment.NewLine}Press any key to close the application.");
                    Console.ReadKey();
                    return;
                }

                Console.Write($"Please enter the Client Secret or press enter for dev environment default{Environment.NewLine}>> ");
                var secret = Console.ReadLine();

                var tokenClient =
                    new IdentityModel.Client.TokenClient(discoveryResponse.TokenEndpoint, "TokenClient", string.IsNullOrWhiteSpace(secret) ? "secretToken" : secret);

                Console.Write($"Please enter your username or press enter to use dev default{Environment.NewLine}>> ");
                var username = Console.ReadLine();
                Console.Write($"Please enter your password or press enter to use dev default{Environment.NewLine}>> ");
                var password = Console.ReadLine();

                var tokenResponseWithClaim = tokenClient.RequestResourceOwnerPasswordAsync(
                    string.IsNullOrWhiteSpace(username) ? "test@zupa.co.uk" : username,
                    string.IsNullOrWhiteSpace(password) ? "Password0-" : password,
                    apiScope).Result;

                Console.WriteLine(tokenResponseWithClaim.IsError
                    ? $"ERROR: {tokenResponseWithClaim.Error}"
                    : $"{Environment.NewLine}{tokenResponseWithClaim.Json}{Environment.NewLine}");

                Console.Write($"Please enter the Authority URL or QUIT to exit{Environment.NewLine}>> ");
                authorityUrl = Console.ReadLine();
            }
        }
    }
}
