using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using TheBattleCats.Content.Items.Weapons;

namespace TheBattleCats.Content.NPCs
{
    public class TimeGolem : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.GraniteGolem];
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.GraniteGolem); // Copies all stats, AI, and behavior
            AIType = NPCID.GraniteGolem; // Ensures AI matches the Granite Golem
            AnimationType = NPCID.GraniteGolem; // Uses same animation
        }


        public override void HitEffect(NPC.HitInfo hit)
        {
            // Optional: Add visual effects on hit or death
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ChronosWatch>(), 10));
            // ^ 10 means 1 in 10 chance = 10%
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (!spawnInfo.PlayerSafe && spawnInfo.Player.ZoneMarble && spawnInfo.Player.ZoneRockLayerHeight)
            {
                return 0.05f; // 5% spawn chance in Marble biome
            }

            return 0f;
        }



    }
}
