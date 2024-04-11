using System;
using BR.AN.PviServices;

namespace ControlWorks.Services.PVI
{
    public class HeartBeatService
    {
        public HeartBeatService() { }

        public void Run(Variable heartBeatVariable)
        {
            if (heartBeatVariable != null)
            {
                if (heartBeatVariable.IsConnected)
                {
                    heartBeatVariable.ValueChanged += (sender, args) =>
                    {
                        if (sender is Variable variable)
                        {
                            if (variable.Name == "Heartbeat")
                            {
                                if (variable.Value.ToBoolean(null) == false)
                                {
                                    System.Threading.Thread.Sleep(1000);
                                    variable.Value.Assign(true);
                                    variable.WriteValue();
                                }
                            }
                        }
                    };
                }
            }

        }
    }
}
