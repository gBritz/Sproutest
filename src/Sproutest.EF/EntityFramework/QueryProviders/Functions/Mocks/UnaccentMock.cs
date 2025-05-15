using Microsoft.EntityFrameworkCore;
using Sproutest.EF.Extensions;
using System.Reflection;

namespace Sproutest.EF.EntityFramework.QueryProviders.Functions.Mocks;

internal class UnaccentMock
{
    // TODO: verificar se tem consultas em linq no EF que utilzam funções
    public static readonly MethodInfo Original = typeof(NpgsqlFullTextSearchDbFunctionsExtensions)
        .GetMethod(nameof(NpgsqlFullTextSearchDbFunctionsExtensions.Unaccent), new[] { typeof(DbFunctions), typeof(string) })
            ?? throw new MissingMemberException($"Cannot get method {nameof(NpgsqlDbFunctionsExtensions.ILike)} from {nameof(NpgsqlDbFunctionsExtensions)}.");

    public static readonly MethodInfo Replacement = typeof(UnaccentMock)
        .GetMethod(nameof(UnaccentMock.Unaccent))
            ?? throw new MissingMemberException($"Cannot get method {nameof(UnaccentMock.Unaccent)} from {nameof(UnaccentMock)}.");

    public static string Unaccent(DbFunctions functions, string text)
        => text.RemoveDiacritics();
}