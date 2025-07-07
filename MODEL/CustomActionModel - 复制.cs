using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class CustomActionModel
    {
        public int Service;
        public int ID;
        public string Name;
        public string SpName;
        public int ParameterNum;

        public CustomActionModel(int service, int iD, string name, string spName, int parameterNum)
        {
            Service = service;
            ID = iD;
            Name = name;
            SpName = spName;
            ParameterNum = parameterNum;
        }
        public CustomActionModel(int service,string name, string spName, int parameterNum)
        {
            Service = service;
            Name = name;
            SpName = spName;
            ParameterNum = parameterNum;
        }
        public CustomActionModel()
        {
        }
    }
}
