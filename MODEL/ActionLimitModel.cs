using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class ActionLimitModel
    {
        public ActionLimitModel(int service, int iD, int actionCode, string name, string spName, string introduce)
        {
            Service = service;
            ID = iD;
            ActionCode = actionCode;
            Name = name;
            SpName = spName;
            Introduce = introduce;
        }

        public int Service { get; set; }
        public int ID { get; set; }
        public int ActionCode { get; set; }
        public string Name { get; set; }
        public string SpName { get; set; }
        public string Introduce { get; set; }
    }
}
