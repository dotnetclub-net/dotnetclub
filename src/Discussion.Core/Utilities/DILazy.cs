using System;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Core.Utilities
{
    public class DILazy<T> : Lazy<T> where T : class
    {
        public DILazy(IServiceProvider provider) : base(() => provider.GetRequiredService<T>()) { }
    }

    public static class LazySupportDIExtensions
    {
        public static void AddLazySupport(this IServiceCollection services)
        {
            services.AddTransient(typeof(Lazy<>), typeof(DILazy<>));
        }
    }
}