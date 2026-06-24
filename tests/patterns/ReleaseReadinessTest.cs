// TRAP: Перед релизом забывают проверить критичные guardrails: security headers, rate limiting, OpenAPI snapshot, smoke.
// GUARDRAIL: Композитный тест проверяет наличие обязательных артефактов и прохождение ключевых проверок.
//
// Адаптация под фреймворк:
// - TUnit:  [Test] + Assert.That(...)
// - xUnit:  [Fact] + Assert.True(...)
// - NUnit:  [Test] + Assert.That(...)
// - MSTest: [TestMethod] + Assert.IsTrue(...)
//
// NOTE: Это не замена полным аудитам, а быстрый gate перед релизом. Не дублируйте логику других тестов.

using System.Net.Http.Json;
using TUnit;

namespace Tests.Patterns;

public class ReleaseReadinessTests
{
    private static readonly HttpClient HttpClient = new() { BaseAddress = new Uri("http://localhost:5000") };

    // TRAP: Релиз уходит без проверки health endpoint.
    // GUARDRAIL: /health отвечает 200 OK.
    [Test]
    public async Task HealthEndpoint_ShouldBeHealthy()
    {
        var response = await HttpClient.GetAsync("/health");
        Assert.That(response.StatusCode).IsEqualTo(System.Net.HttpStatusCode.OK);
    }

    // TRAP: Security headers не настроены или агент их удалил.
    // GUARDRAIL: Проверяем наличие базовых security headers.
    [Test]
    public async Task SecurityHeaders_ShouldBePresent()
    {
        var response = await HttpClient.GetAsync("/health");
        var headers = response.Headers;

        Assert.That(headers.Contains("X-Content-Type-Options")).IsTrue();
        Assert.That(headers.Contains("X-Frame-Options")).IsTrue();
        Assert.That(headers.Contains("Referrer-Policy")).IsTrue();
    }

    // TRAP: OpenAPI контракт сломался, а фронт не узнал.
    // GUARDRAIL: /openapi/v1.json доступен и валиден.
    [Test]
    public async Task OpenApiContract_ShouldBeAvailable()
    {
        var response = await HttpClient.GetAsync("/openapi/v1.json");
        Assert.That(response.StatusCode).IsEqualTo(System.Net.HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content).Contains("\"openapi\"");
    }

    // TRAP: Важные конфигурационные файлы или документы отсутствуют перед релизом.
    // GUARDRAIL: Проверяем наличие обязательных артефактов.
    [Test]
    public void RequiredReleaseArtifacts_ShouldExist()
    {
        var requiredFiles = new[]
        {
            "../AGENTS.md",
            "../docs/DEPLOYMENT.md",
            "../.github/workflows/deploy-api.yml"
        };

        var missing = requiredFiles.Where(f => !File.Exists(f)).ToList();
        Assert.That(missing).IsEmpty()
            .Because("Required release artifacts must exist: {0}", string.Join(", ", missing));
    }
}
