using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Provides a mechanism for retrieving a service provider from a cache or creating a new one if it does not exist.
    ///     </para>
    /// </summary>
    public abstract class ServiceManagerBase
    {
        /// <summary>
        ///     <para>
        ///         Gets the service provider cache.
        ///     </para>
        /// </summary>
        protected static readonly ConcurrentDictionary<int, IServiceProvider> ServiceProviderCache = new ConcurrentDictionary<int, IServiceProvider>();

        /// <summary>
        ///     <para>
        ///         Gets the service provider from the cache or creates a new one if it does not exist.
        ///     </para>
        /// </summary>
        /// <param name="serviceConfiguration"><see cref="IServiceContextConfiguration"/> instance.</param>
        /// <returns><see cref="IServiceProvider"/> instance.</returns>
        public IServiceProvider GetOrAdd(IServiceContextConfiguration serviceConfiguration)
        {
            var key = this.GetKey(serviceConfiguration);
            if (!ServiceProviderCache.TryGetValue(key, out var serviceProviderCache))
            {
                var serviceCollection = new ServiceCollection();
                if (serviceConfiguration?.Extensions != null)
                {
                    foreach (var extension in serviceConfiguration.Extensions)
                    {
                        extension.AddServices(serviceCollection);
                    }
                }

                var serviceBuilder = this.CreateServiceBuilder(serviceCollection);
                serviceBuilder.AddCoreServices();

                serviceBuilder.ValidateCoreServiceAdded();

                serviceProviderCache = serviceCollection.BuildServiceProvider();
                ServiceProviderCache[key] = serviceProviderCache;
            }
            return serviceProviderCache;
        }

        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="ServiceBuilderBase"/> instance.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"><see cref="IServiceCollection"/> instance.</param>
        /// <returns><see cref="ServiceBuilderBase"/> instance.</returns>
        protected abstract ServiceBuilderBase CreateServiceBuilder(IServiceCollection serviceCollection);

        /// <summary>
        ///     <para>
        ///         Gets the key for the service provider cache.
        ///     </para>
        /// </summary>
        /// <param name="config"><see cref="IServiceContextConfiguration"/> instance.</param>
        /// <returns>A hash code for the given <paramref name="config"/> parameter.</returns>
        protected virtual int GetKey(IServiceContextConfiguration config)
        {
            var hash = new HashCode();
            hash.Add(config.GetType());
            foreach (var extension in config.Extensions.OrderBy(x => x.GetType().Name))
            {
                hash.Add(extension.GetType());
            }
            return hash.ToHashCode();
        }
    }
}
