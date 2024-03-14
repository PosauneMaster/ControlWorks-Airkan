using BR.AN.PviServices;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Panel;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Exception = System.Exception;

namespace ControlWorks.Services.PVI
{
    public class PollingService
    {
        private Service _service;
        ICpuManager _cpuManager;
        private CancellationTokenSource _cts;
        private System.Threading.Tasks.Task _pollingTask;

        public PollingService(Service service, ICpuManager cpuManager)
        {
            _service = service;
            _cpuManager = cpuManager;
            _cts = new CancellationTokenSource();
        }

        public void Start()
        {
            _pollingTask = new System.Threading.Tasks.Task(Poll, _cts.Token, TaskCreationOptions.LongRunning);
            _pollingTask.Start();
        }

        public void Stop()
        {
            _cts.Cancel();
            _pollingTask.Wait();
        }

        private async void Poll()
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(30));

            CancellationToken token = _cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            while (!token.WaitHandle.WaitOne(interval))
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    await System.Threading.Tasks.Task.Run(() => _cpuManager.Reconnect());
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"PollingService.Poll", ex);
                }

                interval = TimeSpan.FromMilliseconds(ConfigurationProvider.PollingMilliseconds);
            }
        }
    }
}
