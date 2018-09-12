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

Task("ci")
   .IsDependentOn("build-all")
   .IsDependentOn("cs-test")
   .Does(() =>
    {
        DoInDirectory("./src/Discussion.Web/wwwroot", () =>
        {
            Execute("npm install gulp-cli -g");
            Execute("npm install bower -g");

            NpmInstall();

            Execute("bower install");
            Execute("gulp");
        });
    });

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

Task("cs-test")
  .Does(() =>

    {
        DoInDirectory("./test/Discussion.Web.Tests/", () =>
        {
            DotNetCoreTest();
        });
    });



void Execute(string command){
    var indexOfSpace = command.IndexOf(" ");
    var args = new ProcessSettings();
    var commandName = command;
    if (IsRunningOnWindows())
    {
        commandName = "powershell.exe";
        args.Arguments = "-Command \'" + command + "\'";
    }else{       
        if(indexOfSpace > -1){
            args.Arguments = command.Substring(indexOfSpace+1);
        }
        commandName = indexOfSpace > -1 ? command.Substring(0, indexOfSpace)  : command;
    }
    Information($"Executing {command}");
    using(var process = StartAndReturnProcess(commandName, args))
    {
        process.WaitForExit();
        var code = process.GetExitCode();
        if(code != 0)
            throw new Exception($"${commandName} returned a non-zero code: {code}");
    }
}



RunTarget(target);