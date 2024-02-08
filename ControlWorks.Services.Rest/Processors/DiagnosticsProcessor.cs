using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Pvi;
using ControlWorks.Services.Rest.Models;

using LiteDB;

namespace ControlWorks.Services.Rest.Processors
{
    public interface IDiagnosticsProcessor
    {
        Task<HealthCheckItem> GetHealthCheck();
        void UpdateStatus(string status);
    }

    public class DiagnosticsProcessor : IDiagnosticsProcessor
    {
        private readonly string _status = "status";


        private IPviApplication _pviApplication;


        public DiagnosticsProcessor()
        {
        }

        public DiagnosticsProcessor(IPviApplication pviApplication)
        {
            _pviApplication = pviApplication;

        }
        public void UpdateStatus(string status)
        {
            //try
            //{
            //    using (var db = new LiteDatabase(ConfigurationProvider.SmartConveyorDbConnectionString))
            //    {
            //        var statusCol = db.GetCollection<SystemStatus>(_status);

            //        if (statusCol != null)
            //        {
            //            var systemStatus = statusCol.FindAll().FirstOrDefault();
            //            if (systemStatus == null)
            //            {
            //                var statusItem =  new SystemStatus("Unknown");
            //                statusCol.Insert(statusItem);
            //            }
            //            else
            //            {
            //                systemStatus.Status = status;
            //                statusCol.Update(systemStatus);
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Trace.TraceError($"DiagnosticsProcessor.UpdateStatus. {ex.Message}\r\n", ex);
            //    throw;
            //}

        }

        public async Task<HealthCheckItem> GetHealthCheck()
        {

            try
            {
                var statusInfo = await Task.Run(() => _pviApplication.GetStatusInfo());

                var healthCheckItem = new HealthCheckItem();
                healthCheckItem.ActiveAlarms = statusInfo.ActiveAlarms;
                healthCheckItem.ActiveRecipes = statusInfo.ActiveOrders;
                healthCheckItem.Status = statusInfo.SystemStatus;

                return healthCheckItem;

            }
            catch (Exception ex)
            {
                Trace.TraceError($"DiagnosticsProcessor.GetHealthCheck. {ex.Message}\r\n", ex);
                throw;
            }
        }
    }
}
