using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;

namespace TheBattleCats.Content.NPCs
{
    public class TackeySmoke : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.width       = 180;
            Projectile.height      = 180;
            Projectile.friendly    = false;
            Projectile.hostile     = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft    = 30; // 3 frames * ~10 ticks each
            Projectile.aiStyle     = -1;
            Projectile.alpha       = 0;
            Projectile.scale = 0.7f; 
        }

        public override void AI()
        {
            // Slow down
            Projectile.velocity *= 0.9f;

            // Fade out over lifetime
            Projectile.alpha = (int)(255 * (1f - (float)Projectile.timeLeft / 30f));

            // Animate through 3 frames
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 10)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 3)
                    Projectile.frame = 2; // hold last frame until it dies
            }

            if (Projectile.timeLeft == 30)
            Projectile.rotation = Projectile.velocity.ToRotation();
        }



    }
}