using FluentAssertions;
using System;
using Xunit;

namespace Atis.DependencyInjection.Tests
{
    public class CoreServicesNotInitializedExceptionTests
    {
        [Fact]
        public void Constructor_SetsServicesNotInitialized()
        {
            var missingTypes = new[] { typeof(IDisposable), typeof(ICloneable) };

            var exception = new CoreServicesNotInitializedException(missingTypes);

            exception.ServicesNotInitialized.Should().BeEquivalentTo(missingTypes);
        }

        [Fact]
        public void Constructor_MessageContainsFullTypeNames()
        {
            var missingTypes = new[] { typeof(IDisposable) };

            var exception = new CoreServicesNotInitializedException(missingTypes);

            exception.Message.Should().Contain(typeof(IDisposable).FullName);
        }

        [Fact]
        public void Constructor_MessageContainsAllMissingTypes()
        {
            var missingTypes = new[] { typeof(IDisposable), typeof(ICloneable) };

            var exception = new CoreServicesNotInitializedException(missingTypes);

            exception.Message.Should().Contain(typeof(IDisposable).FullName);
            exception.Message.Should().Contain(typeof(ICloneable).FullName);
        }
    }
}