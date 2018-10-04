using System.Net;
using System.Diagnostics;
using Xunit;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using static Discussion.Web.Tests.TestEnv;
using System.Text.RegularExpressions;

namespace Discussion.Web.Tests.StartupSpecs
{
    public class BootStrapSpecs
    {
        [Fact]
        public void should_bootstrap_success()
        {
            var testCompleted = false;
            HttpWebResponse response = null;

            var httpPort = GetAvailablePort();
            StartWebApp(httpPort, (runningProcess) =>
            {
                try
                {
                    Console.WriteLine("Server started successfully, trying to request...");
                    var httpWebRequest = WebRequest.CreateHttp("http://localhost:" + httpPort.ToString());
                    response = httpWebRequest.GetResponseAsync().Result as HttpWebResponse;
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                finally
                {
                    testCompleted = true;

                    RunningDotnetProcess.TryKillProcess(runningProcess.WorkerProcessId);
                    RunningDotnetProcess.TryKillProcess(runningProcess.HostProcessId);
                }
            }, () => testCompleted);
            
            if (response == null)
            {
                throw new Exception("Can not launch the web server process!");
            }
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        private void StartWebApp(int port, Action<RunningDotnetProcess> onServerReady, Func<bool> testSuccessed)
        {
            var dotnetPath = RuntimeLauncherPath();
            var webProject = WebProjectPath();

            var dotnetProcess = new ProcessStartInfo
            {
                FileName = dotnetPath,
                Arguments = "run --environment Integration --urls http://localhost:" + port,
                WorkingDirectory = webProject,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            dotnetProcess.Environment["DOTNET_CLI_CONTEXT_VERBOSE"] = "true";
            Console.WriteLine($"dotnet command is: {dotnetPath}{Environment.NewLine}\nStarting web site at: {webProject}");

            string outputData = string.Empty, errorOutput = string.Empty;
            var startedSuccessfully = false;
            int workerProcessId = 0;
            var serverHostProcess = new Process { StartInfo = dotnetProcess };


            serverHostProcess.OutputDataReceived += (sender, e) =>
            {
                if (startedSuccessfully)
                {
                    return;
                }

                outputData += e.Data;
                if (workerProcessId == 0 && outputData.Contains("Process ID:"))
                {
                    workerProcessId = int.Parse(Regex.Match(outputData, @"Process ID: (\d+)").Groups[1].Value);
                }
                if (outputData.Contains("Now listening on") && outputData.Contains("Application started."))
                {
                    startedSuccessfully = true;
                    onServerReady.Invoke(new RunningDotnetProcess { HostProcessId = serverHostProcess.Id, WorkerProcessId = workerProcessId });
                }
            };
            serverHostProcess.ErrorDataReceived += (sender, e) =>
            {
                errorOutput += e.Data;
            };


            serverHostProcess.EnableRaisingEvents = true;
            serverHostProcess.Exited += (sender, e) =>
            {
                if (!testSuccessed())
                {
                    var msg = $"Cannot launch a server for the website. \nError output:{errorOutput}\nStandard output:{outputData}";
                    throw new Exception(msg);
                }
            };

            serverHostProcess.Start();
            serverHostProcess.BeginErrorReadLine();
            serverHostProcess.BeginOutputReadLine();
            var exited = serverHostProcess.WaitForExit(30 * 1000);
            if (!exited)
            {
                RunningDotnetProcess.TryKillProcess(serverHostProcess.Id);
                RunningDotnetProcess.TryKillProcess(workerProcessId);
            }
        }

        private int GetAvailablePort()
        {
            bool IsPortTaken(int port)
            {
                return IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpConnections()
                    .Any(conn => conn.LocalEndPoint.Port == port);
            }

            var httpPort = 5010;
            while (IsPortTaken(httpPort))
            {
                httpPort++;
            }

            return httpPort;
        }
    }



    class RunningDotnetProcess
    {
        public int HostProcessId { get; set; }
        public int WorkerProcessId { get; set; }


        public static Process GetProcess(int id)
        {
            try
            {
                return Process.GetProcessById(id);
            }
            catch
            {
                return null;
            }
        }


        public static void TryKillProcess(int id)
        {
            if (id < 1)
            {
                return;
            }
            
            var process = GetProcess(id);
            if (process != null)
            {
                try
                {
                    
                    process.Kill();
                }
                catch { }
            }            
        }
    }

}
