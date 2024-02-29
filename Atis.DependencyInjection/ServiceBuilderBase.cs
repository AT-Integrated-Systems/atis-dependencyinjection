using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Provides a convenient way to add services to the <see cref="IServiceCollection"/>.
    ///     </para>
    /// </summary>
    public abstract class ServiceBuilderBase
    {
        /// <summary>
        ///     <para>
        ///         Gets the <see cref="IServiceCollection"/> to add services to.
        ///     </para>
        /// </summary>
        protected IServiceCollection ServiceCollection { get; }

        /// <summary>
        ///     <para>
        ///         Creates a new instance of <see cref="ServiceBuilderBase"/>.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"><see cref="IServiceCollection"/> to add services to.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ServiceBuilderBase(IServiceCollection serviceCollection)
        {
            this.ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        }

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="ServiceCharacteristic"/> for the specified service type.
        ///     </para>
        /// </summary>
        /// <param name="serviceType"><see cref="Type"/> of the service.</param>
        /// <returns><see cref="ServiceCharacteristic"/> for the specified service type.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        protected virtual ServiceCharacteristic GetServiceInfo(Type serviceType)
        {
            if (!CoreServices.TryGetValue(serviceType, out var serviceCharacteristic))
                throw new InvalidOperationException($"ServiceCharacteristic is not defined for Service type '{serviceType}'");

            return serviceCharacteristic;
        }

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="ServiceCharacteristic"/> for the specified service type.
        ///     </para>
        /// </summary>
        public abstract IDictionary<Type, ServiceCharacteristic> CoreServices { get; }
        /// <summary>
        ///     <para>
        ///         Adds the core services to the <see cref="IServiceCollection"/>.
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
        /// <returns><see cref="ServiceBuilderBase"/> instance for chaining.</returns>
        public virtual ServiceBuilderBase TryAdd(Type serviceType, Type implementationType)
            => InternalTryAdd(serviceType, implementationType, null);

        /// <summary>
        ///     <para>
        ///         Tries to add the specified service type and implementation factory to the <see cref="IServiceCollection"/>,
        ///         does nothing if the service type is already registered.
        ///     </para>
        /// </summary>
        /// <param name="serviceType"><see cref="Type"/> of the service.</param>
        /// <param name="implementationFactory">Function that returns the implementation.</param>
        /// <returns><see cref="ServiceBuilderBase"/> instance for chaining.</returns>
        public virtual ServiceBuilderBase TryAdd(Type serviceType, Func<IServiceProvider, object> implementationFactory)
            => InternalTryAdd(serviceType, null, implementationFactory);
        
        /// <summary>
        ///     <para>
        ///         Tries to add the specified service type and implementation type to the <see cref="IServiceCollection"/>,
        ///         does nothing if the service type is already registered.
        ///     </para>
        /// </summary>
        /// <typeparam name="IServiceType">Type of the service.</typeparam>
        /// <typeparam name="IImplementationType">Type of the implementation.</typeparam>
        /// <returns><see cref="ServiceBuilderBase"/> instance for chaining.</returns>
        public virtual ServiceBuilderBase TryAdd<IServiceType, IImplementationType>()
            => this.TryAdd(typeof(IServiceType), typeof(IImplementationType));

        /// <summary>
        ///     <para>
        ///         Tries to add the specified service type and implementation factory to the <see cref="IServiceCollection"/>,
        ///         does nothing if the service type is already registered.
        ///     </para>
        /// </summary>
        /// <typeparam name="IServiceType">Type of the service.</typeparam>
        /// <param name="implementationFactory">Function that returns the implementation.</param>
        /// <returns><see cref="ServiceBuilderBase"/> instance for chaining.</returns>
        public virtual ServiceBuilderBase TryAdd<IServiceType>(Func<IServiceProvider, object> implementationFactory)
            => this.TryAdd(typeof(IServiceType), implementationFactory);

        /// <summary>
        ///     <para>
        ///         Core method to add the specified service type and implementation type or implementation factory to the <see cref="IServiceCollection"/>.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         At-least one of the <paramref name="implementationType"/> or <paramref name="implementationFactory"/> must be provided,
        ///         otherwise an <see cref="ArgumentNullException"/> will be thrown.
        ///     </para>
        ///     <para>
        ///         Throws <see cref="ArgumentNullException"/> if <paramref name="serviceType"/> is null.
        ///     </para>
        /// </remarks>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="implementationFactory">Function that returns the implementation.</param>
        /// <returns><see cref="ServiceBuilderBase"/> instance for chaining.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual ServiceBuilderBase InternalTryAdd(Type serviceType, Type implementationType, Func<IServiceProvider, object> implementationFactory)
        {
            if (serviceType is null)
                throw new ArgumentNullException(nameof(serviceType));
            if (implementationType is null && implementationFactory is null)
                throw new ArgumentNullException($"Both '{nameof(implementationType)}' and '{nameof(implementationFactory)}' arguments are null, at-least one of them must be provided");

            var serviceCharacteristic = this.GetServiceInfo(serviceType);

            if (
                (serviceCharacteristic.AllowMultiple && !this.HasImplementationRegistered(serviceType, implementationType, implementationFactory))
                ||
                (!serviceCharacteristic.AllowMultiple && !this.HasServiceRegistered(serviceType))
                )
                if (implementationType != null)
                    this.ServiceCollection.Add(new ServiceDescriptor(serviceType, implementationType, serviceCharacteristic.Lifetime));
                else
                    this.ServiceCollection.Add(new ServiceDescriptor(serviceType, implementationFactory, serviceCharacteristic.Lifetime));

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Determines whether the specified service type is already registered in the <see cref="IServiceCollection"/>.
        ///     </para>
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns><c>true</c> if the service type is already registered; otherwise, <c>false</c>.</returns>
        protected virtual bool HasServiceRegistered(Type serviceType)
        {
            return this.ServiceCollection.Any(x => x.ServiceType == serviceType);
        }

        /// <summary>
        ///     <para>
        ///         Determines whether the specified service type and implementation type or implementation factory is already registered in the <see cref="IServiceCollection"/>.
        ///     </para>
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="implementationFactory">Function that returns the implementation.</param>
        /// <returns><c>true</c> if the service type and implementation type or implementation factory is already registered; otherwise, <c>false</c>.</returns>
        protected virtual bool HasImplementationRegistered(Type serviceType, Type implementationType, Func<IServiceProvider, object> implementationFactory)
        {
            if (implementationType != null)
                return this.ServiceCollection.Any(x => x.ServiceType == serviceType && x.ImplementationType == implementationType);
            else
                return this.ServiceCollection.Any(x => x.ServiceType == serviceType && x.ImplementationFactory == implementationFactory);
        }

        /// <summary>
        ///     <para>
        ///         Validates that all core services are added to the <see cref="IServiceCollection"/>.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Throws <see cref="CoreServicesNotInitializedException"/> if any of the core services are not added.
        ///     </para>
        /// </remarks>
        /// <exception cref="CoreServicesNotInitializedException"></exception>
        public void ValidateCoreServiceAdded()
        {
            var coreServices = this.CoreServices.Keys.ToArray();
            var servicesNotAdded = coreServices.Where(x => !this.ServiceCollection.Any(y => x == y.ServiceType)).ToArray();
            if (servicesNotAdded.Length > 0)
                throw new CoreServicesNotInitializedException(servicesNotAdded);
        }
    }
}
