namespace ContractTests.WireMock.Clients;

internal static class ContractHttp
{
    internal static async Task<HttpResponseMessage> SendAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response;
        try
        {
            response = await send(cancellationToken);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new UpstreamTimeoutException(
                "The upstream API did not respond within the configured timeout.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var statusCode = response.StatusCode;
            response.Dispose();
            throw new UpstreamServerException(statusCode);
        }

        return response;
    }
}
