// TRAP: Агент поменял DTO, а фронт не узнал. Контракт сломался тихо.
// GUARDRAIL: OpenAPI snapshot тест ловит любое изменение API контракта.

using System.Net.Http.Json;
using System.Text.Json;
using TUnit;

namespace Tests.Patterns;

public class SnapshotTests
{
    private const string SnapshotPath = "../../../Snapshots/openapi-snapshot.json";

    // TRAP: Агент добавил поле в Response DTO, удалил или переименовал.
    // CI зелёный, тесты проходят, но фронт падает.
    // GUARDRAIL: Сравниваем текущий OpenAPI со snapshot. Любой diff = fail.
    [Test]
    public async Task OpenApi_ShouldMatchSnapshot()
    {
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        var response = await client.GetAsync("/openapi/v1.json");
        var currentOpenApi = await response.Content.ReadAsStringAsync();

        if (!File.Exists(SnapshotPath))
        {
            // Первый запуск — создаём snapshot
            Directory.CreateDirectory(Path.GetDirectoryName(SnapshotPath)!);
            await File.WriteAllTextAsync(SnapshotPath, currentOpenApi);
            return;
        }

        var snapshot = await File.ReadAllTextAsync(SnapshotPath);

        // Нормализуем JSON для сравнения
        var currentNormalized = NormalizeJson(currentOpenApi);
        var snapshotNormalized = NormalizeJson(snapshot);

        Assert.That(currentNormalized).IsEqualTo(snapshotNormalized);
    }

    private static string NormalizeJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
    }
}
