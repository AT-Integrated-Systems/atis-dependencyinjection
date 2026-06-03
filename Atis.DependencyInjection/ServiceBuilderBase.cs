using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Provides a base implementation for registering services into an <see cref="IServiceCollection"/>.
    ///         Inherit from this class to define the core services your component requires and how they are registered.
    ///     </para>
    /// </summary>
    public abstract class ServiceBuilderBase
    {
        /// <summary>
        ///     <para>
        ///         Gets the <see cref="IServiceCollection"/> that services are registered into.
        ///     </para>
        /// </summary>
        protected IServiceCollection ServiceCollection { get; }

        /// <summary>
        ///     <para>
        ///         Initializes a new instance of <see cref="ServiceBuilderBase"/>.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"><see cref="IServiceCollection"/> to register services into.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceCollection"/> is <c>null</c>.</exception>
        public ServiceBuilderBase(IServiceCollection serviceCollection)
        {
            this.ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        /// <summary>
        ///     <para>
        ///         Returns the <see cref="ServiceCharacteristic"/> for the specified service type.
        ///     </para>
        ///     <para>
        ///         Implement this method to describe the lifetime and registration behavior of each
        ///         service type your builder is responsible for.
        ///     </para>
        /// </summary>
        /// <param name="serviceType"><see cref="Type"/> of the service to retrieve characteristics for.</param>
        /// <returns><see cref="ServiceCharacteristic"/> describing the lifetime and registration behavior of the service.</returns>
        protected abstract ServiceCharacteristic GetServiceCharacteristic(Type serviceType);

        /// <summary>
        ///     <para>
        ///         Returns all service types that this builder is responsible for registering.
        ///     </para>
        ///     <para>
        ///         The types returned here are used by <see cref="ValidateCoreServicesAdded"/> to verify
        ///         that all required services have been registered into the <see cref="IServiceCollection"/>.
        ///     </para>
        /// </summary>
        /// <returns>A read-only collection of service <see cref="Type"/> instances this builder owns.</returns>
        protected abstract IReadOnlyCollection<Type> GetCoreServiceTypes();

        /// <summary>
        ///     <para>
        ///         Registers all core services into the <see cref="IServiceCollection"/>.
        ///     </para>
        ///     <para>
        ///         Implement this method to call <see cref="TryAdd(Type, Type)"/> or
        ///         <see cref="TryAdd(Type, Func{IServiceProvider, object})"/> for each service
        ///         your component requires.
        ///     </para>
        /// </summary>
        public abstract void AddCoreServices();

        /// <summary>
        ///     <para>
        ///         Tries to add the specified service type and implementation type to the <see cref="IServiceCollection"/>,
        ///         does nothing if the service type is already registered.
        ///     </para>
        /// </summary>
        /// <param name="serviceType"><see cref="Type"/> of the service.</param>
        /// <param name="implementationType"><see cref="Type"/> of the implementation.</param>
        /// <returns>The current <see cref="ServiceBuilderBase"/> instance for chaining.</returns>
        public virtual ServiceBuilderBase TryAdd(Type serviceType, Type implementationType)
            => InternalTryAdd(serviceType, implementationType, null);

        /// <summary>
        ///     <para>
        ///         Tries to add the specified service type and implementation factory to the <see cref="IServiceCollection"/>,
        ///         does nothing if the service type is already registered.
        ///     </para>
        /// </summary>
        /// <param name="serviceType"><see cref="Type"/> of the service.</param>
        /// <param name="implementationFactory">Factory function that creates the service implementation.</param>
        /// <returns>The current <see cref="ServiceBuilderBase"/> instance for chaining.</returns>
        public virtual ServiceBuilderBase TryAdd(Type serviceType, Func<IServiceProvider, object> implementationFactory)
            => InternalTryAdd(serviceType, null, implementationFactory);

        /// <summary>
        ///     <para>
        ///         Tries to add the specified service type and implementation type to the <see cref="IServiceCollection"/>,
        ///         does nothing if the service type is already registered.
        ///     </para>
        /// </summary>
        /// <typeparam name="TService">Type of the service.</typeparam>
        /// <typeparam name="TImplementation">Type of the implementation.</typeparam>
        /// <returns>The current <see cref="ServiceBuilderBase"/> instance for chaining.</returns>
        public virtual ServiceBuilderBase TryAdd<TService, TImplementation>()
            => this.TryAdd(typeof(TService), typeof(TImplementation));

        /// <summary>
        ///     <para>
        ///         Tries to add the specified service type and implementation factory to the <see cref="IServiceCollection"/>,
        ///         does nothing if the service type is already registered.
        ///     </para>
        /// </summary>
        /// <typeparam name="TService">Type of the service.</typeparam>
        /// <param name="implementationFactory">Factory function that creates the service implementation.</param>
        /// <returns>The current <see cref="ServiceBuilderBase"/> instance for chaining.</returns>
        public virtual ServiceBuilderBase TryAdd<TService>(Func<IServiceProvider, object> implementationFactory)
            => this.TryAdd(typeof(TService), implementationFactory);

        /// <summary>
        ///     <para>
        ///         Validates that all service types returned by <see cref="GetCoreServiceTypes"/> have been
        ///         registered into the <see cref="IServiceCollection"/>.
        ///     </para>
        /// </summary>
        /// <exception cref="CoreServicesNotInitializedException">
        ///     Thrown when one or more core service types have not been registered.
        /// </exception>
        public void ValidateCoreServicesAdded()
        {
            var coreServiceTypes = this.GetCoreServiceTypes();
            var servicesNotAdded = coreServiceTypes
                .Where(x => !this.ServiceCollection.Any(y => x == y.ServiceType))
                .ToArray();
            if (servicesNotAdded.Length > 0)
                throw new CoreServicesNotInitializedException(servicesNotAdded);
        }

        private ServiceBuilderBase InternalTryAdd(Type serviceType, Type implementationType, Func<IServiceProvider, object> implementationFactory)
        {
            if (serviceType is null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationType is null && implementationFactory is null)
                throw new ArgumentException($"At least one of '{nameof(implementationType)}' or '{nameof(implementationFactory)}' must be provided.");

            var serviceCharacteristic = this.GetServiceCharacteristic(serviceType);

            if (
                (serviceCharacteristic.AllowMultiple && !HasImplementationRegistered(serviceType, implementationType, implementationFactory))
                ||
                (!serviceCharacteristic.AllowMultiple && !HasServiceRegistered(serviceType))
               )
                if (implementationType != null)
                    this.ServiceCollection.Add(new ServiceDescriptor(serviceType, implementationType, serviceCharacteristic.Lifetime));
                else
                    this.ServiceCollection.Add(new ServiceDescriptor(serviceType, implementationFactory, serviceCharacteristic.Lifetime));

            return this;
        }

        private bool HasServiceRegistered(Type serviceType)
        {
            return this.ServiceCollection.Any(x => x.ServiceType == serviceType);
        }

        private bool HasImplementationRegistered(Type serviceType, Type implementationType, Func<IServiceProvider, object> implementationFactory)
        {
            if (implementationType != null)
                return this.ServiceCollection.Any(x => x.ServiceType == serviceType && x.ImplementationType == implementationType);
            else
                return this.ServiceCollection.Any(x => x.ServiceType == serviceType && x.ImplementationFactory == implementationFactory);
        }
    }
}