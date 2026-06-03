using Atis.DependencyInjection.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Atis.DependencyInjection.Tests.Infrastructure
{
    internal class TestServiceBuilder : ServiceBuilderBase
    {
        private static readonly Dictionary<Type, ServiceCharacteristic> _serviceCharacteristics
            = new Dictionary<Type, ServiceCharacteristic>
            {
                { typeof(IFakeService),      new ServiceCharacteristic(ServiceLifetime.Transient, allowMultiple: false) },
                { typeof(IFakeMultiService), new ServiceCharacteristic(ServiceLifetime.Singleton, allowMultiple: true)  },
            };

        public TestServiceBuilder(IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
        }

        protected override ServiceCharacteristic GetServiceCharacteristic(Type serviceType)
        {
            if (!_serviceCharacteristics.TryGetValue(serviceType, out var characteristic))
                throw new InvalidOperationException($"No characteristic defined for service type '{serviceType.FullName}'.");
            return characteristic;
        }

        protected override IReadOnlyCollection<Type> GetCoreServiceTypes()
        {
            return _serviceCharacteristics.Keys;
        }

        public override void AddCoreServices()
        {
            this.TryAdd<IFakeService, FakeService>();
            this.TryAdd<IFakeMultiService, FakeMultiService>();
        }
    }
}