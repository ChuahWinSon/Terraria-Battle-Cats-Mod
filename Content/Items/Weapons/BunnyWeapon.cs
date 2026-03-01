using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace TheBattleCats.Content.Items.Weapons
{
    public class BunnyWeapon : ModItem
    {
        public override void SetDefaults()
        {
            Item.width        = 40;
            Item.height       = 20;
            Item.damage       = 20;
            Item.DamageType   = DamageClass.Ranged;
            Item.knockBack    = 2f;
            Item.useTime      = 60;
            Item.useAnimation = 60;
            Item.useStyle     = ItemUseStyleID.Shoot;
            Item.noMelee      = true;
            Item.shoot        = ProjectileID.Bullet;
            Item.shootSpeed   = 10f;
            Item.useAmmo      = AmmoID.Bullet;
            Item.value        = Item.buyPrice(0, 5, 0, 0);
            Item.rare         = ItemRarityID.Green;
            Item.autoReuse    = false;
            Item.scale = 0.4f;
        }

        // public override bool CanUseItem(Player player)
        // {
        //     // Only trigger if player is on the ground
        //     // return player.velocity.Y == 0f;
        // }

        public override bool? UseItem(Player player)
        {
            player.GetModPlayer<BunnyWeaponLogic>().StartFlip();
            return null;
        }

        public override Vector2? HoldoutOffset() {
			return new Vector2(30f,20f );  //(x,y )
		}

        public override bool Shoot(Player player, Terraria.DataStructures.EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Block the default shoot — we fire manually in BunnyWeaponPlayer
            return false;
        }



        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IronBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}