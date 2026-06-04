# DemoProject.MinimalApi

> Single-project ASP.NET Core Minimal API с guardrails.
> Для проектов **без Clean Architecture** — показывает, какие архитектурные тесты остаются релевантными.

## Стек

- .NET 10
- ASP.NET Core Minimal API
- Single-project (нет слоёв Domain / Application / Infrastructure)
- TUnit + `dotnet run --project`
- NetArchTest.eNhancedEdition

## Что внутри

```
src/DemoProject.MinimalApi/
├── Program.cs
├── Domain/
│   ├── Order.cs              # Record с init-only properties
│   └── Payment.cs
├── Features/
│   ├── Orders/
│   │   ├── OrderEndpoints.cs # Minimal API endpoint mapping
│   │   └── OrderService.cs   # Бизнес-логика
│   └── Payments/
│       ├── PaymentEndpoints.cs
│       └── PaymentService.cs

tests/DemoProject.MinimalApi.Tests/
├── ArchitectureRules.cs      # Naming, banned APIs (DateTime.Now), CancellationToken
├── RatchetTests.cs           # Публичные типы и тесты не уменьшились
└── DuplicationGuardTest.cs   # Бизнес-логика не дублируется
```

## Как запустить

```bash
cd examples/DemoProject.MinimalApi
dotnet build
dotnet run --project tests/DemoProject.MinimalApi.Tests
```

## Чем отличается от DemoProject (Clean Architecture)

| Аспект | DemoProject (Clean) | DemoProject.MinimalApi (No Layers) |
|--------|---------------------|------------------------------------|
| Проектов | 4 (Domain, Application, Infrastructure, Tests) | 1 + Tests |
| Arch-тесты | Зависимости между слоями | Naming conventions, banned APIs |
| EF Core | Да | Нет (in-memory список) |
| Цель | Показать guardrails для CA | Показать guardrails для single-project MVP |
