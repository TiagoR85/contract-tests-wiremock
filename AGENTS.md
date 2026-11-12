# AGENTS — contract-tests-wiremock

Regras deste repo (valem para qualquer agente que trabalhe aqui).

## Entrega

- **TDD**: teste antes da implementação; mudança não trivial sai com teste.
- **Docs no mesmo commit**: STATUS.md (título com a versão), CHANGELOG.md (entrada
  nova no topo, antigas nunca reescritas), MANUAL.md quando o comando muda,
  DECISIONS.md para decisão de arquitetura (ADR completo em `docs/adr/`).
- **Semver + tag anotada** a cada entrega relevante (`git tag -a vX.Y.Z`).
- **Card no Hub**: `POST http://127.0.0.1:5002/api/tasks`
  (`{"project_id": <id>, "title": "...", "status": "todo"}`), mover para
  `in_progress`/`done` com `PATCH /api/tasks/<id>`.
- **Pre-commit `doccheck`** (vendorizado em `.githooks`): a doc sai no mesmo commit.

## Stack e convenções

- .NET 10 (`global.json` 10.0.301); solução no formato `.slnx`; dependências
  centralizadas em `Directory.Packages.props` (CPM).
- **Tudo offline**: nenhum teste chama rede real ou Docker — stubs WireMock.Net locais
  em porta aleatória; cliente novo que precisar de rede quebra essa regra.
- **Contrato como código**: todo cliente tem contrato correspondente em
  `Contracts/` (JSON Schema 2020-12 ou XSD) como recurso embutido; schema e cliente
  mudam no mesmo commit.
- **Erros tipados**: `ContractHttp.SendAsync` classifica em
  `UpstreamTimeoutException` / `UpstreamServerException` /
  `ContractViolationException`; não inventar quarto tipo sem registrar na classe.
- **Fixture compartilhada**: `WireMockFixture` é collection fixture com `Reset()`
  por teste; não subir servidor WireMock novo por teste.
- Gate de cobertura de 80% de linha no CI mede só `Clients/` (excluir namespace novo
  de tests/fixtures no config do Coverlet).
- Zero leak: nada de chave, token ou caminho de produção em doc/commit; `.env`
  nunca versionado.

## Estilo de código

- Cada API simulada é um cliente (`HttpBinClient`, `SefazNfeClient`, `PixClient`)
  com seus testes próprios; validar contrato antes de desserializar.
