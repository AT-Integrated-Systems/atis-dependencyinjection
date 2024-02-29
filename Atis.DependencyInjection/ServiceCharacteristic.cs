using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Defines the characteristics of a service registration.
    ///     </para>
    /// </summary>
    public readonly struct ServiceCharacteristic
    {
        /// <summary>
        ///     <para>
        ///         Gets the <see cref="ServiceLifetime"/> of the service registration.
        ///     </para>
        /// </summary>
        public ServiceLifetime Lifetime { get; }

        /// <summary>
        ///     <para>
        ///         Gets a value indicating whether multiple service registrations are allowed.
        ///     </para>
        /// </summary>
        public bool AllowMultiple { get; }

        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="ServiceCharacteristic"/> instance.
        ///     </para>
        /// </summary>
        /// <param name="lifetime"><see cref="ServiceLifetime"/> of the service registration.</param>
        /// <param name="allowMultiple">If <c>true</c> multiple service registrations are allowed.</param>
        public ServiceCharacteristic(ServiceLifetime lifetime, bool allowMultiple = false)
        {
            Lifetime = lifetime;
            AllowMultiple = allowMultiple;
        }
    }
}
