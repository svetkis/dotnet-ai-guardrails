// TEMPLATE: Регрессионный тест для баг-фикса.
// Копируй этот файл и переименовывай по шаблону: BUG###_ShortDescription.cs

using TUnit;

namespace Tests.Conventions;

// TRAP: Баг пофиксили, но через неделю агент теми же руками его вернул.
// GUARDRAIL: Баг-фикс обязан сопровождаться тестом, который воспроизводит проблему.
public class BUG_TEMPLATE // Переименуй: BUG055_ShortDescription
{
    [Test]
    public async Task Scenario_ShouldNotReproduceTheBug()
    {
        // Arrange: создаём состояние, при котором баг воспроизводился
        // var context = await SetupBrokenState();

        // Act: выполняем действие, которое раньше ломалось
        // var result = await context.Execute();

        // Assert: убеждаемся, что баг не воспроизводится
        // await Assert.That(result.Status).IsEqualTo(ExpectedStatus);
    }

    // Если баг был race condition или требует специфичного setup —
    // добавь [Before(Test)] и [After(Test)] для изоляции.
}
