using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Discussion.Tests.Common {

    public static class TestEnv
    {
        public static string SolutionPath()
        {
            const string testProjectSubPath = "test/Discussion.Web.Tests/Discussion.Web.Tests.csproj";
            var currentPath = Directory.GetCurrentDirectory();

            do
            {
                var testProjectPath = Path.Combine(currentPath, testProjectSubPath).NormalizeToAbsolutePath();
                if (File.Exists(testProjectPath))
                {
                    return currentPath;
                }

                var parent = Directory.GetParent(currentPath);
                currentPath = parent == null ? null : parent.FullName;
            } while (currentPath != null);

            throw new InvalidOperationException("Cannot find the Discussion.Web.Tests test project.");
        }

        public static string WebProjectPath()
        {
            return Path.Combine(SolutionPath(), "src/Discussion.Web").NormalizeToAbsolutePath();
        }

        public static string RuntimeLauncherPath()
        {
            var isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var envVarSeparateChar = isWindows ? ';' : ':';
            var commandName = isWindows ? "dotnet.exe" : "dotnet";

            return FindFileThoughEnvironmentVariables(commandName, envVarSeparateChar);
        }

        private static string FindFileThoughEnvironmentVariables(string executableName, char envVarSeparateChar)
        {
            foreach (string envPath in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(envVarSeparateChar))
            {
                var path = envPath.Trim();
                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path = Path.Combine(path, executableName)))
                {
                    return Path.GetFullPath(path);
                }
            }

            throw new Exception("Runtime not detected on the machine.");
        }

        private static string NormalizeToAbsolutePath(this string relativePath)
        {
            return Path.GetFullPath(relativePath.NormalizeSeparatorChars());
        }

        public static string NormalizeSeparatorChars(this string path)
        {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
