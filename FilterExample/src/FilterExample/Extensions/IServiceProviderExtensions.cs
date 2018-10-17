using System;

namespace FilterExample.Extensions
{
    public static class IServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider instance)
            where T : class
        {
            return instance.GetService(typeof(T)) as T;
        }
    }
}
