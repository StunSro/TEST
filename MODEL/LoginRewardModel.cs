using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class LoginRewardModel
    {
        public int LoginDay;
        public int SilkReward;
        public int SLBReward;

        public LoginRewardModel(int loginDay, int silkReward, int sLBReward)
        {
            LoginDay = loginDay;
            SilkReward = silkReward;
            SLBReward = sLBReward;
        }
    }
}
