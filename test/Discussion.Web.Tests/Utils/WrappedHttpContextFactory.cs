using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Discussion.Web.Tests
{
    public class WrappedHttpContextFactory : IHttpContextFactory
    {
        IHttpContextFactory _contextFactory;
        Action<IFeatureCollection, HttpContext> _configureContextFeatures;
        public WrappedHttpContextFactory(IHttpContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }
        
        public void ConfigureFeatureWithContext(Action<IFeatureCollection, HttpContext> configureFeatures)
        {
            _configureContextFeatures = configureFeatures;
        }

        public HttpContext Create(IFeatureCollection contextFeatures)
        {
            var httpContext = _contextFactory.Create(contextFeatures);
            _configureContextFeatures?.Invoke(contextFeatures, httpContext);

            return httpContext;
        }

        public void Dispose(HttpContext httpContext)
        {
            _contextFactory.Dispose(httpContext);
        }
    }
}