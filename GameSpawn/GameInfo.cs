using SR_PROXY.MODEL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_PROXY.GameSpawn
{
    class GameInfo
    {
        public struct Items_
        {
            public List<uint> id;
            public List<string> type;
        }
        public static Items_ Items = new Items_();

        public struct Mobs_
        {
            public List<uint> id;
            public List<string> type;
        }
        public static Mobs_ Mobs = new Mobs_();
        public struct Skills_
        {
            public List<uint> id;
            public List<string> type;
        }
        public static Skills_ Skills = new Skills_();

        public struct Types_
        {
            public List<string> grab_types;
            public List<string> grabpet_item_types;
            public List<string> attack_types;
            public List<string> attack_item_types;
        }
        public static Types_ Types = new Types_();
        private static void InitializeTypes()
        {
            //Attack Pets
            Types.attack_types = new List<string>();
            Types.attack_types.Add("COS_P_BEAR");
            Types.attack_types.Add("COS_P_FOX");
            Types.attack_types.Add("COS_P_PENGUIN");
            Types.attack_types.Add("COS_P_WOLF_WHITE_SMALL");
            Types.attack_types.Add("COS_P_WOLF_WHITE");
            Types.attack_types.Add("COS_P_WOLF");
            Types.attack_types.Add("COS_P_JINN");
            Types.attack_types.Add("COS_P_KANGAROO");
            Types.attack_types.Add("COS_P_RAVEN");
            //Attack Pets

            //Attack Pets Item
            Types.attack_item_types = new List<string>();
            Types.attack_item_types.Add("ITEM_COS_P_BEAR_SCROLL");
            Types.attack_item_types.Add("ITEM_COS_P_FOX_SCROLL");
            Types.attack_item_types.Add("ITEM_COS_P_PENGUIN_SCROLL");
            Types.attack_item_types.Add("ITEM_COS_P_FLUTE_WHITE_SMALL");
            Types.attack_item_types.Add("ITEM_COS_P_FLUTE_WHITE");
            Types.attack_item_types.Add("ITEM_COS_P_FLUTE");
            Types.attack_item_types.Add("ITEM_COS_P_FLUTE_SILK");
            Types.attack_item_types.Add("ITEM_COS_P_JINN_SCROLL");
            Types.attack_item_types.Add("ITEM_COS_P_KANGAROO_SCROLL");
            Types.attack_item_types.Add("ITEM_COS_P_RAVEN_SCROLL");
            //Attack Pets Item



            //Grab Pets
            Types.grab_types = new List<string>();
            Types.grab_types.Add("COS_P_SPOT_RABBIT");
            Types.grab_types.Add("COS_P_RABBIT");
            Types.grab_types.Add("COS_P_GGLIDER");
            Types.grab_types.Add("COS_P_MYOWON");
            Types.grab_types.Add("COS_P_SEOWON");
            Types.grab_types.Add("COS_P_RACCOONDOG");
            Types.grab_types.Add("COS_P_CAT");
            Types.grab_types.Add("COS_P_BROWNIE");
            Types.grab_types.Add("COS_P_PINKPIG");
            Types.grab_types.Add("COS_P_GOLDPIG");
            Types.grab_types.Add("COS_P_WINTER_SNOWMAN");
            //Grab Pets

            //Grab Pets Item
            Types.grabpet_item_types = new List<string>();
            Types.grabpet_item_types.Add("ITEM_COS_P_SPOT_RABBIT_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_RABBIT_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_RABBIT_SCROLL_SILK");
            Types.grabpet_item_types.Add("ITEM_COS_P_GGLIDER_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_MYOWON_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_SEOWON_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_RACCOONDOG_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_CAT_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_BROWNIE_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_PINKPIG_SCROLL");
            Types.grabpet_item_types.Add("ITEM_EVENT_COS_P_PINKPIG_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_GOLDPIG_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_GOLDPIG_SCROLL_SILK");
            Types.grabpet_item_types.Add("ITEM_EVENT_COS_P_GOLDPIG_SCROLL");
            Types.grabpet_item_types.Add("ITEM_COS_P_WINTER_SNOWMAN_SCROLL");
            Types.grabpet_item_types.Add("ITEM_EVENT_COS_P_WINTER_SNOWMAN_SCROLL");
            //Grab Pets Item
        }
        public static void InitializeLists()
        {
            Items.id = new List<uint>(new uint[] { 0 });
            Items.type = new List<string>(new string[] { "ITEM_UNKNOWN" });

            Skills.id = new List<uint>();
            Skills.type = new List<string>();

            Mobs.id = new List<uint>();
            Mobs.type = new List<string>();
            InitializeTypes();
        }
        public static List<ReputationRankModel> ReputationRanks = new List<ReputationRankModel>();
    }
}
