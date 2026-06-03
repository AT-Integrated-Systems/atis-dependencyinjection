using Atis.DependencyInjection.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Atis.DependencyInjection.Tests
{
    public class ServiceManagerBaseTests
    {
        [Fact]
        public void GetOrAdd_ReturnsServiceProvider()
        {
            var manager = new TestServiceManager();
            var config = new ServiceContextConfiguration();

            var provider = manager.GetOrAdd(config);

            provider.Should().NotBeNull();
        }

        [Fact]
        public void GetOrAdd_WithSameConfiguration_ReturnsCachedInstance()
        {
            var manager = new TestServiceManager();
            var config = new ServiceContextConfiguration();

            var provider1 = manager.GetOrAdd(config);
            var provider2 = manager.GetOrAdd(config);

            provider2.Should().BeSameAs(provider1, "because same configuration should return cached provider");
        }

        [Fact]
        public void GetOrAdd_WithDifferentExtensions_ReturnsDifferentProviders()
        {
            var manager = new TestServiceManager();

            var config1 = new ServiceContextConfiguration();
            var config2 = new ServiceContextConfiguration();
            config2.AddOrUpdateExtension(new TestServiceContextExtension());

            var provider1 = manager.GetOrAdd(config1);
            var provider2 = manager.GetOrAdd(config2);

            provider2.Should().NotBeSameAs(provider1, "because different extensions produce different providers");
        }

        [Fact]
        public void GetOrAdd_CanResolveRegisteredService()
        {
            var manager = new TestServiceManager();
            var config = new ServiceContextConfiguration();

            var provider = manager.GetOrAdd(config);
            var service = provider.GetService(typeof(IFakeService));

            service.Should().NotBeNull();
            service.Should().BeAssignableTo<IFakeService>();
        }
    }
}