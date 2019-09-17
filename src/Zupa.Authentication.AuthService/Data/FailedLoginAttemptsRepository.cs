using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zupa.Authentication.AuthService.Models.Account;
using Zupa.Authentication.AuthService.Models.Account.Entities;
using Zupa.Libraries.CosmosTableStorageClient;

namespace Zupa.Authentication.AuthService.Data
{
    public class FailedLoginAttemptsRepository : IFailedLoginAttemptsRepository
    {
        private readonly ICosmosTableStorageClient<FailedAttemptEntity> _storageClient;
        private readonly ICosmosTableCommand<FailedAttemptEntity> _storageCommand;
        private readonly ICosmosTableQuery<TableQuery<FailedAttemptEntity>> _storageQuery;

        public FailedLoginAttemptsRepository(
            ICosmosTableStorageClient<FailedAttemptEntity> storageClient,
            ICosmosTableCommand<FailedAttemptEntity> storageCommand,
            ICosmosTableQuery<TableQuery<FailedAttemptEntity>> storageQuery
        ) {
            _storageClient = storageClient;
            _storageCommand = storageCommand ;
            _storageQuery = storageQuery;
        }
        
        public async Task<List<FailedAttempt>> FindFailedAttemptsForIpAddressAsync(string ipAddress, DateTimeOffset newerThan)
        {
            var query = _storageQuery
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ipAddress))
                .And(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, newerThan.Ticks.ToString()));
            
            var ipAddresses = await _storageClient.RunQueryAsync(query);
            
            return ipAddresses.Select(ip => new FailedAttempt(
                ip.PartitionKey,
                ip.Timestamp)
            ).ToList();
        }

        public async Task PersistFailedAttemptAsync(FailedAttempt failedAttempt)
        { 
            var failedAttemptToPersist = new FailedAttemptEntity
            {
                PartitionKey = failedAttempt.IpAddress,
                RowKey = failedAttempt.FailedAt.Ticks.ToString()
            };
              
            var command = _storageCommand.AddCommand(failedAttemptToPersist);

            try
            {
                await _storageClient.ExecuteCommandAsync(command);
            }
            catch (StorageException exception) when (exception.RequestInformation.HttpStatusCode == 409)
            {
                throw new FailedAttemptConflictException("Record already exists in the table.", exception);
            }
        }

        public async Task RemoveAnyOldEntries(string ipAddress, DateTimeOffset olderThan)
        {
            var query = _storageQuery
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ipAddress))
                .And(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, olderThan.Ticks.ToString()));
            
            var olderIpAddresses = await _storageClient.RunQueryAsync(query);
            if (!olderIpAddresses.Any())
                return;

            var command = _storageCommand.DeleteBatchCommand(olderIpAddresses);
            await _storageClient.ExecuteCommandAsync(command);
        }
    }
}
