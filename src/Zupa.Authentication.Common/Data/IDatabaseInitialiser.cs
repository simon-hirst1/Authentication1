using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Zupa.Authentication.Common.Data
{
    public interface IDatabaseInitialiser
    {
        void Initialise(IApplicationBuilder app, IHostingEnvironment environment);
    }
}
