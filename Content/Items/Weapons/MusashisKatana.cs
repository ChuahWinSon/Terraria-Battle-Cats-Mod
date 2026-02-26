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
            Item.useStyle = ItemUseStyleID.Swing; // Change from HoldUp so it responds to left-click
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
            // Prevent using attack if one is already happening
            return player.ownedProjectileCounts[ModContent.ProjectileType<MusashiAttack1>()] < 1;
            
        }



        public override void HoldItem(Player player)
{
    // Kill idle during either attack
    if (player.ownedProjectileCounts[ModContent.ProjectileType<MusashiAttack1>()] > 0 ||
        player.ownedProjectileCounts[ModContent.ProjectileType<MusashiAttack2>()] > 0)
    {
        foreach (Projectile proj in Main.projectile)
        {
            if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<MusashiIdle>())
            {
                proj.Kill();
            }
        }
        return;
    }

    // Spawn idle if not attacking
    if (player.whoAmI == Main.myPlayer &&
        player.ownedProjectileCounts[ModContent.ProjectileType<MusashiIdle>()] < 1 && !Main.mouseLeft)
    {
        Projectile.NewProjectile(
            player.GetSource_ItemUse(Item),
            player.Center,
            Vector2.Zero,
            ModContent.ProjectileType<MusashiIdle>(),
            0,
            0,
            player.whoAmI
        );
    }
}
    }
}
