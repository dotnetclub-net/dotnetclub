using System;
using Discussion.Web.Data;
using Discussion.Web.Data.InMemory;
using Jusfr.Persistent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;

namespace Discussion.Web.ApplicationSupport
{
    internal static class DataInfrastructure
    {
        internal static void AddDataServices(this IServiceCollection services, IConfiguration appConfiguration)
        {
            var ravenServerUrl = appConfiguration["ravenServerUrl"];
            var ravenDatabase = appConfiguration["ravenDbName"];
            
            if (string.IsNullOrWhiteSpace(ravenServerUrl))
            {
                var dataContext = new InMemoryResponsitoryContext();
                services.AddSingleton(typeof(IRepositoryContext), (serviceProvider) => dataContext);
                services.AddScoped(typeof(Repository<>), typeof(InMemoryDataRepository<>));
                services.AddScoped(typeof(IRepository<>), typeof(InMemoryDataRepository<>));
                return;
            }
            
            services.AddSingleton(new Lazy<IDocumentStore>(() =>
            {
                var store = new DocumentStore
                {
                    Urls = new[] {ravenServerUrl},
                    Database = ravenDatabase
                };
                store.Initialize();

                return store;
            }));

            services.AddScoped(typeof(IRepositoryContext), (serviceProvider) => {
                return new RavenRepositoryContext(() => serviceProvider.GetService<Lazy<IDocumentStore>>().Value);
            });
            services.AddScoped(typeof(Repository<>), typeof(RavenDataRepository<>));
            services.AddScoped(typeof(IRepository<>), typeof(RavenDataRepository<>));
        }

        internal static void SetupDisposing(IApplicationBuilder app)
        {
            var ravenStore = app.ApplicationServices.GetService<Lazy<IDocumentStore>>();
            if (ravenStore != null)
            {
                var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
                lifetime.ApplicationStopping.Register(() =>
                {
                    if (ravenStore.IsValueCreated && !ravenStore.Value.WasDisposed)
                    {
                        ravenStore.Value.Dispose();
                    }
                });
            }
        }
    }
}