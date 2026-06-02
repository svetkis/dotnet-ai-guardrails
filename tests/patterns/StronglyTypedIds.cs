// TRAP: Агент по привычке использует Guid/string/int для идентификаторов в Domain-сущностях,
// вместо создания strongly typed ID (BookingId, ClientId, AgentId).
// Это открывает дверь для подстановки ClientId в метод, ожидающий AgentId.
// GUARDRAIL: Архитектурный тест сканирует Domain-сборку и падает, если найдёт
// "голый" примитив в свойстве с именем, оканчивающимся на Id.

using System.Reflection;
using TUnit;

namespace Tests.Patterns;

public class StronglyTypedIds
{
    // TRAP: Агент создал new Booking { Id = Guid.NewGuid() } вместо BookingId.New().
    // GUARDRAIL: Все свойства с именем *Id в Domain-сущностях должны иметь тип,
    // оканчивающийся на Id (не Guid/string/int/long).
    [Test]
    public void DomainEntities_ShouldNotUseRawPrimitivesForIds()
    {
        // Адаптация: замените на свою assembly и convention.
        // var domainAssembly = typeof(YourDomainEntity).Assembly;
        // var violations = GetRawIdViolations(domainAssembly);
        //
        // Assert.That(violations).IsEmpty()
        //     .Because($"Domain entities must use strongly typed IDs. Violations: {string.Join(", ", violations)}");

        Assert.That(true).IsTrue()
            .Because("Template: adapt this test to your assembly. See commented code and helper below.");
    }

    // TRAP: Агент добавил сущность с Guid Id, но baseline не обновлён — тест молча проходит.
    // GUARDRAIL: Ratchet — считаем текущее количество сущностей с strongly typed ID
    // и проверяем, что оно не уменьшается (или что количество нарушений не растёт).
    [Test]
    public void StronglyTypedIdUsage_ShouldNotDecrease()
    {
        // Адаптация:
        // var domainAssembly = typeof(YourDomainEntity).Assembly;
        // var stronglyTypedCount = CountStronglyTypedIds(domainAssembly);
        //
        // const int baseline = 5; // Зафиксируй текущее значение
        // Assert.That(stronglyTypedCount).IsGreaterThanOrEqualTo(baseline)
        //     .Because($"Strongly typed IDs must not decrease. Current: {stronglyTypedCount}, baseline: {baseline}");

        Assert.That(true).IsTrue()
            .Because("Template: adapt this test to count strongly typed IDs. See commented code.");
    }

    // --- Helper: находит свойства *Id с "голыми" примитивами в Domain-сущностях ---
    private static IEnumerable<string> GetRawIdViolations(Assembly domainAssembly)
    {
        var rawTypes = new HashSet<string> { "Guid", "String", "Int32", "Int64" };

        var entityTypes = domainAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace?.Contains("Domain") == true);

        foreach (var type in entityTypes)
        {
            var idProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name.EndsWith("Id", StringComparison.Ordinal));

            foreach (var prop in idProperties)
            {
                var propTypeName = prop.PropertyType.Name;
                if (rawTypes.Contains(propTypeName) ||
                    (prop.PropertyType.IsGenericType && rawTypes.Contains(prop.PropertyType.GetGenericTypeDefinition().Name)))
                {
                    yield return $"{type.Name}.{prop.Name} : {propTypeName}";
                }
            }
        }
    }

    private static int CountStronglyTypedIds(Assembly domainAssembly)
    {
        var rawTypes = new HashSet<string> { "Guid", "String", "Int32", "Int64" };

        return domainAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace?.Contains("Domain") == true)
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Count(p => p.Name.EndsWith("Id", StringComparison.Ordinal) &&
                        !rawTypes.Contains(p.PropertyType.Name) &&
                        !(p.PropertyType.IsGenericType && rawTypes.Contains(p.PropertyType.GetGenericTypeDefinition().Name)));
    }
}
