using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace TheBattleCats.Content.NPCs.NyandamBoss
{
    public class NyandamBlock : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            // this.MineResist = float.MaxValue;
            // this.MinPick = int.MaxValue;
            AddMapEntry(new Color(100, 100, 100));

            // Prevent block from dropping when broken
            RegisterItemDrop(-1);
        }

        public override bool CanExplode(int i, int j) => false;
    }
}
