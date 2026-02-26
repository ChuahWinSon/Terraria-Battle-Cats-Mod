using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace TheBattleCats.Content.Items.Weapons
{
    public class MusashiProjectile1 : ModProjectile
    {

        private int direction;
        public override void SetDefaults()
        {
            // Set the size of the hitbox (adjust as needed)
            Projectile.width = 140;
            Projectile.height = 360;
            Projectile.damage = 0; // Damage dealt
            // It doesn't move, it's just a hitbox
            Projectile.friendly = true;
            Projectile.hostile = false; // Make sure it doesn't hurt the player
            Projectile.timeLeft = 10; // The hitbox lasts for 10 frames
            Projectile.penetrate = -1; // Doesn't disappear on collision
            Projectile.tileCollide = false; // Don't collide with tiles
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        // If direction hasn't been set yet (i.e., this is the first frame), set the direction
        if (direction == 0)
        {
            Vector2 toCursor = Main.MouseWorld - player.Center;
            direction = toCursor.X >= 0 ? 1 : -1;
        }

        // Position the hitbox relative to the player's position using the stored direction
        Projectile.Center = player.Center + new Vector2(100 * direction, -30); 

        
    }



    }
}
