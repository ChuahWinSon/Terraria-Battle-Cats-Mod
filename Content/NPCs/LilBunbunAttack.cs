using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace TheBattleCats.Content.NPCs
{
    public class LilBunbunAttack : ModProjectile
    {


        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3; // If the projectile has 3 frames
        }


        public override void SetDefaults()
        {
            Projectile.width = 80; // Width of hitbox
            Projectile.height = 80; // Height of hitbox
            Projectile.timeLeft = 20; // Lasts for 10 frames (1/6 second)
            Projectile.penetrate = -1; // Doesn't disappear on hit
            Projectile.tileCollide = false; // Doesn't hit tiles
            Projectile.friendly = false; // Doesn't hurt other enemies
            Projectile.hostile = true; // Damages the player
            Projectile.damage = 0; // Damage dealt
        }

public override void AI()
{
    // Every 5 ticks, move to the next frame
    if (++Projectile.frameCounter >= 6) // Change every 5 ticks (60 ticks = 1 second)
    {
        Projectile.frameCounter = 0; // Reset counter
        Projectile.frame++; // Move to the next frame
        
        // If we reached the last frame, loop back to the first frame
        if (Projectile.frame >= Main.projFrames[Projectile.type])
        {
            Projectile.frame = 0;
        }
    }
}

    }
}
