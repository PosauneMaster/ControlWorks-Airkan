using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.PVI.Models
{
    public class AirkanInputFileInfo
    {
        public AirkanInputFileInfo()
        {
        }

        public AirkanInputFileInfo(int index, string path)
        {
            Index = index;
            Path = path;
        }

        public int Index { get; set; }
        public string Path { get; set; }
    }
}
