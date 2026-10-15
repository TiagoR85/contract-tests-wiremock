using System.Net;

namespace ContractTests.WireMock.Clients;

public sealed class UpstreamTimeoutException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class UpstreamServerException(HttpStatusCode statusCode)
    : Exception($"Upstream API failed with HTTP {(int)statusCode} ({statusCode}).")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

public sealed class ContractViolationException(IReadOnlyList<string> errors)
    : Exception($"Contract violation: {string.Join("; ", errors)}")
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
