# ADR-0002: Contracts as embedded JSON Schema / XSD instead of snapshots or Pact

**Status:** Accepted
**Date:** 2026-10-15

## Decision

Store each API contract as versioned code inside the repo — JSON Schema 2020-12 files for HTTPBin and PIX, a minimal XSD for the NFe authorization response — embedded as resources and validated **inside the clients** at runtime. A response that violates the contract throws `ContractViolationException` with the collected errors.

## Context

Schema drift (a provider changing its response without notice) must fail loudly. Options: snapshot/golden files (catch diffs but do not explain the contract), Pact with a broker (right tool for consumer/provider networks, heavy for a study repo), or hand-written asserts (not a schema at all).

## Consequences

- The contract is readable, diffable, and reusable (OpenAPI 3.1 tools speak JSON Schema natively).
- Validation lives in the client, so the same guard runs in tests and would run in production code.
- The XSD intentionally validates only the `nfeResultMsg` fragment (no SOAP namespace chain) — a documented simplification; a full WSDL/XSD set belongs to a real integration.

## Rejected alternatives

- **Verify/snapshot tests:** detect change but describe it as a file diff, not as a requirement (`required: [url]`).
- **Pact broker:** adds running infrastructure and vocabulary with no second party to contract with here.
