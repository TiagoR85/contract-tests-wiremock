using System.Text.Json.Nodes;

namespace ContractTests.WireMock.Clients;

public sealed record HttpBinResult(string Origin, string Url);

public sealed class HttpBinClient
{
    private readonly HttpClient _http;

    public HttpBinClient(HttpClient http) => _http = http;

    public async Task<HttpBinResult> GetAsync(CancellationToken cancellationToken = default)
    {
        using var response = await ContractHttp.SendAsync(
            ct => _http.GetAsync("get", ct), cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var node = ContractValidator.ValidateJson("httpbin-get.schema.json", json);
        return new HttpBinResult(
            node["origin"]!.GetValue<string>(),
            node["url"]!.GetValue<string>());
    }
}
