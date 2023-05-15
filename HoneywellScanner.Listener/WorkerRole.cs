using Microsoft.Azure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HoneywellScanner.Listener
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);


        public WorkerRole()
        {
        }


        public override void Run()
        {
            Trace.TraceInformation("HoneywellScanner.Listener is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 500;

            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("HoneywellScanner.Listener has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HoneywellScanner.Listener is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("HoneywellScanner.Listener has stopped");
        }

        private static string[] settings = {
            "Api.Url", "Api.Username", "Api.Password",
            "HoneywellScanner.Ip", "HoneywellScanner.Port", "HoneywellScanner.ActivityTimeout",
            "ApplicationInsights.ConnectionString",
        };


        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var args = new Dictionary<string, string>();
            foreach(var key in settings)
               args[key] = CloudConfigurationManager.GetSetting(key);
            await ScannerListener.RunAsync(args, cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
                await Task.Delay(1000);
        }
    }
}
