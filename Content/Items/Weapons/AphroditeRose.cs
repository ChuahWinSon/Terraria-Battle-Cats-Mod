using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace TheBattleCats.Content.Items.Weapons
{
    public class AphroditeRose : ModItem
    {


        public override void SetDefaults()
        {
            Item.damage = 3000;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 1;
            Item.useAnimation = 1;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2f;
            Item.rare = ItemRarityID.Blue;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<AphroditeRoseAttack>();
            Item.shootSpeed = 0f;
        }

        public override bool CanUseItem(Player player)
        {
            // Prevents multiple projectiles
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

              public override void HoldItem(Player player)
{
    // Kill idle during either attack
    if  (player.ownedProjectileCounts[ModContent.ProjectileType<AphroditeRoseAttack>()] > 0||
    player.ownedProjectileCounts[ModContent.ProjectileType<AphroditeEnd>()] > 0)
    {
        foreach (Projectile proj in Main.projectile)
        {
            if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<AphroditeIdle>())
            {
                proj.Kill();
            }
        }
        return;
    }

    // Spawn idle if not attacking
    if (player.whoAmI == Main.myPlayer &&
        player.ownedProjectileCounts[ModContent.ProjectileType<AphroditeIdle>()] < 1)
    {
        Projectile.NewProjectile(
            player.GetSource_ItemUse(Item),
            player.Center,
            Vector2.Zero,
            ModContent.ProjectileType<AphroditeIdle>(),
            0,
            0,
            player.whoAmI
        );
    }
}

    }
}
