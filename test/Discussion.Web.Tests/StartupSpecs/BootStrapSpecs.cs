using System.Net;
using System.Diagnostics;
using Xunit;
using System;
using static Discussion.Web.Tests.TestEnv;
using System.Text.RegularExpressions;

namespace Discussion.Web.Tests.StartupSpecs
{
    public class BootStrapSpecs
    {
        [Fact]
        public void should_bootstrap_success()
        {
            const int httpListenPort = 5001;
            var testCompleted = false;
            HttpWebResponse response = null;

            StartWebApp(httpListenPort, (runningProcess) =>
            {
                try
                {
                    Console.WriteLine("Server started successfully, trying to request...");
                    var httpWebRequest = WebRequest.CreateHttp("http://localhost:" + httpListenPort.ToString());
                    response = httpWebRequest.GetResponseAsync().Result as HttpWebResponse;
                }
                catch (WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception thrown: {ex.Message}\n {ex.StackTrace}");
                    throw;
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
                Console.WriteLine("Error: Response object is not assigned.");
            }

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        private void StartWebApp(int port, Action<RunningDotnetProcess> onServerReady, Func<bool> testSuccessed)
        {
            var args = Environment.GetCommandLineArgs();

            var dotnetPath = RuntimeLauncherPath();
            var webProject = WebProjectPath();

            var dotnetProcess = new ProcessStartInfo
            {
                FileName = dotnetPath,
                Arguments = "run --environment Integration --server.urls http://localhost:" + port.ToString(),
                WorkingDirectory = webProject,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                // LoadUserProfile = true,   // will throw System.PlatformNotSupportedException : Operation is not supported on this platform.  see https://travis-ci.org/jijiechen/openaspnetorg/builds/140064540
                UseShellExecute = false
            };
            dotnetProcess.Environment["DOTNET_CLI_CONTEXT_VERBOSE"] = "true";
            Console.WriteLine($"dotnet command is: {dotnetPath}{Environment.NewLine}\nStarting web site at: {webProject}");

            string outputData = string.Empty, errorOutput = string.Empty;
            var startedSuccessfully = false;
            var dnxWebServer = new Process { StartInfo = dotnetProcess };


            dnxWebServer.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (startedSuccessfully)
                {
                    return;
                }

                outputData += e.Data;
                if (outputData.Contains("Now listening on") && outputData.Contains("Application started."))
                {
                    startedSuccessfully = true;
                    var workerProcessId = int.Parse(Regex.Match(outputData, @"Process ID: (\d+)").Groups[1].Value);
                    onServerReady.Invoke(new RunningDotnetProcess { HostProcessId = dnxWebServer.Id, WorkerProcessId = workerProcessId });
                };
            };
            dnxWebServer.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                errorOutput += e.Data;
            };


            dnxWebServer.EnableRaisingEvents = true;
            dnxWebServer.Exited += (object sender, EventArgs e) =>
            {
                if (!testSuccessed())
                {
                    Console.WriteLine($"Cannot launch a server for the website. \nError output:{errorOutput}\nStandard output:{outputData}");
                    throw new Exception("Server is down unexpectedly.");
                }
            };

            dnxWebServer.Start();
            dnxWebServer.BeginErrorReadLine();
            dnxWebServer.BeginOutputReadLine();
            dnxWebServer.WaitForExit(20 * 1000);
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
            var process = GetProcess(id);
            if (process != null)
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    
                }

                //process = GetProcess(id);
                //if(process != null)
                //{
                //    // did not kill
                //}
            }            
        }
    }

}
