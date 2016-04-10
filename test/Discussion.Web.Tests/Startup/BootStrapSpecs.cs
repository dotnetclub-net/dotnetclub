using System.Net;
using System.Diagnostics;
using System.IO;
using Xunit;
using System;
using static Discussion.Web.Tests.Utils.TestEnv;

namespace Discussion.Web.Tests.Startup
{
    public class BootStrapSpecs
    {
        [Fact]
        public void should_bootstrap_success()
        {
            const int httpListenPort = 5000;
            var testCompleted = false;
            HttpWebResponse response = null;

            StartWebApp(httpListenPort, (dnxWebServer) =>
            {
                try
                {
                    Console.WriteLine("Server started successfully, trying to request...");
                    var httpWebRequest = WebRequest.CreateHttp("http://localhost:" + httpListenPort.ToString());
                    response = httpWebRequest.GetResponse() as HttpWebResponse;
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
                    dnxWebServer.Kill();
                }
            }, () => testCompleted);

            if(response == null)
            {
                Console.WriteLine("Error: Response object is not assigned.");
            }

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        private void StartWebApp(int port, Action<Process> onServerReady, Func<bool> testSuccessed)
        {
            var args = Environment.GetCommandLineArgs();

            var dnxPath = DnxPath();
            var appBaseIndex = Array.IndexOf(args, "--appbase");
            var webProject = WebProjectPath();

            var dnxWeb = new ProcessStartInfo
            {
                FileName = dnxPath,
                Arguments = "Microsoft.AspNet.Server.Kestrel --server.urls http://localhost:" + port.ToString(),
                WorkingDirectory = webProject,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                LoadUserProfile = true,
                UseShellExecute = false
            };
            Console.WriteLine($"dnx command is: {dnxPath}{Environment.NewLine}\nStarting web site at: {webProject}");

            string outputData = string.Empty, errorOutput = string.Empty;
            var startedSuccessfully = false;
            var dnxWebServer = new Process { StartInfo = dnxWeb };


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
                    onServerReady.BeginInvoke(dnxWebServer, null, null);
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
}
