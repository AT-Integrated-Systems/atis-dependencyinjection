using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Provides a base implementation for managing a cache of <see cref="IServiceProvider"/> instances.
    ///         Inherit from this class to define how your component's service provider is built and reused.
    ///     </para>
    /// </summary>
    public abstract class ServiceManagerBase
    {
        private static readonly ConcurrentDictionary<int, IServiceProvider> _serviceProviderCache =
            new ConcurrentDictionary<int, IServiceProvider>();

        /// <summary>
        ///     <para>
        ///         Returns a cached <see cref="IServiceProvider"/> for the given <paramref name="serviceConfiguration"/>,
        ///         or builds and caches a new one if it does not exist yet.
        ///     </para>
        /// </summary>
        /// <param name="serviceConfiguration"><see cref="IServiceContextConfiguration"/> instance describing the service context.</param>
        /// <returns>A cached or newly built <see cref="IServiceProvider"/> instance.</returns>
        public IServiceProvider GetOrAdd(IServiceContextConfiguration serviceConfiguration)
        {
            var key = this.GetKey(serviceConfiguration);
            if (!_serviceProviderCache.TryGetValue(key, out var serviceProvider))
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
                serviceBuilder.ValidateCoreServicesAdded();

                serviceProvider = serviceCollection.BuildServiceProvider();
                _serviceProviderCache[key] = serviceProvider;
            }
            return serviceProvider;
        }

        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="ServiceBuilderBase"/> instance for the given <paramref name="serviceCollection"/>.
        ///     </para>
        ///     <para>
        ///         Implement this method to return your component's specific <see cref="ServiceBuilderBase"/> implementation.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"><see cref="IServiceCollection"/> to pass to the builder.</param>
        /// <returns>A new <see cref="ServiceBuilderBase"/> instance.</returns>
        protected abstract ServiceBuilderBase CreateServiceBuilder(IServiceCollection serviceCollection);

        /// <summary>
        ///     <para>
        ///         Computes a cache key for the given <paramref name="config"/>.
        ///     </para>
        ///     <para>
        ///         The default implementation hashes the configuration type and the full type names of all
        ///         registered extensions. Override this method to provide a custom caching strategy.
        ///     </para>
        /// </summary>
        /// <param name="config"><see cref="IServiceContextConfiguration"/> instance to compute the key for.</param>
        /// <returns>An integer hash code used as the cache key.</returns>
        protected virtual int GetKey(IServiceContextConfiguration config)
        {
            var hash = new HashCode();
            hash.Add(config.GetType());
            foreach (var extension in config.Extensions.OrderBy(x => x.GetType().FullName))
            {
                hash.Add(extension.GetType());
            }
            return hash.ToHashCode();
        }
    }
}