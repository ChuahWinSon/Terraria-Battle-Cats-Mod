using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TheBattleCats.Content.Projectiles;
using Microsoft.Xna.Framework;

namespace TheBattleCats.Content.Items.Weapons
{
    public class ChronosWatch : ModItem
    {
        public static int attackPhase = 0;

        public override void SetDefaults()
        {
            Item.damage = 100;
            Item.DamageType = DamageClass.Magic;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 40;
            Item.useAnimation = 1000;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item20;
            Item.autoReuse = true;
            Item.mana = 10;
            Item.shootSpeed = 0f;
            Item.channel = true;
            Item.UseSound = null;
        }

        public override bool? UseItem(Player player)
        {
            // Determine where you want the projectile to spawn
            Vector2 spawnPos = player.Center + new Vector2(100 * player.direction, -10);

            // Choose projectile based on attack phase
            int projType;
            switch (attackPhase)
            {
                case 0:
                    projType = ModContent.ProjectileType<ChronosAttack1>();
                    break;
                case 1:
                    projType = ModContent.ProjectileType<ChronosAttack2>(); // Make sure this exists
                    break;
                case 2:
                    projType = ModContent.ProjectileType<ChronosAttack3>(); // Make sure this exists
                    break;
                default:
                    projType = ModContent.ProjectileType<ChronosAttack1>();
                    break;
            }

            // Spawn the projectile
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), spawnPos, Vector2.Zero, projType, Item.damage, Item.knockBack, player.whoAmI);

            // Cycle the attack phase
            attackPhase = (attackPhase + 1) % 3;

            return true;
        }

        // Override CanUseItem to set Item.useTime and Item.useAnimation dynamically based on attack phase
        public override bool CanUseItem(Player player)
        {
            if (attackPhase == 2) // Attack C (32 frames)
            {
                Item.useTime = 95;
                Item.useAnimation = 95;
                Item.damage = 1000;
            }
            else // Attacks A and B (20 frames)
            {
                Item.useTime = 40;
                Item.useAnimation = 40;
                Item.damage = 200;
            }

            return base.CanUseItem(player);
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FallenStar, 5)
                .AddIngredient(ItemID.Wood, 10)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
