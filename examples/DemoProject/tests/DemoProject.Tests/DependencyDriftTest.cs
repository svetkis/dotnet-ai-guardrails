// TRAP: Агент добавил +1 using или ProjectReference — диф выглядит безобидно,
// но породил циклическую зависимость между слоями или проектами.
// GUARDRAIL: NetArchTest + парсинг .csproj ловят нарушения графа.
// Этот файл — рабочая адаптация шаблона из tests/patterns/DependencyDriftTest.cs

using System.Reflection;
using System.Xml.Linq;
using NetArchTest.Rules;
using TUnit;

namespace DemoProject.Tests;

public class DependencyDriftTest
{
    private static readonly Assembly DomainAssembly = typeof(Domain.Booking).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(Infrastructure.InfrastructureBookingService).Assembly;

    // TRAP: Агент добавил ссылку на проект "ради одного extension-метода".
    // GUARDRAIL: Граф ProjectReference (через .csproj) не содержит циклов.
    // NOTE: Альтернатива — рефлексия Assembly.GetReferencedAssemblies() для runtime-графа.
    [Test]
    public async Task ProjectReferences_ShouldNotHaveCycles()
    {
        var solutionRoot = FindSolutionRoot();
        if (solutionRoot is null)
            Assert.Fail("Solution root not found");

        var projects = Directory.GetFiles(solutionRoot!, "*.csproj", SearchOption.AllDirectories);
        var graph = BuildProjectGraph(projects);
        var cycles = FindCycles(graph);

        await Assert.That(cycles).IsEmpty()
            .Because($"Circular project references detected: {string.Join(" | ", cycles)}");
    }

    // TRAP: Агент внёс межслоевой using в "косметическом" рефакторинге.
    // GUARDRAIL: NetArchTest ловит реальные IL-зависимости типов, а не строки в файлах.
    // NOTE: Это дублирует ArchitectureRules.Domain_ShouldNotDependOn_Infrastructure
    //       как специфичный guard для дрейфа графа. Оба теста могут сосуществовать.
    [Test]
    public async Task Domain_ShouldNotDependOn_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureAssembly.GetName().Name!)
            .GetResult();

        await Assert.That(result.IsSuccessful).IsTrue()
            .Because("Domain layer must not depend on Infrastructure layer");
    }

    // TRAP: Агент добавил using Infrastructure в Domain — regex дал false negative/positive.
    // GUARDRAIL: Рефлексия сборок подтверждает отсутствие runtime-ссылок.
    // NOTE: Assembly reference graph — runtime view, в отличие от .csproj (build intent).
    [Test]
    public async Task DomainAssembly_ShouldNotReference_InfrastructureRuntime()
    {
        var domainRefs = DomainAssembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var infraName = InfrastructureAssembly.GetName().Name;

        await Assert.That(domainRefs).DoesNotContain(infraName!)
            .Because("Domain assembly must not reference Infrastructure assembly at runtime");
    }

    private static string? FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6; i++)
        {
            if (dir is null) break;
            if (dir.GetFiles("*.sln").Any() || dir.GetDirectories("src").Any())
                return dir.FullName;
            dir = dir.Parent;
        }
        return null;
    }

    private static Dictionary<string, List<string>> BuildProjectGraph(string[] projectFiles)
    {
        var graph = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var proj in projectFiles)
        {
            var projName = Path.GetFileNameWithoutExtension(proj);
            graph[projName] = new List<string>();

            var doc = XDocument.Load(proj);
            var ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;

            foreach (var reference in doc.Descendants(ns + "ProjectReference"))
            {
                var include = reference.Attribute("Include")?.Value;
                if (!string.IsNullOrEmpty(include))
                {
                    var refName = Path.GetFileNameWithoutExtension(include);
                    graph[projName].Add(refName);
                }
            }
        }

        return graph;
    }

    private static List<string> FindCycles(Dictionary<string, List<string>> graph)
    {
        var cycles = new List<string>();
        var visited = new HashSet<string>();
        var recStack = new HashSet<string>();

        foreach (var node in graph.Keys)
        {
            if (!visited.Contains(node))
                Dfs(node, graph, visited, recStack, new List<string>(), cycles);
        }

        return cycles;
    }

    private static void Dfs(string node, Dictionary<string, List<string>> graph,
        HashSet<string> visited, HashSet<string> recStack, List<string> path, List<string> cycles)
    {
        visited.Add(node);
        recStack.Add(node);
        path.Add(node);

        foreach (var neighbor in graph.GetValueOrDefault(node) ?? new List<string>())
        {
            if (!visited.Contains(neighbor))
            {
                Dfs(neighbor, graph, visited, recStack, path, cycles);
            }
            else if (recStack.Contains(neighbor))
            {
                var cycleStart = path.IndexOf(neighbor);
                var cycle = path.Skip(cycleStart).ToList();
                cycle.Add(neighbor);
                cycles.Add(string.Join(" → ", cycle));
            }
        }

        path.RemoveAt(path.Count - 1);
        recStack.Remove(node);
    }
}
