using Microsoft.AspNet.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Xunit;

namespace Discussion.Web.Tests.StartupSpecs
{
    public class SyncFileProviderSpecs
    {


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

        static void MockRuntime(string runtimeType)
        {
            var currentPlatformService = PlatformServices.Default;
            var runtime = new StubRuntimeEnvironment(runtimeType);

            var customPlatformService = PlatformServices.Create(currentPlatformService, currentPlatformService.Application, runtime);
            PlatformServices.SetDefault(customPlatformService);
        }


        class StubRuntimeEnvironment : IRuntimeEnvironment
        {
            string _runtimeType;
            public StubRuntimeEnvironment(string runtimeType)
            {
                _runtimeType = runtimeType;
            }


            public string OperatingSystem
            {
                get
                {
                    return "Windows";
                }
            }

            public string OperatingSystemVersion
            {
                get
                {
                    return "10.0";
                }
            }

            public string RuntimeArchitecture
            {
                get
                {
                    return "x86";
                }
            }

            public string RuntimePath
            {
                get
                {
                    return "/mnt/C/dnx/bin";
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
                    return "1.0.0-rc1-16231";
                }
            }
        }
    }
}
