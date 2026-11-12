# Changelog

Formato baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/);
versão em [SemVer](https://semver.org/lang/pt-BR/).

## [1.0.0] — 2026-11-12

### Adicionado
- Três clientes HTTP com validação de contrato embutida: `HttpBinClient` (REST/JSON), `SefazNfeClient` (SOAP/XML com XSD) e `PixClient` (REST/JSON com header `Idempotency-Key`).
- `ContractHttp.SendAsync`: classificação tipada de transporte em `UpstreamTimeoutException`, `UpstreamServerException` e `ContractViolationException` (sem WCF/Refit).
- `ContractValidator`: validação de resposta por JSON Schema 2020-12 e XSD antes de qualquer parsing — drift quebra o teste com os erros exatos.
- Contratos como recursos embutidos: `Contracts/httpbin-get.schema.json`, `Contracts/pix-cob.schema.json`, `Contracts/nfe-response.xsd`.
- Fixture compartilhada do WireMock.Net (collection fixture xUnit, `Reset()` antes de cada teste) — 12 cenários: sucesso, timeout, HTTP 500 e schema drift por API.
- Asserção do header de idempotência via log de requisições do WireMock.
- CI com gate de cobertura de linha de 80% (Coverlet exclui namespaces `Tests`/`Fixtures`).
- ADR-0001 (fixture compartilhada), ADR-0002 (contrato como código, em vez de snapshots/Pact), ADR-0003 (HttpClient cru com erros tipados).
