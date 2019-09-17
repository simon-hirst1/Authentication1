using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.InformationModels;
using Zupa.Authentication.AuthService.Models.Entity;
using Zupa.Libraries.CosmosTableStorageClient;

namespace Zupa.Authentication.AuthService.Data
{
    public class WhitelistRepository : IWhitelistRepository
    {
        private readonly ICosmosTableStorageClient<WhitelistEntity> _storageClient;
        private readonly ICosmosTableQuery<TableQuery<WhitelistEntity>> _storageQuery;

        public WhitelistRepository(
            ICosmosTableStorageClient<WhitelistEntity> storageClient,
            ICosmosTableQuery<TableQuery<WhitelistEntity>> storageQuery)
        {
            _storageClient = storageClient;
            _storageQuery = storageQuery;
        }

        public Task<IEnumerable<WhitelistEntity>> FindAllByEmailAsync(string emailaddress)
        {
            var query = _storageQuery
                .Where(TableQuery.GenerateFilterCondition(nameof(WhitelistEntity.EmailAddress), QueryComparisons.Equal, emailaddress));
            return _storageClient.RunQueryAsync(query);
        }

        public async Task InsertAsync(InviteInformation invite)
        {
            var entity = new WhitelistEntity
            {
                EmailAddress = invite.EmailAddress,
                PartitionKey = invite.Id.ToString(),
                RowKey = invite.Id.ToString()
            };
            await _storageClient.ExecuteAsync(TableOperation.Insert(entity));
        }

        public async Task RemoveAsync(string inviteId)
        {
            var entity = await _storageClient.ExecuteAsync(TableOperation.Retrieve<WhitelistEntity>(inviteId, inviteId));
            if (entity.HttpStatusCode == 404) return;

            await _storageClient.ExecuteAsync(TableOperation.Delete((WhitelistEntity)entity.Result));
        }
    }
}
