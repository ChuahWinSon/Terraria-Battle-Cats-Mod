using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using TheBattleCats.Content.NPCs.NyandamBoss;
using TheBattleCats.Content.Items.Placeable;
using Terraria.ObjectData;
using TheBattleCats.Content.Items.Consumables;

namespace TheBattleCats.Content.Tiles
{
    public class CatCapsuleMachine : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolidTop[Type] = true;  // lets player stand on top
            Main.tileTable[Type] = true;     // treated as a table/surface
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);

        }

        public override bool RightClick(int i, int j)
        {
            CatCapsuleUISystem.Open();
            return true;
        }
    }
}