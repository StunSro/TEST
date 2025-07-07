using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class Rank
    {
        public byte LineNum { get; set; } = 0;
        public string Guild { get; set; } = string.Empty;
        public string Points { get; set; } = string.Empty;
        public int ShardID { get; set; } = 0;

    }
}
