using BR.AN.PviServices;

using ControlWorks.Services.PVI.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.PVI
{
    public class HeartBeatService
    {
        private Cpu _cpu;
        public HeartBeatService() { }
        public HeartBeatService(Cpu cpu)
        {
            _cpu = cpu;
        }
        public void HeartBeat()
        {
            if (_cpu == null)
            {
                return;
            }

            try
            {
                if (_cpu.Tasks.ContainsKey("Heartbeat"))
                {
                    Variable hb = _cpu.Tasks["DataTransf"].Variables["Heartbeat"];
                    if (hb != null)
                    {
                        hb.Connected += Hb_Connected;
                        hb.Active = true;
                        hb.Connect();
                    }
                }
            }
            catch (System.Exception e)
            {
            }

        }

        private void Hb_Connected(object sender, PviEventArgs e)
        {
            if (_cpu.Variables.ContainsKey("Heartbeat"))
            {
                Variable hb = _cpu.Variables["Heartbeat"];
                if (hb != null)
                {
                    hb.ValueChanged += Hb_ValueChanged;
                    System.Threading.Thread.Sleep(1000);
                    hb.Value.Assign(false);
                    hb.WriteValue();
                }
            }
        }

        private void Hb_ValueChanged(object sender, VariableEventArgs e)
        {
            if (_cpu.Variables.ContainsKey("Heartbeat"))
            {
                Variable hb = _cpu.Variables["Heartbeat"];
                if (hb != null)
                {
                    var v = hb.Value.ToBoolean(null);

                    if (v)
                    {
                        System.Threading.Thread.Sleep(1000);
                        hb.Value.Assign(false);
                        hb.WriteValue();
                    }
                }
            }
        }
    }
}
