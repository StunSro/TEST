using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class ShardModel
    {
        public ShardModel(int id, string dBName, string name, int capacity)
        {
            Id = id;
            DBName = dBName;
            Name = name;
            Capacity = capacity;
        }

        public int Id { get; set; }
        public string DBName { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
    }
}
