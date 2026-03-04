using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheBattleCats.Content.Items;

namespace TheBattleCats.Common.GlobalNPCs
{
	public class CatFoodPickupGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc)
        {
            if (npc.lifeMax > 1 && npc.damage > 0 && Main.rand.NextBool(3))
            {
                Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<CatFood>());
            }
        }
	}
}
