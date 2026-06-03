using Atis.DependencyInjection.Tests.Infrastructure;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace Atis.DependencyInjection.Tests
{
	public class ServiceContextConfigurationTests
	{
		[Fact]
		public void AddOrUpdateExtension_AddsNewExtension()
		{
			var config = new ServiceContextConfiguration();
			var extension = new TestServiceContextExtension();

			config.AddOrUpdateExtension(extension);

			config.Extensions.Should().ContainSingle();
		}

		[Fact]
		public void AddOrUpdateExtension_WithSameType_UpdatesExistingExtension()
		{
			var config = new ServiceContextConfiguration();
			var first = new TestServiceContextExtension();
			var second = new TestServiceContextExtension();

			config.AddOrUpdateExtension(first);
			config.AddOrUpdateExtension(second);

			config.Extensions.Should().ContainSingle("because same type replaces existing extension");
			config.Extensions.First().Should().BeSameAs(second);
		}

		[Fact]
		public void AddOrUpdateExtension_WithNull_ThrowsArgumentNullException()
		{
			var config = new ServiceContextConfiguration();

			Action act = () => config.AddOrUpdateExtension(null);

			act.Should().Throw<ArgumentNullException>()
				.WithParameterName("extension");
		}

		[Fact]
		public void Extensions_WhenEmpty_ReturnsEmptyCollection()
		{
			var config = new ServiceContextConfiguration();

			config.Extensions.Should().BeEmpty();
		}
	}
}