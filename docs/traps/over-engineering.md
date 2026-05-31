# Ловушка: Овер-инжиниринг (Over-Engineering)

## Сценарий

Агент реализует фичу, но вместо простого решения строит архитектурный собор:

- Для валидации 2 полей создаёт `IValidationStrategy<T>` с 3 реализациями
- Для списка заказов из 5 колонок внедряет CQRS + Read Model + Projection
- Для вызова внешнего API оборачивает `HttpClient` в 4 слоя: `Factory` → `Provider` → `Service` → `Manager`
- Для хранения `User` с `Id`, `Name`, `Email` создаёт `BaseEntity<TId>` с 8 generic-ограничениями

```csharp
// Агент: "Нужно получить заказ по id"
// Было (простое):
var order = await db.Orders.FindAsync(id);

// Стало (архитектурный собор):
var query = new GetOrderByIdQuery(id);
var handler = _mediator.Send(query);
var result = await _pipelineBehavior.Handle(handler, CancellationToken.None);
var dto = _mapper.Map<OrderResponseDto>(result.Value);
```

## Последствия

- **Время чтения:** junior разработчик тратит день, чтобы понять, как работает `FindAsync`
- **Время отладки:** баг в валидации прячется за 3 интерфейса и 2 фабрики
- **Время компиляции:** generic-вложенность замедляет IntelliSense и сборку
- **Время тестирования:** для проверки `a + b > 0` нужно мокнуть 5 зависимостей
- **AI-деградация:** следующий агент, видя "красивый" код, добавляет ещё один слой абстракции

## Почему агент овер-инжинирит

- **Training data:** в обучающих данных больше "правильных" примеров с Clean Architecture, чем простых скриптов
- **Pattern recognition:** агент видит `Order` → автоматически генерирует `IOrderRepository`, `OrderService`, `OrderManager`
- **Hallucination of scale:** агент не знает, что у проекта 10 пользователей, и внедряет Event Sourcing "на вырост"
- **Lack of context:** агент не видит, что соседняя фича сделана в 5 строк, и делает свою в 500

## Почему автотеста недостаточно

Можно измерить количество интерфейсов или глубину generic-вложенности, но **сложность — это семантика, а не синтаксис**:

```csharp
// Автотест не поймёт, что это перебор:
public interface IBookingValidationStrategy<TRequest, TResult, TContext>
    where TRequest : class, IRequest<TResult>
    where TResult : class
    where TContext : ValidationContext<TRequest>
{
    Task<TResult> ValidateAsync(TRequest request, TContext context, CancellationToken ct);
}
```

Автотест скажет "много generic-параметров", но не скажет, зачем они на самом деле.

## Решение

### 1. Simplicity Audit — персона-аудитор
Агент запускается раз в спринт с чеклистом простоты. См. `skills/simplicity-audit/SKILL.md`.

Чеклист:
- [ ] Интерфейс с одной реализацией — можно заменить на класс?
- [ ] CQRS/Event Sourcing — есть ли требование на read/write разделение или аудит?
- [ ] Generic-конвейер — сколько параметров? Можно ли свернуть?
- [ ] DTO-вложенность — сколько уровней? Клиент использует все поля?
- [ ] async void — есть ли в коде? Заменить на Task?
- [ ] Методы с > 5 параметрами — вынести в DTO?

### 2. Code Review: "Объясни junior'у" `[ADAPT]`
Добавь в свой `skills/code-review/CHECKLIST.md`:
> Если решение нельзя объяснить junior-разработчику за 5 минут — оно слишком сложное.

### 3. AGENTS.md: Правило «Простота > Паттерн» `[ADAPT]`
Добавь в корневой `AGENTS.md` проекта:

```markdown
## Простота vs Паттерн
- Предпочитай `if/else` `IStrategy`, пока ветвлений < 3
- Предпочитай `db.Orders.Where(...)` `IRepository<Order>`, пока нет тестов на замену БД
- Предпочитай record/DTO `IResponseMapper<TDomain, TDto>`, пока маппинг тривиален
- Любая абстракция должна иметь **две** реализации или **одну** вескую причину (тестирование, DI)
- Запрещён `async void` вне event-handlers фреймворка
- Метод с > 5 параметрами → выноси в параметр-объект
```

### 4. Метрика: Interface-to-Class Ratio
Архитектурный тест считает отношение интерфейсов к классам в слое:

```csharp
// Если в Application > 0.8 — тревога
var interfaces = assembly.GetTypes().Count(t => t.IsInterface);
var classes = assembly.GetTypes().Count(t => t.IsClass && !t.IsAbstract);
var ratio = (double)interfaces / classes;
```

### 5. Objective метрики + мёртвый guardrail
Автотесты ловят измеримое **только если этот паттерн реально возникает в твоей кодбазе**:

- `GenericParameters_ShouldNotExceed_3` — public типы с > 3 generic-параметрами
- `MethodNames_ShouldNotExceed_40Chars` — имена методов > 40 символов
- `MethodParameters_ShouldNotExceed_5` — методы с > 5 параметрами
- `AsyncVoid_ShouldNotExist` — async void в production-коде
- `FileLength_ShouldNotExceed_300Lines` — файлы > 300 effective lines

> **Но:** если в проекте generic > 3, inheritance > 3 или nested Func **никогда не возникают**, то эти проверки — **мёртвый guardrail**. Он создаёт ложное ощущение защиты, тратит время CI и размывает внимание.
>
> ```csharp
> // Ты добавил SimplicityGuardTest с 8 проверками
> // Но в твоём проекте:
> // - Generic > 3 parameters — никогда не было
> // - Inheritance depth > 3 — никогда не было
> // - Nested Func — никогда не было
> // 
> // Результат: 27 тестов, 0 падений, 0 ценности.
> // Это архитектурный dead code — ровно то, что guardrail должен ловить.
> ```
>
> **Правило:** оставляй guardrail только если он хотя бы раз поймал реальный баг. Иначе — удали.

### 6. Правило «No abstraction without pain»
В проекте фиксируется принцип: абстракция вводится только тогда, когда **уже** есть боль от её отсутствия (тесты ломаются, код дублируется, замена реализации нужна).

Не: "наверняка пригодится". Только: "уже больно без этого".

## Паттерн

См. `skills/simplicity-audit/SKILL.md` и `skills/simplicity-audit/CHECKLIST.md`
