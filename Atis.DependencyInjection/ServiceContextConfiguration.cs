using System;
using System.Collections.Generic;

namespace Atis.DependencyInjection
{
    /// <inheritdoc />
    public class ServiceContextConfiguration : IServiceContextConfiguration
    {
        private readonly Dictionary<Type, IServiceContextExtension> _extensions = new Dictionary<Type, IServiceContextExtension>();

        /// <inheritdoc />
        public IEnumerable<IServiceContextExtension> Extensions => this._extensions.Values;

        /// <inheritdoc />
        public void AddOrUpdateExtension(IServiceContextExtension extension)
        {
            if (extension is null)
                throw new ArgumentNullException(nameof(extension));
            this._extensions[extension.GetType()] = extension;
        }
    }
}