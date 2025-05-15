using System.Runtime.CompilerServices;

namespace Sencinet.DigitalJourney.TestingLibrary.Utils
{
    public static class ActivatorHelper
    {
        public static T CreateUninitialized<T>()
        {
            return (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        }
    }
}
