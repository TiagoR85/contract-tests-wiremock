using ContractTests.WireMock.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace ContractTests.WireMock.Tests;

[Collection("wiremock")]
public sealed class ScaffoldSmokeTests
{
    private readonly WireMockFixture _fixture;

    public ScaffoldSmokeTests(WireMockFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    [Fact]
    public async Task Fixture_serves_stubbed_ping()
    {
        _fixture.Server
            .Given(Request.Create().WithPath("/ping").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("pong"));

        using var http = _fixture.CreateClient();
        var body = await http.GetStringAsync("ping");

        Assert.Equal("pong", body);
    }
}
