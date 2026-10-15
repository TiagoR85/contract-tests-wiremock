using WireMock.Server;

namespace ContractTests.WireMock.Fixtures;

public sealed class WireMockFixture : IDisposable
{
    public WireMockServer Server { get; } = WireMockServer.Start();

    public Uri BaseUri => new(Server.Url + "/");

    public HttpClient CreateClient(TimeSpan? timeout = null) => new()
    {
        BaseAddress = BaseUri,
        Timeout = timeout ?? TimeSpan.FromSeconds(5)
    };

    public void Reset() => Server.Reset();

    public void Dispose() => Server.Dispose();
}

[CollectionDefinition("wiremock")]
public sealed class WireMockCollection : ICollectionFixture<WireMockFixture>;
