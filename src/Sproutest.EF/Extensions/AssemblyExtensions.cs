using System.Reflection;

namespace Sproutest.EF.Extensions;

internal static class AssemblyExtensions
{
    public static IEnumerable<Type> GetClassesByGenericInterface(
        this Assembly assembly,
        Type interfaceType)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));
    }
}
