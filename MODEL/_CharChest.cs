using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class _CharChest
    {
        public int LineNum { get; set; } = 0;
        public uint RefItemID { get; set; } = 0;
        public int Count { get; set; } = 0;
        public byte OptLevel { get; set; } = 0;
        public bool RandomizedStats { get; set; } = false;
        public string From { get; set; } = "";
        public int ShardID { get; set; } = 0;
        public DateTime RegisterTime { get; set; }
    }
}
