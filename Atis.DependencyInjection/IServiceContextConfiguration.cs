using System.Collections.Generic;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Defines a contract for configuring a service context by registering
    ///         <see cref="IServiceContextExtension"/> instances that contribute services
    ///         to the <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
    ///     </para>
    /// </summary>
    public interface IServiceContextConfiguration
    {
        /// <summary>
        ///     <para>
        ///         Gets the collection of registered service extensions.
        ///     </para>
        /// </summary>
        IEnumerable<IServiceContextExtension> Extensions { get; }

        /// <summary>
        ///     <para>
        ///         Adds a new extension or replaces an existing one of the same type.
        ///     </para>
        /// </summary>
        /// <param name="extension">The <see cref="IServiceContextExtension"/> to add or replace.</param>
        void AddOrUpdateExtension(IServiceContextExtension extension);
    }
}