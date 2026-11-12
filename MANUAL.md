# MANUAL — contract-tests-wiremock

## Visão geral

Showcase de testes de contrato: três clientes (REST/JSON, SOAP/XML, REST/JSON com idempotência) testados contra stubs WireMock.Net locais em 12 cenários (sucesso, timeout, HTTP 500, schema drift). Contratos são JSON Schema 2020-12 + XSD embutidos; tudo offline, sem Docker.

## Instalação

Requisitos:

| Requisito | Observação |
|---|---|
| .NET SDK 10 | `global.json` fixa `10.0.301` (rollForward latestFeature) |

Sem Docker, sem rede, sem variáveis de ambiente.

## Primeiro uso

```bash
git clone https://github.com/TiagoR85/contract-tests-wiremock.git
cd contract-tests-wiremock
dotnet test ContractTests.WireMock -p:CollectCoverage=false   # loop rápido (~segundos)
dotnet test ContractTests.WireMock                             # gate do CI (cobertura >= 80% linha)
```

## Comandos

| Comando | Para que serve | Estado |
|---|---|---|
| `dotnet test ContractTests.WireMock -p:CollectCoverage=false` | roda os 12 cenários sem coleta de cobertura (loop rápido) | ✅ |
| `dotnet test ContractTests.WireMock` | roda com gate de cobertura de 80% de linha (como o CI) | ✅ |
| `dotnet restore ContractTests.WireMock.slnx` | restaura a solução (formato `.slnx`) | ✅ |

## Configuração (.env)

Nenhuma variável obrigatória — o `.env.example` existe só para manter a convenção de secret-scan do portfólio. O que precisa existir: .NET SDK 10 instalado. Não precisa de Docker nem de acesso a rede (os stubs são locais; HTTPBin/SEFAZ/PIX são apenas os formatos simulados).

## Troubleshooting

| Sintoma | Causa | Correção |
|---|---|---|
| `ContractViolationException` num teste de sucesso | resposta do stub fora do schema/XSD | conferir o stub montado no teste contra o contrato em `Contracts/` |
| Teste de timeout não dispara `UpstreamTimeoutException` | delay do stub menor que o timeout do cliente | usar `WithDelay` acima do timeout configurado no cliente |
| Cobertura abaixo de 80% no CI | cliente novo sem teste | o gate mede só `Clients/` (namespaces `Tests`/`Fixtures` excluídos) |
| Mudança na API do provedor quebrou vários testes de uma vez | drift de contrato | atualizar schema/XSD e cliente juntos (ADR-0002) |
