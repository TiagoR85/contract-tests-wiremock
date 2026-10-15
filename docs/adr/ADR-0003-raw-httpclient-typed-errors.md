# ADR-0003: Raw HttpClient with typed errors instead of WCF/Refit-style stacks

**Status:** Accepted
**Date:** 2026-10-15

## Decision

All three clients sit directly on `HttpClient`. One shared helper (`ContractHttp.SendAsync`) maps failures to typed exceptions: `UpstreamTimeoutException` (HttpClient timeout — `TaskCanceledException` whose inner is `TimeoutException`), `UpstreamServerException(StatusCode)` (any non-2xx), `ContractViolationException(Errors)` (response violates the embedded contract).

## Context

The SEFAZ integration could use CoreWCF/WCF client stacks, and REST could use Refit or a generated SDK. Both would hide the exact layer this repo exists to teach: transport, classification, and contract.

## Consequences

- The SOAP envelope is an interpolated string with `Content-Type: text/xml` and a `SOAPAction` header — the actual wire format is visible and asserted in tests.
- Exception taxonomy is the public API of the clients; callers can react to timeout vs. server failure vs. drift without string matching.
- No retry/backoff here (out of scope — see `jobforge-runner` ADR-0001 for the retry classification pattern).

## Rejected alternatives

- **CoreWCF client:** heavy dependency and abstraction over a single POST with one header.
- **Refit:** nice ergonomics, but hides status/timeout classification behind generated code.
