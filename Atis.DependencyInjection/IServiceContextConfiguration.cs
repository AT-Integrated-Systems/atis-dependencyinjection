using System;
using System.Collections.Generic;
using System.Text;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Used to inject / override services in the service collection.
    ///     </para>
    /// </summary>
    public interface IServiceContextConfiguration
    {
        /// <summary>
        ///     <para>
        ///         Gets the collection of service extensions.
        ///     </para>
        /// </summary>
        IEnumerable<IServiceContextExtension> Extensions { get; }
        /// <summary>
        ///     <para>
        ///         Adds or updates the service extension.
        ///     </para>
        /// </summary>
        /// <param name="extension"></param>
        void AddOrUpdateExtension(IServiceContextExtension extension);
    }
}
