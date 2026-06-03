namespace Atis.DependencyInjection.Tests.Infrastructure
{
    internal interface IFakeService { }

    internal class FakeService : IFakeService { }

    internal class AnotherFakeService : IFakeService { }

    internal interface IFakeMultiService { }

    internal class FakeMultiService : IFakeMultiService { }

    internal class AnotherFakeMultiService : IFakeMultiService { }
}