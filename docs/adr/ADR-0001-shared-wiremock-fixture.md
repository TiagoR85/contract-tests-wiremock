# ADR-0001: One shared WireMock server per xUnit collection

**Status:** Accepted
**Date:** 2026-10-15

## Decision

Start a single `WireMockServer` (random port) in an xUnit collection fixture (`[Collection("wiremock")]`) shared by all three test classes, and call `Reset()` in each test-class constructor so every test registers its own mappings and gets a clean log.

## Context

The suite has 12 scenarios across three classes. Alternatives were a server per test class (port churn, still needs reset) or a server per test (slowest; constructor noise in every test).

## Consequences

- One process-wide server; tests within a collection never run in parallel, which is fine — the whole suite finishes in seconds.
- `Reset()` also clears `LogEntries`, so request-log assertions (`Idempotency-Key`, `SOAPAction`) are race-free.
- Delayed responses from timeout tests are dropped by `Reset()` instead of leaking into later tests.

## Rejected alternatives

- **Server per test:** extra ~100ms per test and more disposal code for no isolation gain at this scale.
- **Parallel classes with per-class servers:** faster in theory, but the suite is not CPU-bound; the complexity buys nothing.
