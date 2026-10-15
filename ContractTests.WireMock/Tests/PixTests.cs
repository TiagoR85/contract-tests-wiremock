using System.Net;
using ContractTests.WireMock.Clients;
using ContractTests.WireMock.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace ContractTests.WireMock.Tests;

[Collection("wiremock")]
public sealed class PixTests
{
    private const string GoodResponse = """
        {"cobId":"cob-0001","status":"ATIVA","chave":"fistful@of.dollars",
         "calendario":{"expiracao":3600},"valor":{"original":"12.50"}}
        """;

    private readonly WireMockFixture _fixture;

    public PixTests(WireMockFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    private void StubCob(string body, HttpStatusCode status = HttpStatusCode.OK, TimeSpan? delay = null)
    {
        var response = Response.Create().WithStatusCode((int)status).WithBody(body);
        if (delay is not null)
            response = response.WithDelay(delay.Value);
        _fixture.Server
            .Given(Request.Create().WithPath("/v2/cob").UsingPost())
            .RespondWith(response);
    }

    [Fact]
    public async Task CriarCob_success_sends_idempotency_key_and_returns_cob()
    {
        StubCob(GoodResponse);

        var cob = await new PixClient(_fixture.CreateClient())
            .CriarCobAsync(new CobRequest(12.50m, "fistful@of.dollars"), "key-123");

        Assert.Equal("cob-0001", cob.CobId);
        Assert.Equal("ATIVA", cob.Status);
        Assert.Equal("fistful@of.dollars", cob.Chave);

        var entry = _fixture.Server.LogEntries.Single(e => e.RequestMessage!.Path == "/v2/cob");
        Assert.Equal("key-123", entry.RequestMessage!.Headers!["Idempotency-Key"]?.First());
        Assert.Contains("fistful@of.dollars", entry.RequestMessage.Body);
    }

    [Fact]
    public async Task CriarCob_timeout_throws_upstream_timeout()
    {
        StubCob(GoodResponse, delay: TimeSpan.FromSeconds(3));

        var ex = await Assert.ThrowsAsync<UpstreamTimeoutException>(
            () => new PixClient(_fixture.CreateClient(TimeSpan.FromMilliseconds(400)))
                .CriarCobAsync(new CobRequest(12.50m, "fistful@of.dollars")));

        Assert.Contains("timeout", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CriarCob_500_throws_upstream_server_with_status()
    {
        StubCob("""{"message":"boom"}""", HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<UpstreamServerException>(
            () => new PixClient(_fixture.CreateClient())
                .CriarCobAsync(new CobRequest(12.50m, "fistful@of.dollars")));

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public async Task CriarCob_schema_drift_throws_contract_violation_missing_chave()
    {
        StubCob("""{"cobId":"cob-0002","status":"ATIVA"}""");

        var ex = await Assert.ThrowsAsync<ContractViolationException>(
            () => new PixClient(_fixture.CreateClient())
                .CriarCobAsync(new CobRequest(12.50m, "fistful@of.dollars")));

        Assert.Contains("chave", ex.Message);
    }
}
