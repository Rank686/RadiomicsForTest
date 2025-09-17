using System;
using System.Collections.Generic;

namespace Radiomics.Net.Tests;

internal static class TestRunner
{
    private static readonly List<string> Failures = new();
    private static int _total;

    public static void Run(string name, Action test)
    {
        _total++;
        try
        {
            test();
            Console.WriteLine($"[PASS] {name}");
        }
        catch (Exception ex)
        {
            Failures.Add($"{name}: {ex.Message}");
            Console.WriteLine($"[FAIL] {name}\n{ex}");
        }
    }

    public static int Report()
    {
        Console.WriteLine();
        if (Failures.Count == 0)
        {
            Console.WriteLine($"All {_total} test(s) passed.");
            return 0;
        }

        Console.WriteLine($"{Failures.Count} of {_total} test(s) failed:");
        foreach (var failure in Failures)
        {
            Console.WriteLine($" - {failure}");
        }

        return 1;
    }
}
