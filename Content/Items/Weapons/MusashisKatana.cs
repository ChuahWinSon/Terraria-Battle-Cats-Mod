using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace TheBattleCats.Content.Items.Weapons
{
    public class MusashisKatana : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 100;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 62;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.rare = ItemRarityID.Blue;
            Item.shoot = ModContent.ProjectileType<MusashiAttack1>();
            Item.shootSpeed = 10f;
            Item.DamageType = DamageClass.MeleeNoSpeed;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ModContent.ProjectileType<MusashiAttack1>()] < 1;
        }

        public override void HoldItem(Player player)
        {
            bool isAttacking =
                player.ownedProjectileCounts[ModContent.ProjectileType<MusashiAttack1>()] > 0 ||
                player.ownedProjectileCounts[ModContent.ProjectileType<MusashiAttack2>()] > 0;

            if (isAttacking)
            {
                KillIdleProjectile(player);
                return;
            }

            bool shouldSpawnIdle =
                player.whoAmI == Main.myPlayer &&
                player.ownedProjectileCounts[ModContent.ProjectileType<MusashiIdle>()] < 1 &&
                !Main.mouseLeft;

            if (shouldSpawnIdle)
            {
                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<MusashiIdle>(),
                    0, 0,
                    player.whoAmI
                );
            }
        }

        private static void KillIdleProjectile(Player player)
        {
            int idleType = ModContent.ProjectileType<MusashiIdle>();
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == idleType)
                {
                    proj.Kill();
                    break; // Only one idle can exist at a time
                }
            }
        }
    }
}
