using System;

namespace Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class FactAttribute : Attribute
{
}

public sealed class XunitException : Exception
{
    public XunitException(string message)
        : base(message)
    {
    }
}

public static class Assert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition)
        {
            throw new XunitException(message ?? "Expected condition to be true.");
        }
    }

    public static void False(bool condition, string? message = null)
    {
        if (condition)
        {
            throw new XunitException(message ?? "Expected condition to be false.");
        }
    }
}
