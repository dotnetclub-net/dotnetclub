#addin nuget:?package=Cake.DoInDirectory
#addin "Cake.Npm"

var target = Argument("target", "Default");
var imagetag = Argument("imagetag", string.Empty);

// ./build.sh --target=build-all

// Available methods: https://github.com/cake-build/cake/blob/develop/src/Cake.Core/Scripting/ScriptHost.cs
Task("Default")
    .Does(() =>
        {
          Information("Please specify a target to run:");
          Information("-------------------------------");

          foreach(var task in Tasks){
              Information($"{task.Name}\t\t\t\t{task.Description}");
          }
        });

Task("build")
  .Does(() =>
    {
        DoInDirectory("./src/Discussion.Web/", () =>
        {
            Execute("dotnet build");
        });

        DoInDirectory("./src/Discussion.Web/wwwroot", () =>
        {
            Execute("yarn install");
        });
        DoInDirectory("./src/Discussion.Web/wwwroot/lib", () =>
        {
            Execute("yarn install");
        });
        DoInDirectory("./src/Discussion.Web/wwwroot", () =>
        {
            Execute("npm run clean");
            Execute("npm run prod");
        });


        DoInDirectory("./src/Discussion.Admin/", () =>
        {
            Execute("dotnet build");
        });
        DoInDirectory("./src/Discussion.Admin/ClientApp", () =>
        {
            Execute("yarn install");
            Execute("npm run build");
        });
    });

Task("test")
  .Does((context) =>
    {
        var isLinux = context.Environment.Platform.Family == Cake.Core.PlatformFamily.Linux;
        var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TRAVIS"));

        DoInDirectory("./test/Discussion.Web.Tests/", () =>
        {
            if(isLinux && isCI){
                Execute("dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover");
            }else{
                DotNetCoreTest();
            }
        });

        DoInDirectory("./src/Discussion.Admin/ClientApp", () =>
        {
            Execute("npm run test-coverage");
        });
    });


Task("package")
  .WithCriteria(() => !IsRunningOnWindows())
  .Does((context) =>
    {
        DoInDirectory("./src/Discussion.Web/", () =>
        {
            Execute("dotnet publish -c Release -o publish");
        });
        DoInDirectory("./src/Discussion.Admin/", () =>
        {
            Execute("dotnet publish -c Release -o publish");
        });
 
        var isMac = context.Environment.Platform.Family == Cake.Core.PlatformFamily.OSX;
        var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TRAVIS"));
        var skipDocker = isCI && isMac;

        if(!skipDocker){
            if(string.IsNullOrWhiteSpace(imagetag)){
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                imagetag = $"jijiechen/dotnetclub:{now}";
            } 

            CopyFile("./DockerFile", "./src/Discussion.Web/publish/DockerFile");
            
            CopyFile("./upgrade-from-existing.sh", "./src/Discussion.Web/publish/upgrade-from-existing.sh ");
            CopyFile("src/Discussion.Migrations/bin/Release/netcoreapp2.1/Discussion.Migrations.deps.json", "./src/Discussion.Web/publish/Discussion.Migrations.deps.json");
            CopyFile("src/Discussion.Migrations/bin/Release/netcoreapp2.1/Discussion.Migrations.runtimeconfig.json", "./src/Discussion.Web/publish/Discussion.Migrations.runtimeconfig.json");

            Execute($"docker build ./src/Discussion.Web/publish -t {imagetag} -f ./src/Discussion.Web/publish/DockerFile");
        }
    });



Task("ci")
   .IsDependentOn("build")
   .IsDependentOn("test")
   .IsDependentOn("package");


void Execute(string command, string workingDir = null){
    
 if (string.IsNullOrEmpty(workingDir))
        workingDir = System.IO.Directory.GetCurrentDirectory();

    System.Diagnostics.ProcessStartInfo processStartInfo;

    if (IsRunningOnWindows())
    {
        processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            UseShellExecute = false,
            WorkingDirectory = workingDir,
            FileName = "cmd",
            Arguments = "/C \"" + command + "\"",
        };
    }
    else
    {
        processStartInfo = new System.Diagnostics.ProcessStartInfo
        {
            UseShellExecute = false,
            WorkingDirectory = workingDir,
            FileName = "bash",
            Arguments = "-c \"" + command + "\"",
        };
    }

    using (var process = System.Diagnostics.Process.Start(processStartInfo))
    {
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception(string.Format("Exit code {0} from {1}", process.ExitCode, command));
    }
}



RunTarget(target);