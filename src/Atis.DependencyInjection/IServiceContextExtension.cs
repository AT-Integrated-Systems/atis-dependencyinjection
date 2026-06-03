using Microsoft.Extensions.DependencyInjection;

namespace Atis.DependencyInjection
{
    /// <summary>
    ///     <para>
    ///         Defines a contract for contributing services to an
    ///         <see cref="IServiceCollection"/> as part of a service context configuration.
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