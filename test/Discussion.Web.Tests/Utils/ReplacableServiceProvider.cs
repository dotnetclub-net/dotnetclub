using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Discussion.Web.Tests
{
    public class ReplacableServiceProvider : IServiceProvider, IServiceScope
    {
        private readonly IServiceProvider _systemProvider;
        private static ServiceProvider _replacingProvider;

        public ReplacableServiceProvider(IServiceProvider systemProvider)
            : this(systemProvider, true)
        {
        }

        ReplacableServiceProvider(IServiceProvider systemProvider, bool shouldReset)
        {
            _systemProvider = systemProvider;
            if (shouldReset)
            {
                Reset();
            }
        }
        
        public object GetService(Type serviceType)
        {   
            var replaced = _replacingProvider.GetService(serviceType);
            return replaced ?? _systemProvider.GetService(serviceType);
        }
        

        public static void Replace(IServiceCollection services)
        {
            _replacingProvider = services.BuildServiceProvider();
        }
        
        public static void Replace(Action<IServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices?.Invoke(services);
            Replace(services);
        }
        
        public static void Reset()
        {
            Replace(new ServiceCollection());
        }


        public void Dispose()
        {
            Reset();
        }

        public IServiceProvider ServiceProvider => this;

        public IServiceScopeFactory CreateScopeFactory()
        {
            return new ScopeFactory(this);
        }



        class ScopeFactory: IServiceScopeFactory
        {
            private readonly ReplacableServiceProvider _replacableServiceProvider;
            public ScopeFactory(ReplacableServiceProvider replacableServiceProvider)
            {
                _replacableServiceProvider = replacableServiceProvider;
            }
            
            public IServiceScope CreateScope()
            {
                var scope = _replacableServiceProvider._systemProvider.CreateScope();
                return new ReplacableServiceProvider(scope.ServiceProvider, false);
            }
        }
    }


}