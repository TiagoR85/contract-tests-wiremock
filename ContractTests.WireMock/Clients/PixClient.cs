using System.Net.Http.Json;

namespace ContractTests.WireMock.Clients;

public sealed record CobRequest(decimal Valor, string Chave);

public sealed record PixCob(string CobId, string Status, string Chave);

public sealed class PixClient
{
    private readonly HttpClient _http;

    public PixClient(HttpClient http) => _http = http;

    public async Task<PixCob> CriarCobAsync(
        CobRequest request,
        string? idempotencyKey = null,
        CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "v2/cob")
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add("Idempotency-Key", idempotencyKey ?? Guid.NewGuid().ToString("N"));

        using var response = await ContractHttp.SendAsync(
            ct => _http.SendAsync(message, ct), cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var node = ContractValidator.ValidateJson("pix-cob.schema.json", json);
        return new PixCob(
            node["cobId"]!.GetValue<string>(),
            node["status"]!.GetValue<string>(),
            node["chave"]!.GetValue<string>());
    }
}
