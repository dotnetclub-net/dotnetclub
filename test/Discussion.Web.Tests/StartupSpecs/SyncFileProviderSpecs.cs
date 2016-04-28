using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using Xunit;

namespace Discussion.Web.Tests.StartupSpecs
{
    public class SyncFileProviderSpecs: IDisposable
    {
        PlatformServices _defaultPlatformService;
        public SyncFileProviderSpecs()
        {
            _defaultPlatformService = PlatformServices.Default;
        }

        [Fact]
        public void should_use_synchronous_file_provider_on_mono()
        {
            // arrage
            MockRuntime("Mono");
            var applicationServices = ServicesSpecs.CreateApplicationServices();

            // act
            var razorOptions = applicationServices.GetRequiredService<IOptions<RazorViewEngineOptions>>();

            // assert
            razorOptions.Value.FileProvider.ShouldNotBeNull();
            Assert.IsType<WrappedSynchronousFileProvider>(razorOptions.Value.FileProvider);
        }


        [Fact]
        public void should_use_butin_physical_file_provider_on_non_mono()
        {
            // arrage
            MockRuntime("Clr");
            var applicationServices = ServicesSpecs.CreateApplicationServices();

            // act
            var razorOptions = applicationServices.GetRequiredService<IOptions<RazorViewEngineOptions>>();


            // assert
            razorOptions.Value.FileProvider.ShouldNotBeNull();

            var fileProviderType = razorOptions.Value.FileProvider.GetType().FullName;
            Assert.Equal("Microsoft.AspNet.FileProviders.PhysicalFileProvider", fileProviderType);
        }

        void MockRuntime(string runtimeType)
        {
            var runtime = new StubRuntimeEnvironment(runtimeType, _defaultPlatformService.Runtime);

            var customPlatformService = PlatformServices.Create(_defaultPlatformService, _defaultPlatformService.Application, runtime);
            PlatformServices.SetDefault(customPlatformService);
        }

        public void Dispose()
        {
            PlatformServices.SetDefault(_defaultPlatformService); 
        }

        class StubRuntimeEnvironment : IRuntimeEnvironment
        {
            string _runtimeType;
            IRuntimeEnvironment _originalEnv;
            public StubRuntimeEnvironment(string runtimeType, IRuntimeEnvironment originalEnv)
            {
                _runtimeType = runtimeType;
                _originalEnv = originalEnv;
            }


            public string OperatingSystem
            {
                get
                {
                    return _originalEnv.OperatingSystem;
                }
            }

            public string OperatingSystemVersion
            {
                get
                {
                    return _originalEnv.OperatingSystemVersion;
                }
            }

            public string RuntimeArchitecture
            {
                get
                {
                    return _originalEnv.RuntimeArchitecture;
                }
            }

            public string RuntimePath
            {
                get
                {
                    return _originalEnv.RuntimePath;
                }
            }

            public string RuntimeType
            {
                get
                {
                    return _runtimeType;
                }
            }

            public string RuntimeVersion
            {
                get
                {
                    return _originalEnv.RuntimeVersion;
                }
            }
        }
    }
}
