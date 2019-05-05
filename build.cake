#addin nuget:?package=Cake.DoInDirectory
#addin "Cake.Npm"

var target = Argument("target", "Default");
var imagetag = Argument("imagetag", string.Empty);

var collectCoverageData = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("COLLECT_COVERAGE_DATA"));
var dockerExists = false;
try{ Execute("docker --version"); dockerExists = true; }catch{ }



// ./build.sh --target=ci

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

Task("clean")
  .Does(() =>{
        var directoriesToDelete = new DirectoryPath[]{
            Directory("./src/Discussion.Web/publish"),
            Directory("./src/Discussion.Web/wwwroot/dist"),
            Directory("./src/Discussion.Admin/publish"),
            Directory("./src/Discussion.Admin/ClientApp/dist")
        }.Where(DirectoryExists).ToArray();

        DeleteDirectories(directoriesToDelete, new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });

        DotNetCoreClean("./dotnetclub.sln");
    });

Task("build:web")
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
            Execute("npm run dev");
            Execute("npm run prod");
        });
    });

Task("test:web")
  .Does((context) =>
    {
        DoInDirectory("./test/Discussion.Web.Tests/", () =>
        {
            if(collectCoverageData){
                Execute("dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:Exclude=\\\"[*]Discussion.Tests.Common.*\\\"");
            }else{
                DotNetCoreTest();
            }
        });
    });


Task("package:web")
  .WithCriteria(() => !IsRunningOnWindows())
  .Does((context) =>
    {
        DoInDirectory("./src/Discussion.Web/", () =>
        {
            Execute("dotnet publish -c Release -o publish");
        });
        CopyFile("src/Discussion.Migrations/bin/Release/netcoreapp2.1/Discussion.Migrations.deps.json", "./src/Discussion.Web/publish/Discussion.Migrations.deps.json");
        CopyFile("src/Discussion.Migrations/bin/Release/netcoreapp2.1/Discussion.Migrations.runtimeconfig.json", "./src/Discussion.Web/publish/Discussion.Migrations.runtimeconfig.json");

        if(dockerExists){
            if(string.IsNullOrWhiteSpace(imagetag)){
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                imagetag = $"jijiechen/dotnetclub:{now}";
            }

            Execute($"docker build ./src/Discussion.Web/publish -t {imagetag} -f ./src/Discussion.Web/publish/Dockerfile");
        }
    });

Task("build:admin")
  .Does(() =>
    {
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

Task("test:admin")
  .Does((context) =>
    {
        DoInDirectory("./test/Discussion.Admin.Tests/", () =>
        {
            if(collectCoverageData){
                Execute("dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover /p:Exclude=\\\"[*]Discussion.Tests.Common.*\\\"");
            }else{
                DotNetCoreTest();
            }
        });

        DoInDirectory("./src/Discussion.Admin/ClientApp", () =>
        {
            Execute("npm run test-coverage");
        });
    });


Task("package:admin")
  .WithCriteria(() => !IsRunningOnWindows())
  .Does((context) =>
    {
        DoInDirectory("./src/Discussion.Admin/", () =>
        {
            Execute("dotnet publish -c Release -o publish");
        });

        if(dockerExists){
            var adminTag = imagetag;
            if(string.IsNullOrWhiteSpace(adminTag)){
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                adminTag = $"jijiechen/dotnetclub-adm:{now}";
            }else{
                adminTag = adminTag.Replace("dotnetclub:", "dotnetclub-adm:");
            }
            
            Execute($"docker build ./src/Discussion.Admin/publish -t {adminTag} -f ./src/Discussion.Admin/publish/Dockerfile");
        }
    });


Task("ci:web")
   .IsDependentOn("clean")
   .IsDependentOn("build:web")
   .IsDependentOn("test:web")
   .IsDependentOn("package:web");

Task("ci:admin")
   .IsDependentOn("clean")
   .IsDependentOn("build:admin")
   .IsDependentOn("test:admin")
   .IsDependentOn("package:admin");

Task("ci")
   .IsDependentOn("ci:web")
   .IsDependentOn("ci:admin");








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