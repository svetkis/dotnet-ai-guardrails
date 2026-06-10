# Type Safety Checklist

## Pre-flight
- [ ] Получен diff изменений
- [ ] Известен scope (какие сущности и DTO затронуты)

## Strongly Typed IDs
- [ ] Все новые ID-свойства в Domain имеют собственный тип (не Guid/string/int/long)
- [ ] Тип ID — `readonly record struct` с единственным полем `Value`
- [ ] Есть фабрика `.New()` или конструктор для создания
- [ ] Для JSON сериализации добавлен `JsonConverter` (System.Text.Json или Newtonsoft)
- [ ] Для EF Core добавлен `ValueConverter` (если используется ORM)

## Value Objects
- [ ] Примитивы, передающиеся вместе, объединены в record/class (Money, Address, DateRange)
- [ ] Value Objects immutable (`init` или `readonly record struct`)

## Антипаттерны
- [ ] Нет методов с сигнатурой `void DoSomething(Guid id1, Guid id2, string code)`
- [ ] Нет DTO с полем `public string Status` вместо enum/strong type
- [ ] Нет передачи примитивов через границы слоёв без типизации

## Регрессия
- [ ] Архитектурный тест `StronglyTypedIds` проходит (`DomainEntities_ShouldNotUseRawPrimitivesForIds`)
- [ ] Ratchet на количество strongly typed IDs не уменьшился
