using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class MonsterKillRewardModel
    {
        public int Service;
        public int ID;
        public string Name;
        public int SilkOwnReward;
        public int SLBReward;
        public int RewardProbability;
        public int NoticeType;
        public string NoticeMessage;
        public MonsterKillRewardModel() {
        }
        public MonsterKillRewardModel(int service, int iD, string name, int silkOwnReward, int sLBReward, int rewardProbability, int noticeType, string noticeMessage)
        {
            Service = service;
            ID = iD;
            Name = name;
            SilkOwnReward = silkOwnReward;
            SLBReward = sLBReward;
            RewardProbability = rewardProbability;
            NoticeType = noticeType;
            NoticeMessage = noticeMessage;
        }
    }
}
