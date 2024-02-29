using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Defines a contract for adding services to the <see cref="IServiceCollection"/>.
    ///     </para>
    /// </summary>
    public interface IServiceContextExtension
    {
        /// <summary>
        ///     <para>
        ///         Adds services to the <see cref="IServiceCollection"/>.
        ///     </para>
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/> to add services to.</param>
        void AddServices(IServiceCollection services);
    }
}
