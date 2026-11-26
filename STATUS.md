# Status do Projeto — v1.0.1 (2026-11-26)

## Estado Atual

Showcase de testes de contrato: três clientes HTTP (REST/JSON HTTPBin, SOAP/XML SEFAZ, REST/JSON PIX com idempotência) verificados contra stubs WireMock.Net locais em 12 cenários (sucesso, timeout, HTTP 500 e schema drift para cada API). Contratos são código (JSON Schema 2020-12 + XSD embutidos como recursos) e drift falha o teste com erro exato. Tudo roda offline, sem Docker, em segundos.

## Fases

| Fase | Estado | Observação |
|---|---|---|
| Stubbing WireMock (paths, métodos, delays) | ✅ Concluído | `Fixtures/WireMockFixture.cs`, fixture compartilhada por coleção xUnit |
| Contrato como código (JSON Schema + XSD embutidos) | ✅ Concluído | `Contracts/*.json|*.xsd`, `Clients/ContractValidator.cs` |
| Classificação tipada de erros de transporte | ✅ Concluído | `Clients/Exceptions.cs` (`UpstreamTimeoutException`, `UpstreamServerException`, `ContractViolationException`), `ContractHttp.SendAsync` |
| Asserção de header de idempotência via log de requisições | ✅ Concluído | `Clients/PixClient.cs` + `Tests/PixTests.cs` |
| CI com gate de cobertura de 80% (exclui Tests/Fixtures) | ✅ Concluído | commit `9169d91` |
| Classificação 4xx/429/408 + retry | ⏳ Pendente | backlog do README |
| Fluxo Pact consumer/provider | ⏳ Pendente | backlog do README |
| Cenários stateful do WireMock, record & replay, schema a partir de OpenAPI, mTLS/API-key, geração por WSDL | ⏳ Pendente | backlog do README |

## Pendências

- [ ] Classificação de 4xx/429/408 + retry (referência: `jobforge-runner`).
- [ ] Fluxo Pact consumer/provider.
- [ ] Cenários stateful do WireMock e record & replay via proxy.
- [ ] Geração de schema a partir do OpenAPI do provedor; geração de contrato por WSDL.
- [ ] Autenticação mTLS/API-key nos clientes.
