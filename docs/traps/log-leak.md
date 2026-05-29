# Ловушка: Утечка данных в логи (Log Leak)

## Сценарий

Агент добавляет логирование для дебага и не думает о чувствительных данных:

```csharp
// Агент: "Добавлю лог, чтобы видеть почему падает"
_logger.LogInformation("User {Email} failed login from {Ip}", user.Email, ip);

// Или ещё хуже — string interpolation:
_logger.LogError($"Payment failed for {order.Phone}, card: {order.CardLast4}");
```

## Последствия

- Email, телефоны, IP-адреса утекают в лог-систему (Elastic, Kibana, Seq)
- Логи часто доступны шире, чем БД (SRE, поддержка, внешние системы)
- GDPR / compliance нарушение — штрафы
- Токены и session ID в логах = потенциальная компрометация сессий

## Почему стандартные слои не ловят

| Слой | Почему не ловит |
|------|-----------------|
| Компилятор | `LogInformation(string, object[])` — валидная сигнатура |
| Архитектура | NetArchTest не видит содержимое строковых аргументов |
| Тесты | Юнит-тесты проверяют логику, не проверяют что пишется в `ILogger` |
| Code Review | Агент-ревьюер видит "логирование добавлено" и не смотрит аргументы |
| E2E | Приложение работает, логи пишутся |

## Решение

### Уровень 1. Compile-time guard (Roslyn / Analyzer)

Custom `DiagnosticAnalyzer` запрещает `Log*` с interpolation (`$"..."`) и маркированными типами:

```csharp
// Анализатор выдаёт warning/error:
_logger.LogError($"Failed for {user.Email}"); // ERROR: interpolated string in logging
```

### Уровень 2. Attribute-driven inventory (Ratchet)

Все PII-поля маркируются `[SensitiveData]`:

```csharp
public record UserDto(
    [SensitiveData] string Email,
    [SensitiveData] string Phone,
    string Name
);
```

Арх-тест проверяет:
1. Все свойства с `*Email*`, `*Phone*`, `*Password*` в имени имеют `[SensitiveData]`
2. Количество `[SensitiveData]` свойств не уменьшается (ratchet)
3. `Log*` вызовы не передают параметры типов, содержащих `[SensitiveData]`

### Уровень 3. Runtime redaction (Serilog / Middleware)

```csharp
// Serilog destructuring policy
public class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyFactory,
        out LogEventPropertyValue result)
    {
        var type = value.GetType();
        var properties = type.GetProperties()
            .Where(p => p.GetCustomAttribute<SensitiveDataAttribute>() != null);

        // Redact marked properties
        ...
    }
}
```

## Паттерн

См. `tests/patterns/PiiGuardTest.cs` и `docs/solutions/ai-patterns.md` (паттерн #9)
