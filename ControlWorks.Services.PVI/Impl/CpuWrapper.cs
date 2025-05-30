﻿using BR.AN.PviServices;
using ControlWorks.Services.PVI.Panel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ControlWorks.Common;

namespace ControlWorks.Services.PVI.Impl
{

    public interface ICpuWrapper
    {
        void Initialize(IEnumerable<CpuInfo> cpuList);
        void CreateCpu(CpuInfo cpuInfo);
        void DisconnectCpu(CpuInfo info);
        CpuDetailResponse GetCpuByName(CpuInfo info);
        List<CpuDetailResponse> GetAllCpus(IEnumerable<CpuInfo> cpuInfo);
        List<string> GetCpuNames();
        void Reconnect(CpuInfo cpuInfo);

    }

    public class CpuWrapper : ICpuWrapper
    {
        private readonly Service _service;
        private readonly IEventNotifier _eventNotifier;
        private int _initialCount;
        private Dictionary<string, CpuInfo> _cpuInfoLookup;

        public CpuWrapper(Service service, IEventNotifier eventNotifier)
        {
            _service = service;
            _eventNotifier = eventNotifier;
            _cpuInfoLookup = new Dictionary<string, CpuInfo>();
        }

        public void Initialize(IEnumerable<CpuInfo> cpuList)
        {
            if (cpuList != null)
            {
                var list = new List<CpuInfo>(cpuList);
                _initialCount = list.Count;

                foreach (var cpuInfo in list)
                {
                    CreateCpu(cpuInfo);
                }
            }
        }

        public void Reconnect(CpuInfo cpuInfo)
        {
            Cpu cpu = null;

            if (_service.Cpus.ContainsKey(cpuInfo.Name))
            {
                cpu = _service.Cpus[cpuInfo.Name];
                if (!cpu.IsConnected || cpu.HasError)
                {
                    CreateCpu(cpuInfo);
                }
            }
            else
            {
                CreateCpu(cpuInfo);
            }
        }

        public List<string> GetCpuNames()
        {
            var list = new List<string>();
            foreach (Cpu cpu in _service.Cpus.Values)
            {
                list.Add(cpu.Name);
            }

            return list;
        }

        private void Connect(CpuInfo cpuInfo)
        {
            try
            {
                if (!_service.Cpus.ContainsKey(cpuInfo.Name))
                {
                    var cpu = new Cpu(_service, cpuInfo.Name);

                    if (ConfigurationProvider.CpuConnectionDeviceType.Equals("TcpIp",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        // Visual Studio Mode
                        cpu.Connection.DeviceType = DeviceType.TcpIp;
                        cpu.Connection.TcpIp.SourceStation = ConfigurationProvider.SourceStationId;
                        cpu.Connection.TcpIp.DestinationIpAddress = cpuInfo.IpAddress;
                    }
                    else if (ConfigurationProvider.CpuConnectionDeviceType.Equals("ANSL",
                                 StringComparison.OrdinalIgnoreCase))
                    {
                        // Panel Mode
                        cpu.Connection.DeviceType = DeviceType.ANSLTcp;
                        cpu.Connection.ANSLTcp.DestinationIpAddress = cpuInfo.IpAddress;
                    }

                    cpu.Connected += Cpu_Connected;
                    cpu.Error += Cpu_Error;
                    cpu.Disconnected += Cpu_Disconnected;

                    cpu.Connect();
                }
            }
            catch (System.Exception e)
            {
                Trace.TraceError(e.Message);
            }
        }

        public void CreateCpu(CpuInfo cpuInfo)
        {
            if (!_cpuInfoLookup.ContainsKey(cpuInfo.Name))
            {
                _cpuInfoLookup.Add(cpuInfo.Name, cpuInfo);
            }

            Cpu cpu = null;
            if (_service.Cpus.ContainsKey(cpuInfo.Name))
            {
                DisconnectCpu(cpuInfo);
            }
            else
            {
                Connect(cpuInfo);
            }

        }
        public void DisconnectCpu(CpuInfo info)
        {
            if (_service.Cpus.ContainsKey(info.Name))
            {
                Cpu cpu = _service.Cpus[info.Name];
                _service.Cpus.Remove(cpu);
            }
        }

        private void Cpu_Disconnected(object sender, PviEventArgs e)
        {
            String ipAddress = String.Empty;

            if (sender is Cpu cpu)
            {
                if (_service.Cpus.ContainsKey(cpu.Name))
                {
                    ipAddress = cpu.Connection.TcpIp.DestinationIpAddress;
                    cpu.Dispose();
                }
            }

            var pviEventMsg = Utils.FormatPviEventMessage($"ServiceWrapper.Cpu_Disconnected. IpAddress={ipAddress}", e);
            _eventNotifier.OnCpuDisconnected(sender, new PviApplicationEventArgs() { Message = pviEventMsg });
        }

        private void Cpu_Error(object sender, PviEventArgs e)
        {
            String ipAddress = String.Empty;

            if (sender is Cpu cpu)
            {
                ipAddress = cpu.Connection.TcpIp.DestinationIpAddress;
            }

            var pviEventMsg = Utils.FormatPviEventMessage($"ServiceWrapper.Cpu_Error. IpAddress={ipAddress} ", e);
            _eventNotifier.OnCpuError(sender, new PviApplicationEventArgs() { Message = pviEventMsg });
        }

        private void Cpu_Connected(object sender, PviEventArgs e)
        {
            String ipAddress = String.Empty;

            Cpu cpu = sender as Cpu;

            if (sender != null)
            {
                ipAddress = cpu.Connection.TcpIp.DestinationIpAddress;
            }

            var pviEventMsg = Utils.FormatPviEventMessage($"ServiceWrapper.Cpu_Connected. IpAddress={ipAddress}", e);
            _eventNotifier.OnCpuConnected(sender, new CpuConnectionArgs(cpu, pviEventMsg));
        }

        public List<CpuDetailResponse> GetAllCpus(IEnumerable<CpuInfo> cpuInfo)
        {
            var list = new List<CpuDetailResponse>();

            foreach (var info in cpuInfo)
            {
                var response = GetCpuByName(info);
                if (response != null)
                {
                    list.Add(response);
                }
            }

            return list;
        }

        public CpuDetailResponse GetCpuByName(CpuInfo info)
        {
            if (info != null && _service.Cpus.ContainsKey(info.Name))
            {
                return Map(info, _service.Cpus[info.Name]);
            }

            return null;
        }

        private CpuDetailResponse Map(CpuInfo setting, Cpu cpu)
        {
            var detail = new CpuDetailResponse
            {
                Description = setting.Description,
                HasError = cpu.HasError,
                IpAddress = setting.IpAddress,
                IsConnected = cpu.IsConnected,
                Name = setting.Name,
                Error = cpu.HasError ? new CpuError { ErrorCode = cpu.ErrorCode, ErrorText = cpu.ErrorText } : null
            };

            return detail;
        }

    }
}
