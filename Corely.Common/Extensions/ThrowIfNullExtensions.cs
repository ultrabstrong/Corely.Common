﻿namespace Corely.Common.Extensions;

public static class ThrowIfNullExtensions
{
    public static T ThrowIfNull<T>(
        this T? obj,
        string paramName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(obj, paramName);
        return obj;
    }

    public static IEnumerable<T> ThrowIfAnyNull<T>(
        this IEnumerable<T?>? obj,
        string paramName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(obj, paramName);
        foreach (var value in obj)
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
        }
        return obj!;
    }

    public static string ThrowIfNullOrWhiteSpace(
        this string? str,
        string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(str, paramName);
        return str;
    }

    public static IEnumerable<string> ThrowIfAnyNullOrWhiteSpace(
        this IEnumerable<string>? obj,
        string paramName)
    {
        ArgumentNullException.ThrowIfNull(obj, paramName);
        foreach (var value in obj)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        }
        return obj;
    }

    public static string ThrowIfNullOrEmpty(
        this string? str,
        string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(str, paramName);
        return str;
    }

    public static IEnumerable<string> ThrowIfAnyNullOrEmpty(
        this IEnumerable<string>? obj,
        string paramName)
    {
        ArgumentNullException.ThrowIfNull(obj, paramName);
        foreach (var value in obj)
        {
            ArgumentException.ThrowIfNullOrEmpty(value, paramName);
        }
        return obj;
    }
}
