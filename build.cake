#addin nuget:?package=Cake.DoInDirectory
#addin "Cake.Npm"

var target = Argument("target", "Default");
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

Task("build-prod")
  .Does(() =>
    {
        DoInDirectory("./src/Discussion.Web/", () =>
        {
            Execute("dotnet build");
        });
    });

Task("build-test")
  .Does(() =>
    {
        DoInDirectory("./test/Discussion.Web.Tests/", () =>
        {
            Execute("dotnet build");
        });
    });

Task("build-web")
  .Does(() => {
        
        DoInDirectory("./src/Discussion.Web/wwwroot", () =>
        {
            Execute("npm install gulp-cli -g");
            Execute("npm install bower -g");

            NpmInstall();

            Execute("bower install");
            Execute("gulp");
        });
    });

Task("cs-test")
  .Does(() =>

    {
        DoInDirectory("./test/Discussion.Web.Tests/", () =>
        {
            DotNetCoreTest();
        });
    });


Task("package")
  .WithCriteria(() => !IsRunningOnWindows())
  .Does(() =>
    {
        DoInDirectory("./src/Discussion.Web/", () =>
        {
            Execute("dotnet publish -c Release -o publish");
        });
 
        var now = DateTime.UtcNow.ToString("yyyyMMddHHmm");
        var imageTag = $"jijiechen/dotnetclub:{now}"; 
        Execute($"docker build ./src/Discussion.Web/publish -t {imageTag}");
    });



Task("build-all")
   .IsDependentOn("build-prod")
   .IsDependentOn("build-test")
   .IsDependentOn("build-web");

Task("ci")
   .IsDependentOn("build-all")
   .IsDependentOn("cs-test")
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