# Version Audit Checklist

## Pre-flight
- [ ] Получен список изменённых файлов (`global.json`, `*.csproj`, `package.json`, `Dockerfile`)
- [ ] Известен team standard для TargetFramework (например, `net9.0`)
- [ ] Доступен вывод `dotnet list package --outdated` (для .NET-проектов)

## SDK & Runtime
- [ ] `global.json` — версия без `preview`/`rc`/`beta`
- [ ] `global.json` — `rollForward` != `disable`
- [ ] Все проекты используют единый `TargetFramework`

## NuGet Packages
- [ ] Нет `preview`/`rc`/`beta` в `PackageReference`/`PackageVersion`
- [ ] Версии Microsoft.* пакетов совпадают с TargetFramework (например, `net9.0` → EF Core 9.x)
- [ ] Нет пакетов старше 2 мажорных релизов от current LTS
- [ ] `dotnet list package --vulnerable` — чисто или все уязвимости задокументированы

## Frontend (если применимо)
- [ ] `package.json` — нет `alpha`/`beta`/`rc` в `dependencies`
- [ ] React/Vue/Angular — отставание не более 1 мажорной версии
- [ ] TypeScript — совместим с framework

## Infrastructure
- [ ] `Dockerfile` — base image не `preview`, не `rc`
- [ ] GitHub Actions — `actions/setup-dotnet` с указанием версии или `global.json`
- [ ] GitHub Actions — `actions/checkout@v4`, не `v2` или `v3`
