using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TheBattleCats.Content.NPCs.CycloneBoss;
using TheBattleCats.Common.Systems; 

namespace TheBattleCats.Content.Items.Consumables
{
	public class DebrisInABottle : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 12;
        }

        public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.value = 100;
			Item.rare = ItemRarityID.Blue;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = true;
		}

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
			itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossSpawners;
		}

		public override bool CanUseItem(Player player) {
			// If you decide to use the below UseItem code, you have to include !NPC.AnyNPCs(id), as this is also the check the server does when receiving MessageID.SpawnBoss.
			// If you want more constraints for the summon item, combine them as boolean expressions:
			//    return !Main.IsItDay() && !NPC.AnyNPCs(ModContent.NPCType<MinionBossBody>()); would mean "not daytime and no MinionBossBody currently alive"
			return !NPC.AnyNPCs(ModContent.NPCType<Cyclone>());
		}

		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				// If the player using the item is the client
				// (explicitly excluded serverside here)
				SoundStyle customSound = new SoundStyle("TheBattleCats/Assets/Effects/BossShockwave")
				{
    			Volume = 1.0f,  // Max volume
    			Pitch = 0.0f,   // Normal pitch
    			PitchVariance = 0.0f // Adds random pitch variation
				};

				// Play the sound at NPC's position
				SoundEngine.PlaySound(customSound, player.position);

				int type = ModContent.NPCType<Cyclone>();
                int spawnX = (int)player.Center.X + (player.direction * 200);
                int spawnY = (int)player.Center.Y - 200;

				if (Main.netMode != NetmodeID.MultiplayerClient) {
                // Spawn directly at player's center position (on-screen)
                int npcIndex = NPC.NewNPC(
                    NPC.GetBossSpawnSource(player.whoAmI),
                    spawnX,
                    spawnY, 
                    type
                );

                
            }
                            else {
					// If the player is in multiplayer, request a spawn
					// This will only work if NPCID.Sets.MPAllowedEnemies[type] is true, which we set in MinionBossBody
					NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, number: player.whoAmI, number2: type);
				}
			}

			return true;
		}
    }
}