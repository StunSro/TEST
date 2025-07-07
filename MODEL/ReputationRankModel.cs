using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.MODEL
{
    public class ReputationRankModel
    {
        public ReputationRankModel(int ranking, int rank, string charName, int charCurLevel, int charMaxLevel, int graduateCount, string guildName, int guildID)
        {
            Ranking = ranking;
            Rank = rank;
            CharName = charName;
            CharCurLevel = charCurLevel;
            CharMaxLevel = charMaxLevel;
            GraduateCount = graduateCount;
            GuildName = guildName;
            GuildID = guildID;
        }

        public int Ranking { get; set; }
        public int Rank { get; set; }
        public string  CharName { get; set; }
        public int CharCurLevel { get; set; }
        public int CharMaxLevel { get; set; }
        public int GraduateCount { get; set; }
        public string GuildName { get; set; }
        public int GuildID { get; set; }

    }
}
