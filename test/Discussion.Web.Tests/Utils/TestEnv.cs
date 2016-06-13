using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.IO;
using System.Linq;

namespace Discussion.Web.Tests {

    public static class TestEnv
    {
        public static string TestProjectPath()
        {
            // return PlatformServices.Default.Application.ApplicationBasePath;
            var args = Environment.GetCommandLineArgs();
            var appBaseIndex = Array.IndexOf(args, "--appbase");

            var path = appBaseIndex >= 0 ? args[appBaseIndex + 1] : Environment.CurrentDirectory;
            return path.NormalizeToAbsolutePath();
        }

        public static string WebProjectPath()
        {
            return Path.Combine(TestProjectPath(), "../../src/Discussion.Web").NormalizeToAbsolutePath();
        }

        public static string RuntimeLauncherPath()
        {
            var isWindows = PlatformServices.Default.Runtime.OperatingSystemPlatform == Platform.Windows;
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
