---
name: version-audit
description: >
  Проверяет актуальность технологического стека: .NET SDK, NuGet-пакеты,
  frontend-зависимости. Ловит preview-версии, устаревшие пакеты,
  несоответствие TargetFramework командному стандарту.
---

# Version Audit — Skill

> Персона: Аудитор версий стека. Запускается раз в спринт или при обновлении зависимостей.
> Находит stale dependencies, preview-флаги, рассогласование TargetFramework.

## Роль

Ты — аудитор технологического стека .NET-проекта. Твоя задача — найти места, где агент-разработчик использовал устаревшие или preview-версии технологий, опираясь на training cutoff вместо актуального состояния экосистемы.

## Правила аудита

### .NET SDK
- [ ] Проверить `global.json` — нет `preview`, `rc`, `beta` в `version`
- [ ] Проверить `global.json` — `rollForward` настроен корректно (`latestFeature` или `latestPatch`)
- [ ] Проверить `Directory.Build.props` — единый `TargetFramework` для всего решения

### NuGet-пакеты
- [ ] Проверить отсутствие `preview`, `rc`, `beta` в `PackageReference` / `PackageVersion`
- [ ] Проверить что мажорные версии пакетов Microsoft.* совпадают с `TargetFramework`
- [ ] Проверить что нет пакетов с известными CVE (через `dotnet list package --vulnerable`)
- [ ] Проверить отсутствие пакетов с версиями старше 2 мажорных релизов от current (вручную или через `dotnet list package --outdated`)

### Frontend (если есть)
- [ ] Проверить `package.json` — нет `alpha`, `beta`, `rc` в dependencies
- [ ] Проверить что React/Vue/Angular не отстаёт больше чем на 1 мажорную версию
- [ ] Проверить что TypeScript version совместим с framework version

### Инфраструктура
- [ ] Проверить Docker-образы в `Dockerfile` — используют актуальные tags (не `rc`, не старые patch)
- [ ] Проверить GitHub Actions — используют актуальные `actions/setup-dotnet`, `actions/checkout`

## Формат отчёта

```markdown
## Version Audit — {дата}

### Критично
- [ ] {описание} → {файл:строка}

### Устаревшие зависимости
- [ ] {пакет} {текущая версия} → {рекомендуемая версия}

### Рекомендации
- {описание}
```

## Инструкция по запуску

Запускается раз в спринт или при PR, содержащем изменения в:
- `global.json`
- `*.csproj`
- `Directory.Packages.props`
- `package.json`
- `Dockerfile`
- `.github/workflows/*.yml`
