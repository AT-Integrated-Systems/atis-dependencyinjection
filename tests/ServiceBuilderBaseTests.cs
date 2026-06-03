using Atis.DependencyInjection.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Atis.DependencyInjection.Tests
{
    public class ServiceBuilderBaseTests
    {
        private IServiceCollection NewCollection() => new ServiceCollection();

        [Fact]
        public void TryAdd_WhenServiceNotRegistered_RegistersService()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);

            builder.TryAdd<IFakeService, FakeService>();

            services.Should().ContainSingle(x => x.ServiceType == typeof(IFakeService));
        }

        [Fact]
        public void TryAdd_WhenServiceAlreadyRegistered_DoesNotRegisterAgain()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);

            builder.TryAdd<IFakeService, FakeService>();
            builder.TryAdd<IFakeService, FakeService>();

            services.Should().ContainSingle(x => x.ServiceType == typeof(IFakeService));
        }

        [Fact]
        public void TryAdd_WhenAllowMultiple_RegistersMultipleImplementations()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);

            builder.TryAdd<IFakeMultiService, FakeMultiService>();
            builder.TryAdd<IFakeMultiService, AnotherFakeMultiService>();

            services.Should().HaveCount(2, "because AllowMultiple is true for IFakeMultiService");
        }

        [Fact]
        public void TryAdd_WhenAllowMultiple_DoesNotRegisterSameImplementationTwice()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);

            builder.TryAdd<IFakeMultiService, FakeMultiService>();
            builder.TryAdd<IFakeMultiService, FakeMultiService>();

            services.Should().ContainSingle(x => x.ServiceType == typeof(IFakeMultiService));
        }

        [Fact]
        public void TryAdd_RegistersCorrectLifetime()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);

            builder.TryAdd<IFakeService, FakeService>();

            services.Should().ContainSingle(x =>
                x.ServiceType == typeof(IFakeService) &&
                x.Lifetime == ServiceLifetime.Transient);
        }

        [Fact]
        public void TryAdd_WithNullServiceType_ThrowsArgumentNullException()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);

            Action act = () => builder.TryAdd(null, typeof(FakeService));

            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("serviceType");
        }

        [Fact]
        public void TryAdd_WithNullImplementation_ThrowsArgumentException()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);

            Action act = () => builder.TryAdd(typeof(IFakeService), (Type)null);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryAdd_ReturnsSameBuilderInstance_ForChaining()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);

            var result = builder.TryAdd<IFakeService, FakeService>();

            result.Should().BeSameAs(builder);
        }

        [Fact]
        public void AddCoreServices_RegistersAllCoreServices()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);

            builder.AddCoreServices();

            services.Should().Contain(x => x.ServiceType == typeof(IFakeService));
            services.Should().Contain(x => x.ServiceType == typeof(IFakeMultiService));
        }

        [Fact]
        public void ValidateCoreServicesAdded_WhenAllRegistered_DoesNotThrow()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);
            builder.AddCoreServices();

            Action act = () => builder.ValidateCoreServicesAdded();

            act.Should().NotThrow();
        }

        [Fact]
        public void ValidateCoreServicesAdded_WhenServiceMissing_ThrowsCoreServicesNotInitializedException()
        {
            var services = NewCollection();
            var builder = new TestServiceBuilder(services);
            // deliberately skip AddCoreServices

            Action act = () => builder.ValidateCoreServicesAdded();

            act.Should().Throw<CoreServicesNotInitializedException>()
                .Which.ServicesNotInitialized.Should().Contain(typeof(IFakeService));
        }
    }
}