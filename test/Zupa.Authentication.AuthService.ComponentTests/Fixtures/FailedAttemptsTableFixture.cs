using Microsoft.Azure.Cosmos.Table;
using System.Threading.Tasks;
using Xunit;

namespace Zupa.Authentication.AuthService.ComponentTests.Fixtures
{
    public class FailedAttemptsTableFixture : IAsyncLifetime
    {
        public CloudTable Table { get; }

        public FailedAttemptsTableFixture()
        {
            var storageAccount =
                CloudStorageAccount.Parse(TestConstants.FailedAttemptsConnectionString);

            var tableClient = storageAccount.CreateCloudTableClient();

            Table = tableClient.GetTableReference(TestConstants.FailedAttemptsTableName);
        }

        public async Task InitializeAsync() => await Table.CreateIfNotExistsAsync();

        public async Task DisposeAsync() => await Table.DeleteIfExistsAsync();
    }
}
