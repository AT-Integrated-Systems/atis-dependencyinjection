using System;
using System.Linq;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         The exception that is thrown when one or more core services have not been
    ///         registered into the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>
    ///         during service builder initialization.
    ///     </para>
    /// </summary>
    public class CoreServicesNotInitializedException : Exception
    {
        /// <summary>
        ///     <para>
        ///         Gets the list of core service types that were not registered.
        ///     </para>
        /// </summary>
        public Type[] ServicesNotInitialized { get; }

        /// <summary>
        ///     <para>
        ///         Creates a new instance of <see cref="CoreServicesNotInitializedException"/>.
        ///     </para>
        /// </summary>
        /// <param name="servicesNotInitialized">Array of service types that were not registered.</param>
        public CoreServicesNotInitializedException(Type[] servicesNotInitialized)
            : base($"These core services were not initialized: {string.Join(", ", servicesNotInitialized?.Select(x => x.FullName).ToArray())}")
        {
            this.ServicesNotInitialized = servicesNotInitialized;
        }
    }
}