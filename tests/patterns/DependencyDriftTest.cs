// TRAP: Агент добавил +1 using или ProjectReference — диф выглядит безобидно,
// но породил циклическую зависимость между слоями или проектами.
// GUARDRAIL: NetArchTest + парсинг .csproj + Assembly-рефлексия ловят нарушения графа.
// Аналогично работает для C++ #include или любого import-графа.

using System.Reflection;
using System.Xml.Linq;
using NetArchTest.Rules;
using TUnit;

namespace Tests.Patterns;

public class DependencyDriftTest
{
    // TRAP: Агент добавил ссылку на проект "ради одного extension-метода".
    // GUARDRAIL: Граф ProjectReference не содержит циклов.
    [Test]
    public void ProjectReferences_ShouldNotHaveCycles()
    {
        var solutionRoot = FindSolutionRoot();
        if (solutionRoot is null)
            Assert.Fail("Solution root not found");

        var projects = Directory.GetFiles(solutionRoot!, "*.csproj", SearchOption.AllDirectories);
        var graph = BuildProjectGraph(projects);
        var cycles = FindCycles(graph);

        Assert.That(cycles).IsEmpty()
            .Because($"Circular project references detected: {string.Join(" | ", cycles)}");
    }

    // TRAP: Агент внёс межслоевой using в "косметическом" рефакторинге.
    // GUARDRAIL: NetArchTest ловит реальные IL-зависимости, а не строки в файлах.
    // NOTE: Замените "MyProject.Domain" и "MyProject.Infrastructure" на свои сборки.
    //       Для чистой рефлексии см. DomainAssembly_ShouldNotReference_InfrastructureRuntime ниже.
    [Test]
    public void Domain_ShouldNotDependOn_Infrastructure()
    {
        // Адаптация: загрузите свои сборки через typeof(DomainType).Assembly
        // var domain = typeof(YourDomainEntity).Assembly;
        // var infra = typeof(YourInfrastructureService).Assembly;
        //
        // var result = Types.InAssembly(domain)
        //     .ShouldNot()
        //     .HaveDependencyOnAny(infra.GetName().Name!)
        //     .GetResult();
        //
        // Assert.That(result.IsSuccessful).IsTrue();

        Assert.That(true).IsTrue()
            .Because("Template: adapt this test to your assembly names. See commented code.");
    }

    // TRAP: Regex-сканирование using-ов даёт false positive на мёртвый код.
    // GUARDRAIL: Assembly.GetReferencedAssemblies() показывает runtime-граф, а не текст.
    // NOTE: Замените на свои сборки.
    [Test]
    public void DomainAssembly_ShouldNotReference_InfrastructureRuntime()
    {
        // Адаптация:
        // var domainRefs = typeof(YourDomainEntity).Assembly
        //     .GetReferencedAssemblies()
        //     .Select(a => a.Name)
        //     .ToHashSet(StringComparer.OrdinalIgnoreCase);
        //
        // var infraName = typeof(YourInfrastructureService).Assembly.GetName().Name;
        // Assert.That(domainRefs).DoesNotContain(infraName!);

        Assert.That(true).IsTrue()
            .Because("Template: adapt this test to your assembly names. See commented code.");
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
