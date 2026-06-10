# Ловушка: Устаревший стек (Stale Stack)

## Сценарий

Агент генерирует код, опираясь на training data, а не на актуальное состояние экосистемы:

- Использует .NET 10 preview, хотя в команде стандарт — только stable SDK
- Цепляет EF Core 8 в проект на .NET 9, хотя есть EF Core 9
- Предлагает `Microsoft.Extensions.Caching.Memory` 6.x вместо актуальной 9.x
- В frontend-части React 17 + class components вместо функциональных + hooks
- Использует пакеты с флагом `-preview`, `-rc`, `-beta` без явного согласования

```csharp
// Агент: "Вот пример с .NET 10 Preview 3"
// global.json
{
  "sdk": {
    "version": "10.0.100-preview.3",
    "rollForward": "latestFeature"
  }
}

// PackageReference — версия из training cutoff
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
```

## Последствия

- Security patches не применяются автоматически (preview-пакеты часто не получают обновлений)
- Новые фичи платформы недоступны (например, C# 13 features в .NET 9)
- Лишние транзитивные зависимости из старых пакетов
- Несовместимость с остальным стеком команды
- Агент пишет код с устаревшими API, которые уже deprecated

## Почему стандартные слои не ловят

| Слой | Почему не ловит |
|------|-----------------|
| Компилятор | Код компилируется — preview SDK валиден |
| Архитектура | NetArchTest не проверяет версии пакетов |
| Тесты | Юнит-тесты проверяют логику, не манифест |
| Code Review | Агент-ревьюер тоже опирается на training cutoff |
| E2E | Приложение работает, но с deprecated dependencies |

## Решение

1. **VersionAuditTest** — тест сканирует `global.json`, `*.csproj`, `package.json`:
   - Запрещает `preview`, `rc`, `beta` в `global.json` без явного whitelist
   - Проверяет что `TargetFramework` совпадает с team standard
   - Сканирует `PackageReference` на устаревшие мажорные версии

2. **SKILL.md version-audit** — периодический аудит:
   - Сравнение `PackageReference` с актуальными версиями через `nuget.org` API или `dotnet list package --outdated`
   - Проверка что frontend-зависимости не отстают больше чем на 1 мажорную версию

3. **AGENTS.md правило** — "Не использовать preview-версии без explicit согласования в PR"

## Паттерн

См. `tests/patterns/VersionAuditTest.cs` и `templates/skills/version-audit/`
