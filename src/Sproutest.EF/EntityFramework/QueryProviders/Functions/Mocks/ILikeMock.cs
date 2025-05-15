using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Sproutest.EF.EntityFramework.QueryProviders.Functions.Mocks;

internal static class ILikeMock
{
    public static readonly MethodInfo Original = typeof(NpgsqlDbFunctionsExtensions)
        .GetMethod(nameof(NpgsqlDbFunctionsExtensions.ILike), new[] { typeof(DbFunctions), typeof(string), typeof(string) })
            ?? throw new MissingMemberException($"Cannot get method {nameof(NpgsqlDbFunctionsExtensions.ILike)} from {nameof(NpgsqlDbFunctionsExtensions)}.");

    public static readonly MethodInfo Replacement = typeof(ILikeMock)
        .GetMethod(nameof(ILikeMock.ILike))
            ?? throw new MissingMemberException($"Cannot get method {nameof(ILikeMock.ILike)} from {nameof(ILikeMock)}.");

    public static bool ILike(DbFunctions functions, string matchExpression, string pattern)
    {
        var percentCount = pattern.Count(x => x == '%');
        return percentCount switch
        {
            0
                => matchExpression.Equals(pattern, StringComparison.InvariantCultureIgnoreCase),
            1 when pattern.StartsWith('%')
                => matchExpression.EndsWith(pattern[1..], StringComparison.InvariantCultureIgnoreCase),
            1 when pattern.EndsWith('%')
                => matchExpression.StartsWith(pattern[1..], StringComparison.InvariantCultureIgnoreCase),
            2 when pattern.StartsWith('%') && pattern.EndsWith('%')
                => matchExpression.Contains(pattern[1..^1], StringComparison.InvariantCultureIgnoreCase),
            _
                => throw new ArgumentException($"Pattern '${pattern}' is not supported.", nameof(pattern)),
        };
    }
}