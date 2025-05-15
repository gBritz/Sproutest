namespace Sproutest.EF.Extensions;

internal static class TaskExtensions
{
  public static Task<T> AsTask<T>(this T instance) => Task.FromResult(instance);
}