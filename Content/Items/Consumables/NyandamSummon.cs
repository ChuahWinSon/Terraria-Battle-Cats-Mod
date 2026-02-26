using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace TheBattleCats.Content.Items.Consumables
{
    public class NyandamSummon : ModItem
    {


        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 999;
            Item.rare = ItemRarityID.Orange;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.consumable = false;
        }
    }
}
