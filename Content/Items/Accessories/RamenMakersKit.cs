using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace TheBattleCats.Content.Items.Accessories
{
    public class RamenMakersKit : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.value = Item.buyPrice(gold: 1);
            Item.rare = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RamenKitPlayer>().ramenKitEquipped = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HealthRegen", "Heals 20 HP every 20 seconds when damaged"));
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBar, 5)
                .AddIngredient(ItemID.LifeCrystal)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class RamenKitPlayer : ModPlayer
    {
        public bool ramenKitEquipped = false;
        private int healTimer = 0;

        public override void ResetEffects()
        {
            ramenKitEquipped = false;
        }

        public override void PostUpdate()
        {
            if (ramenKitEquipped)
            {
                healTimer++;

                if (healTimer >= 60 * 20) // 20 seconds
                {
                    if (Player.statLife < Player.statLifeMax2)
                    {
                        Player.statLife += 20;

                        if (Player.statLife > Player.statLifeMax2)
                            Player.statLife = Player.statLifeMax2;

                        Player.HealEffect(20, true);
                        healTimer = 0; // reset timer only if healed
                    }
                    // else: player full HP, don't reset timer, so it tries again next tick
                }

                // Optional: cap timer so it doesn't get too large
                if (healTimer > 60 * 60) // 60 seconds max wait
                {
                    healTimer = 60 * 20; // reset to 20 sec to try again soon
                }
            }
            else
            {
                // Reset on unequip
                healTimer = 0;
            }
        }
    }
}
