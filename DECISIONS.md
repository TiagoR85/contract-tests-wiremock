# Decisões (ADR)

ADR completos em `docs/adr/`; abaixo o registro resumido.

## #01 — Um servidor WireMock compartilhado por coleção xUnit

**Data:** 2026-10-17 · **Status:** aceita

**Decisão:** fixture de coleção com um único `WireMockServer` por execução, `Reset()` antes de cada teste.

**Contexto:** subir um servidor por teste é lento; reset isolam os cenários.

**Alternativas:** servidor por teste (test class fixture).

**Consequência:** testes não podem assumir estado do servidor entre resets (detalhes em `docs/adr/ADR-0001-shared-wiremock-fixture.md`).

## #02 — Contrato como código (JSON Schema 2020-12 + XSD embutidos)

**Data:** 2026-10-17 · **Status:** aceita

**Decisão:** os contratos vivem em `Contracts/*.json|*.xsd` como recursos embutidos; o `ContractValidator` valida a resposta dentro do cliente, antes de qualquer parsing.

**Contexto:** drift de schema deve falhar com os erros exatos do contrato, não com exceção de desserialização; sem infra de Pact/broker.

**Alternativas:** snapshots de resposta (golden files); Pact com broker.

**Consequência:** mudar a API do provedor exige atualizar schema e cliente juntos (detalhes em `docs/adr/ADR-0002-contract-as-code.md`).

## #03 — HttpClient cru com erros tipados (sem WCF/Refit)

**Data:** 2026-10-17 · **Status:** aceita

**Decisão:** `ContractHttp.SendAsync` classifica o resultado do transporte em três exceções: `UpstreamTimeoutException`, `UpstreamServerException`, `ContractViolationException`.

**Contexto:** timeout/5xx/drift são cenários de teste de primeira classe; geração WCF/Refit esconde o envelope.

**Alternativas:** WCF (SOAP); Refit; HttpClientFactory com Polly.

**Consequência:** cada cliente chama `SendAsync` e trata os três tipos explicitamente nos testes (detalhes em `docs/adr/ADR-0003-raw-httpclient-typed-errors.md`).
