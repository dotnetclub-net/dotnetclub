using System.Net;
using System.Diagnostics;
using System.IO;
using Xunit;
using System;

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
                    var httpWebRequest = WebRequest.CreateHttp("http://localhost:" + httpListenPort.ToString());
                    response = httpWebRequest.GetResponse() as HttpWebResponse;
                }
                catch(WebException ex)
                {
                    response = ex.Response as HttpWebResponse;
                }
                finally
                {
                    testCompleted = true;
                    dnxWebServer.Kill();
                }
            }, () => testCompleted);

            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
        }

        private void StartWebApp(int port, Action<Process> onServerReady, Func<bool> testSuccessed)
        {
            var args = Environment.GetCommandLineArgs();

            var dnxPath = args[0];
            var appBaseIndex = Array.IndexOf(args, "--appbase");
            var testPath = args[appBaseIndex + 1];
            var webProject = Path.Combine(testPath, "../../src/Discussion.Web");

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
                    throw new Exception("Server is down unexpectedly.");
            };

            dnxWebServer.Start();
            dnxWebServer.BeginErrorReadLine();
            dnxWebServer.BeginOutputReadLine();
            dnxWebServer.WaitForExit(20 * 1000);
        }

    }
}
