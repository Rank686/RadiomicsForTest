using System;

namespace Radiomics.Net.Tests;

internal static class TestAssert
{
    public static void AreEqual(double expected, double actual, double tolerance, string? message = null)
    {
        if (double.IsNaN(actual) || Math.Abs(expected - actual) > tolerance)
        {
            throw new InvalidOperationException(message ?? $"Expected {expected:F6} ± {tolerance}, but got {actual:F6}.");
        }
    }

    public static void AreEqual(int expected, int actual, string? message = null)
    {
        if (expected != actual)
        {
            throw new InvalidOperationException(message ?? $"Expected {expected}, but got {actual}.");
        }
    }

    public static void IsTrue(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message ?? "Expected condition to be true.");
        }
    }

    public static void IsFalse(bool condition, string? message = null)
    {
        if (condition)
        {
            throw new InvalidOperationException(message ?? "Expected condition to be false.");
        }
    }
}
