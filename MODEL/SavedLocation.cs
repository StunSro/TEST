using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class SavedLocation
    {
        public int region { get; set; } = 0;
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int z { get; set; } = 0;
        public string locationName { get; set; } = string.Empty;
    }
}
