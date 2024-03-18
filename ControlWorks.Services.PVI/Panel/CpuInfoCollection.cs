using ControlWorks.Common;

using LiteDB;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Linq;
namespace ControlWorks.Services.PVI.Panel
{
    public interface ICpuInfoCollection
    {
        CpuInfo FindByName(string name);
        CpuInfo FindByIp(string ip);
        bool Add(CpuInfo cpu);
        List<CpuInfo> GetAll();
        void AddRange(IEnumerable<CpuInfo> cpuList);
        void Remove(CpuInfo cpu);
    }

    public class CpuInfoCollection : ICpuInfoCollection
    {
        private static readonly AutoResetEvent CpuInfoEvent = new AutoResetEvent(true);
        private readonly string _cpuSettingsName = "CpuSettings";

        public CpuInfoCollection()
        {
        }

        public CpuInfo FindByName(string name)
        {
            return GetAll().
                    FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public CpuInfo FindByIp(string ip)
        {
            return GetAll().
                FirstOrDefault(c => c.IpAddress.Equals(ip, StringComparison.OrdinalIgnoreCase));
        }

        public bool Add(CpuInfo cpu)
        {
            bool inserted = false;
            CpuInfoEvent.WaitOne(1000);

            try
            {
                var cpuInfo = GetAll();
                if (cpuInfo == null)
                {
                    cpuInfo = new List<CpuInfo>();
                }
                var cpuExists = cpuInfo.FirstOrDefault(c => c.IpAddress == cpu.IpAddress);
                if (cpuExists == null)
                {
                    cpuInfo.Add(cpu);
                    inserted = true;
                }

                var cpuSettings = JsonConvert.SerializeObject(cpuInfo, Formatting.Indented);
                File.WriteAllText(ConfigurationProvider.CpuSettings, cpuSettings);
            }
            catch (Exception e)
            {
                Trace.TraceError($"CpuInfoCollection.Add. {e.Message} \r\n", e);
            }
            finally
            {
                CpuInfoEvent.Set();
            }

            return inserted;
        }


        public List<CpuInfo> GetAll()
        {
            CpuInfoEvent.WaitOne(1000);
            List<CpuInfo> cpuList = new List<CpuInfo>();

            try
            {
                if (File.Exists(ConfigurationProvider.CpuSettings))
                {
                    var json = File.ReadAllText(ConfigurationProvider.CpuSettings);
                    cpuList = JsonConvert.DeserializeObject<List<CpuInfo>>(json);
                }
                else
                {
                    Trace.TraceInformation("CpuInfoCollection.GetAll. CpuSetting file not initialized");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"CpuInfoCollection.GetAll. {e.Message} \r\n", e);
            }
            finally
            {
                CpuInfoEvent.Set();
            }
            return cpuList;
        }

        public void AddRange(IEnumerable<CpuInfo> cpuList)
        {
            foreach (var cpu in cpuList)
            {
                Add(cpu);
            }
        }

        public void Remove(CpuInfo cpu)
        {
            CpuInfoEvent.WaitOne(1000);

            try
            {
                var settingsDb = ConfigurationProvider.ControlworksSettingsDbConnectionString;
                using (var db = new LiteDatabase(settingsDb))
                {
                    var cpuInfoCol = db.GetCollection<CpuInfo>(_cpuSettingsName);
                    var currentCpu = cpuInfoCol.Find(c => c.IpAddress == cpu.IpAddress).FirstOrDefault();
                    if (currentCpu != null)
                    {
                        cpuInfoCol.Delete(currentCpu.Id);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"CpuInfoCollection.Remove. {e.Message} \r\n", e);
            }
            finally
            {
                CpuInfoEvent.Set();
            }
        }
    }
}
