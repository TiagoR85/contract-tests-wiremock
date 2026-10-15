using System.Net;
using ContractTests.WireMock.Clients;
using ContractTests.WireMock.Fixtures;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace ContractTests.WireMock.Tests;

[Collection("wiremock")]
public sealed class SefazNfeTests
{
    private const string GoodResponse = """
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
          <env:Body>
            <nfeResultMsg>
              <protNFe>
                <infProt>
                  <cStat>104</cStat>
                  <nProt>352621000000001</nProt>
                </infProt>
              </protNFe>
            </nfeResultMsg>
          </env:Body>
        </env:Envelope>
        """;

    private const string DriftResponse = """
        <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
          <env:Body>
            <nfeResultMsg>
              <protNFe>
                <infProt>
                  <cStat>104</cStat>
                </infProt>
              </protNFe>
            </nfeResultMsg>
          </env:Body>
        </env:Envelope>
        """;

    private readonly WireMockFixture _fixture;

    public SefazNfeTests(WireMockFixture fixture)
    {
        _fixture = fixture;
        _fixture.Reset();
    }

    private void StubAutorizacao(string body, HttpStatusCode status = HttpStatusCode.OK, TimeSpan? delay = null)
    {
        var response = Response.Create()
            .WithStatusCode((int)status)
            .WithHeader("Content-Type", "text/xml; charset=utf-8")
            .WithBody(body);
        if (delay is not null)
            response = response.WithDelay(delay.Value);
        _fixture.Server
            .Given(Request.Create().WithPath("/nfe/autorizacao").UsingPost())
            .RespondWith(response);
    }

    [Fact]
    public async Task Autorizar_success_validates_envelope_and_returns_protocol()
    {
        StubAutorizacao(GoodResponse);

        var protocol = await new SefazNfeClient(_fixture.CreateClient()).AutorizarAsync("<infNFe />");

        Assert.Equal("104", protocol.Stat);
        Assert.Equal("352621000000001", protocol.ProtocolNumber);

        var entry = _fixture.Server.LogEntries.Single(e => e.RequestMessage!.Path == "/nfe/autorizacao");
        Assert.Contains("SOAPAction", entry.RequestMessage!.Headers!.Keys);
        Assert.Contains("text/xml", entry.RequestMessage.Headers!["Content-Type"]!.First());
        Assert.Contains("infNFe", entry.RequestMessage.Body);
    }

    [Fact]
    public async Task Autorizar_timeout_throws_upstream_timeout()
    {
        StubAutorizacao(GoodResponse, delay: TimeSpan.FromSeconds(3));

        var ex = await Assert.ThrowsAsync<UpstreamTimeoutException>(
            () => new SefazNfeClient(_fixture.CreateClient(TimeSpan.FromMilliseconds(400)))
                .AutorizarAsync("<infNFe />"));

        Assert.Contains("timeout", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Autorizar_500_throws_upstream_server_with_status()
    {
        StubAutorizacao("Internal Server Error", HttpStatusCode.InternalServerError);

        var ex = await Assert.ThrowsAsync<UpstreamServerException>(
            () => new SefazNfeClient(_fixture.CreateClient()).AutorizarAsync("<infNFe />"));

        Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
    }

    [Fact]
    public async Task Autorizar_schema_drift_throws_contract_violation_missing_protocol()
    {
        StubAutorizacao(DriftResponse);

        var ex = await Assert.ThrowsAsync<ContractViolationException>(
            () => new SefazNfeClient(_fixture.CreateClient()).AutorizarAsync("<infNFe />"));

        Assert.Contains("nProt", ex.Message);
    }
}
