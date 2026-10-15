using System.Net;
using ContractTests.WireMock.Clients;
using ContractTests.WireMock.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace ContractTests.WireMock.Tests;

[Collection("wiremock")]
public sealed class HttpBinTests
{
    private readonly WireMockFixture _fixture;

    public HttpBinTests(WireMockFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    private void StubGet(string body, HttpStatusCode status = HttpStatusCode.OK, TimeSpan? delay = null)
    {
        var response = Response.Create().WithStatusCode((int)status).WithBody(body);
        if (delay is not null)
            response = response.WithDelay(delay.Value);
        _fixture.Server
            .Given(Request.Create().WithPath("/get").UsingGet())
            .RespondWith(response);
    }

    [Fact]
    public async Task Get_success_validates_contract_and_returns_result()
    {
        StubGet("""{"args":{},"origin":"203.0.113.7","url":"https://httpbin.org/get"}""");

        var result = await new HttpBinClient(_fixture.CreateClient()).GetAsync();

        Assert.Equal("203.0.113.7", result.Origin);
        Assert.Equal("https://httpbin.org/get", result.Url);
    }

    [Fact]
    public async Task Get_timeout_throws_upstream_timeout()
    {
        StubGet("""{"origin":"203.0.113.7","url":"https://httpbin.org/get"}""",
            delay: TimeSpan.FromSeconds(3));

        var ex = await Assert.ThrowsAsync<UpstreamTimeoutException>(
            () => new HttpBinClient(_fixture.CreateClient(TimeSpan.FromMilliseconds(400))).GetAsync());

        Assert.Contains("timeout", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Get_500_throws_upstream_server_with_status()
    {
        StubGet("""{"message":"internal error"}""", HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<UpstreamServerException>(
            () => new HttpBinClient(_fixture.CreateClient()).GetAsync());

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public async Task Get_schema_drift_throws_contract_violation_missing_url()
    {
        StubGet("""{"origin":"203.0.113.7"}""");

        var ex = await Assert.ThrowsAsync<ContractViolationException>(
            () => new HttpBinClient(_fixture.CreateClient()).GetAsync());

        Assert.Contains("url", ex.Message);
    }
}
