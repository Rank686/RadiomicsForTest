using System;
using Xunit;

namespace Radiomics.Net.Tests;

internal static class TestAssert
{
    public static void AreEqual(double expected, double actual, double tolerance, string? message = null)
    {
        Assert.True(!double.IsNaN(actual), message ?? $"Expected {expected:F6} but got NaN.");
        Assert.True(Math.Abs(expected - actual) <= tolerance, message ?? $"Expected {expected:F6} ± {tolerance:E2}, but got {actual:F6}.");
    }

    public static void AreEqual(int expected, int actual, string? message = null)
    {
        Assert.True(expected == actual, message ?? $"Expected {expected}, but got {actual}.");
    }

    public static void IsTrue(bool condition, string? message = null)
    {
        Assert.True(condition, message ?? "Expected condition to be true.");
    }

    public static void IsFalse(bool condition, string? message = null)
    {
        Assert.False(condition, message ?? "Expected condition to be false.");
    }
}
