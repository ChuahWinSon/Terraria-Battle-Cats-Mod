using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheBattleCats.Content.Items.Accessories
{
    public class TheNerdsComputer : ModItem
    {
        public override void SetDefaults()
        {
            // Set the basic properties of the item
            Item.width = 30;  // width of the sprite
            Item.height = 30; // height of the sprite
            Item.accessory = true; // Set as an accessory
            Item.value = Item.buyPrice(gold: 1);  // price in gold
            Item.rare = ItemRarityID.Green;  // Green rarity
        }

        // Override the Equip() method to add effects when the item is equipped
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // Increases max health by 20
            player.statLifeMax2 += 20;
        }

        // Optionally, add a tooltip to explain the effect
        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HealthAmulet", "Increases max health by 20"));
        }

        // Create the recipe for this item
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 5)
                .AddIngredient(ItemID.LifeCrystal)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
