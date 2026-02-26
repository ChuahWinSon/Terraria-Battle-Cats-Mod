using Terraria;
using Terraria.ModLoader;
using TheBattleCats.Content.NPCs.NyandamBoss; // make sure this contains your boss NPC class

public class NetworkyandamGlobalTile : GlobalTile
{
    public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
    {
        // Check if the tile is nyandamBlock
        if (type == ModContent.TileType<NyandamBlock>())
        {
            // If the boss is alive, block breaking
            if (NPC.AnyNPCs(ModContent.NPCType<Nyandam>()))
            {
                return false; // can't break while boss is alive
            }
        }

        return base.CanKillTile(i, j, type, ref blockDamaged);
    }
}
