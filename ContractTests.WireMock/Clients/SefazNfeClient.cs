using System.Text;
using System.Xml.Linq;

namespace ContractTests.WireMock.Clients;

public sealed record NFeProtocol(string Stat, string ProtocolNumber);

public sealed class SefazNfeClient
{
    private static readonly XNamespace Soap = "http://schemas.xmlsoap.org/soap/envelope/";

    private readonly HttpClient _http;

    public SefazNfeClient(HttpClient http) => _http = http;

    public async Task<NFeProtocol> AutorizarAsync(string nfeXml, CancellationToken cancellationToken = default)
    {
        var envelope = $"""
            <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/">
              <env:Header />
              <env:Body>
                <nfeDadosMsg xmlns="http://www.portalfiscal.inf.br/nfe">{nfeXml}</nfeDadosMsg>
              </env:Body>
            </env:Envelope>
            """;

        using var request = new HttpRequestMessage(HttpMethod.Post, "nfe/autorizacao")
        {
            Content = new StringContent(envelope, Encoding.UTF8, "text/xml")
        };
        request.Headers.TryAddWithoutValidation(
            "SOAPAction",
            "\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4/AutorizarLoteNFe\"");

        using var response = await ContractHttp.SendAsync(
            ct => _http.SendAsync(request, ct), cancellationToken);
        var xml = await response.Content.ReadAsStringAsync(cancellationToken);

        var document = XDocument.Parse(xml);
        var resultMsg = document.Root?.Element(Soap + "Body")?.Element("nfeResultMsg")
            ?? throw new ContractViolationException(["nfeResultMsg missing from the SOAP response body"]);

        var validated = ContractValidator.ValidateXml("nfe-response.xsd", resultMsg.ToString());
        var infProt = validated.DocumentElement!["protNFe"]!["infProt"]!;
        return new NFeProtocol(infProt["cStat"]!.InnerText, infProt["nProt"]!.InnerText);
    }
}
