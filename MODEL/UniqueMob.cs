using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class UniqueMob
    {
        public int PageID { get; set; } = 0;
        public byte LineID { get; set; } = 0;
        public byte Status { get; set; } = 0;
        public string Killer { get; set; } = "";
        public string Time { get; set; } = "";
        public int UniqueID { get; set; } = 0;
        public int RegionID { get; set; } = 0;
        public int PostionX { get; set; } = 0;
        public int PostionZ { get; set; } = 0;
        public int CircaleRadius { get; set; } = 0;
        public int Image { get; set; } = 0;
    }
}
