using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheBattleCats.Content.Mounts;

namespace TheBattleCats.Content.Items.Mounts
{
    public class CyberpunkChairItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width        = 36;
            Item.height       = 26;
            Item.useTime      = 20;
            Item.useAnimation = 20;
            Item.useStyle     = ItemUseStyleID.Swing;
            Item.mountType    = ModContent.MountType<CyberpunkChair>();
            Item.noMelee      = true;
            Item.value        = Item.sellPrice(gold: 10);
            Item.rare         = ItemRarityID.Pink;
        }
    }
}