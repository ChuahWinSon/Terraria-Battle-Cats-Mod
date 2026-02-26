using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using TheBattleCats.Content.NPCs.NyandamBoss;
using TheBattleCats.Content.Items.Consumables;
using Terraria.ObjectData;


namespace TheBattleCats.Content.Tiles
{
    public class NyandamAltar : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolidTop[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3); // 3 tiles wide, 3 tiles tall
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 }; // Each row is 16 pixels high
            TileObjectData.addTile(Type);


            // AddMapEntry(new Color(200, 50, 100), "Nyandam Altar");
        }

public override bool RightClick(int i, int j)
{
    Player player = Main.LocalPlayer;

    if (NPC.AnyNPCs(ModContent.NPCType<Nyandam>()))
    {
        Main.NewText("The air trembles... something's already here.", 255, 0, 0);
        return true;
    }

    if (player.HeldItem.type == ModContent.ItemType<NyandamSummon>())
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            int originX = i - tile.TileFrameX / 18 % 3;
            int originY = j - tile.TileFrameY / 18 % 3;

            Vector2 altarCenter = new Vector2(originX + 1.5f, originY + 1.5f) * 16f;
            Vector2 spawnPos = altarCenter - new Vector2(0f, 0f);

            int npcIndex = NPC.NewNPC(null, (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<Nyandam>());
            //delay so player doesnt take dmg
            Main.npc[npcIndex].ai[3] = 60f;
            Main.NewText("Nyandam has been awakened!", 200, 0, 200);

            Vector2 knockDir = Vector2.Normalize(player.Center - spawnPos);
            player.velocity = new Vector2(knockDir.X * 10f, -20f);
            player.immune = true;
            player.immuneTime = 60;
        }

        player.ConsumeItem(ModContent.ItemType<NyandamSummon>());
    }
    else
    {
        Main.NewText("Something seems to be missing...", 180, 180, 180);
    }

    return true;
}
    }
}
