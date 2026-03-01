using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheBattleCats.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Wings)]
    public class OwlbroWings : ModItem
    {
        public override void SetDefaults()
        {
            Item.width     = 24;
            Item.height    = 24;
            Item.accessory = true;
            Item.value     = Item.buyPrice(0, 5, 0, 0);
            Item.rare      = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.wingTimeMax = 200;  // how long the player can fly (ticks)
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            ascentWhenFalling      = 0.85f; // speed when falling and holding jump
            ascentWhenRising       = 0.15f; // acceleration upward
            maxCanAscendMultiplier = 1f;
            maxAscentMultiplier    = 3f;    // max upward speed multiplier
            constantAscend         = 0.135f;
        }

        public override void HorizontalWingSpeeds(Player player, ref float speed, ref float acceleration)
        {
            speed        = 9f;   // max horizontal speed while flying
            acceleration = 2.5f; // horizontal acceleration while flying
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Feather, 20)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}