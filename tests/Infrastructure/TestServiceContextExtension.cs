using Microsoft.Extensions.DependencyInjection;

namespace Atis.DependencyInjection.Tests.Infrastructure
{
    internal class TestServiceContextExtension : IServiceContextExtension
    {
        public void AddServices(IServiceCollection services)
        {
            // intentionally empty — used only to affect cache key
        }
    }
}