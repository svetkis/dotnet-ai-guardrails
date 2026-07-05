# Type Safety Checklist

## Pre-flight
- [ ] Diff of changes is available
- [ ] Scope is known (which entities and DTOs are affected)

## Strongly Typed IDs
- [ ] All new ID properties in Domain have their own type (not Guid/string/int/long)
- [ ] ID type is `readonly record struct` with a single `Value` field
- [ ] There is a `.New()` factory or constructor for creation
- [ ] A `JsonConverter` is added for JSON serialization (System.Text.Json or Newtonsoft)
- [ ] A `ValueConverter` is added for EF Core (if an ORM is used)

## Value Objects
- [ ] Primitives that travel together are combined into a record/class (Money, Address, DateRange)
- [ ] Value Objects are immutable (`init` or `readonly record struct`)

## Anti-patterns
- [ ] No methods with signature `void DoSomething(Guid id1, Guid id2, string code)`
- [ ] No DTOs with field `public string Status` instead of enum/strong type
- [ ] No passing of primitives across layer boundaries without typing

## Regression
- [ ] Architecture test `StronglyTypedIds` passes (`DomainEntities_ShouldNotUseRawPrimitivesForIds`)
- [ ] Ratchet on the number of strongly typed IDs has not decreased
