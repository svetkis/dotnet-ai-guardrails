# Simplicity Audit — Checklist

## Before Starting
- [ ] Access to codebase obtained (`git log --since="3 months ago"` for context)
- [ ] Project stack known (Clean Architecture / MVP / Minimal API)
- [ ] Complexity baseline established (if legacy)

## Abstraction Bloat
- [ ] Interface with one implementation (except Ports/Adapters)
- [ ] Abstract factory / Builder for simple `new`
- [ ] Generic pipeline with > 2 parameters
- [ ] Strategy / protocol for 2 branches without expansion roadmap
- [ ] Wrapper around a wrapper around a standard API

## Pattern Fetishism
- [ ] CQRS for CRUD with < 5 fields
- [ ] Event Sourcing / Outbox without audit requirements
- [ ] MediatR / PipelineBehavior for synchronous calls
- [ ] Repository + UnitOfWork on top of EF Core with no DB tests
- [ ] Specification pattern for a single `Where`

## Data Matryoshkas
- [ ] DTO nesting > 2 levels
- [ ] API returns > 15 fields, client uses < 5
- [ ] Include chain > 2 levels in a list query
- [ ] Audit fields on entities that audit never reads
- [ ] Generic `BaseEntity<TId>` without diversity of ID types

## Algorithmic Overkill
- [ ] LINQ chain > 5 operations
- [ ] Regex for simple `Replace` / `Split`
- [ ] `Parallel.ForEach` / `Task.WhenAll` on < 10 items
- [ ] Expression Tree instead of plain predicate
- [ ] Reflection instead of `internal` + `InternalsVisibleTo`

## Parameter Bloat
- [ ] Method with > 5 parameters
- [ ] `out` / `ref` / `in` in public API without perf requirements
- [ ] `async void` instead of `Task`

## Infrastructure Complexity
- [ ] Microservice / process for 1 endpoint and < 100 RPS
- [ ] gRPC / GraphQL without latency budget
- [ ] Docker / K8s for a console app
- [ ] Custom middleware instead of standard one

## Test Complexity
- [ ] Setup > 50 lines for a test with < 10 lines of logic
- [ ] Mocks of mocks (mock returns mock)
- [ ] > 2 assertion / mock libraries in the project
- [ ] Test checks call order instead of result

## Language Complexity
- [ ] Generic parameters > 3 on a public type
- [ ] Method name > 40 characters
- [ ] `Func<Func<...>>` or similar nesting
- [ ] `Span<T>`, `Memory<T>` in business logic without perf requirements

## Structural Bloat
- [ ] File > 300 effective lines
- [ ] Class violates SRP (2+ unrelated responsibilities)
- [ ] Method > 50 lines without compelling reason
