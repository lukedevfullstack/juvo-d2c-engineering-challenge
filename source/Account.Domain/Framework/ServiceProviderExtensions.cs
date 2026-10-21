using System;

namespace Account.Domain.Framework
{
    internal static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this IServiceProvider serviceProvider) where T : class
        {
            var service = serviceProvider.GetService(typeof(T)) as T;
            if (service == null)
                throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");

            return service;
        }
    }
}
