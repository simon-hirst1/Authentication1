using Xunit;
using Zupa.Authentication.AuthService.ComponentTests.Fixtures;

namespace Zupa.Authentication.AuthService.ComponentTests.Collections
{
    [CollectionDefinition(nameof(FailedAttemptsTableCollection))]
    public class FailedAttemptsTableCollection : ICollectionFixture<FailedAttemptsTableFixture> { }
}
