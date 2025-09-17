using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Radiomics.Net.Tests;
using Xunit;

var testAssembly = typeof(ImagePreprocessingTests).Assembly;
var factAttributeType = typeof(FactAttribute);
var failures = new List<string>();
var total = 0;

foreach (var type in testAssembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract))
{
    object? instance = null;
    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
    {
        if (!method.GetCustomAttributes(factAttributeType, inherit: true).Any())
        {
            continue;
        }

        if (method.GetParameters().Length != 0)
        {
            failures.Add($"{type.FullName}.{method.Name}: xUnit stub only supports parameterless [Fact] methods.");
            continue;
        }

        total++;
        try
        {
            if (!method.IsStatic)
            {
                instance ??= Activator.CreateInstance(type);
            }

            method.Invoke(instance, null);
            Console.WriteLine($"[PASS] {type.Name}.{method.Name}");
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException ?? ex;
            failures.Add($"{type.FullName}.{method.Name}: {inner.Message}");
            Console.WriteLine($"[FAIL] {type.Name}.{method.Name}\n{inner}");
        }
        catch (Exception ex)
        {
            failures.Add($"{type.FullName}.{method.Name}: {ex.Message}");
            Console.WriteLine($"[FAIL] {type.Name}.{method.Name}\n{ex}");
        }
    }
}

Console.WriteLine();
if (failures.Count == 0)
{
    Console.WriteLine($"All {total} test(s) passed.");
    return 0;
}

Console.WriteLine($"{failures.Count} of {total} test(s) failed:");
foreach (var failure in failures)
{
    Console.WriteLine($" - {failure}");
}

return 1;
