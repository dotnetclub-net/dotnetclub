#addin nuget:?package=Cake.DoInDirectory
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

Task("ci")
   .IsDependentOn("build-all")
   .IsDependentOn("cs-test");

Task("build-all")
   .IsDependentOn("build-prod")
   .IsDependentOn("build-test")
  .Does(() =>
    {
        Information("Build successfully.");
    });

Task("build-prod")
  .Does(() =>
    {
        DoInDirectory("./src/Discussion.Web/", () =>
        {
            using(var process = StartAndReturnProcess("dotnet", new ProcessSettings{ Arguments = "build"}))
            {
                process.WaitForExit();
                var code = process.GetExitCode();
                if(code != 0)
                    throw new Exception($"dotnet build returned a non-zero code: {code}");
            }
        });
    });

Task("build-test")
  .Does(() =>
    {
        DoInDirectory("./test/Discussion.Web.Tests/", () =>
        {
            using(var process = StartAndReturnProcess("dotnet", new ProcessSettings{ Arguments = "build"}))
            {
                process.WaitForExit();
                var code = process.GetExitCode();
                if(code != 0)
                    throw new Exception($"dotnet build returned a non-zero code: {code}");
            }
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

RunTarget(target);