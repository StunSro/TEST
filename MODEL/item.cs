using System;

namespace SR_PROXY.MODEL
{
    public class ItemType
    {
        public int ID { get; set; }

        public byte TypeID1 { get; set; }
        public byte TypeID2 { get; set; }
        public byte TypeID3 { get; set; }
        public byte TypeID4 { get; set; }
        public string CodeName128 { get; set; }
        public string NameStrID128 { get; set; }
        public byte Bionic { get; set; }       // (tinyint, not null)
        public byte Rarity { get; set; }       // (tinyint, not null)
        public byte ReqLevel1 { get; set; }    // (tinyint, not null)

        // Constructor (اختياري)
        public ItemType() { }

        public ItemType(int id,byte typeID1, byte typeID2, byte typeID3, byte typeID4,
                        string codeName128, string nameStrID128,
                        byte bionic, byte rarity, byte reqLevel1)
        {
            ID = id;
            TypeID1 = typeID1;
            TypeID2 = typeID2;
            TypeID3 = typeID3;
            TypeID4 = typeID4;
            CodeName128 = codeName128;
            NameStrID128 = nameStrID128;
            Bionic = bionic;
            Rarity = rarity;
            ReqLevel1 = reqLevel1;
        }
    }
}
