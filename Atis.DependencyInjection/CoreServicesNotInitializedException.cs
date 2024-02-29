using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Represents an exception that is thrown when core services are not initialized.
    ///     </para>
    /// </summary>
    public class CoreServicesNotInitializedException : Exception
    {
        /// <summary>
        ///     <para>
        ///         Gets the list of core services that were not initialized.
        ///     </para>
        /// </summary>
        public Type[] ServicesNotInitialized { get; }

        /// <summary>
        ///     <para>
        ///         Creates new instance of <see cref="CoreServicesNotInitializedException"/> class.
        ///     </para>
        /// </summary>
        /// <param name="servicesNotInitialized">List of core services that were not initialized.</param>
        public CoreServicesNotInitializedException(Type[] servicesNotInitialized)
            : base($"These core services were not initialized: {string.Join(", ", servicesNotInitialized?.Select(x => x.Name).ToArray())}")
        {
            this.ServicesNotInitialized = servicesNotInitialized;
        }
    }
}
