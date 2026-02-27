using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;

namespace TheBattleCats.Content.NPCs
{
    public class TackeyProjectile : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width    = 44;
            Projectile.height   = 44;
            Projectile.friendly = false;
            Projectile.hostile  = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.aiStyle  = -1;
            Projectile.scale = 0.7f; 
        }


        public override void AI()
        {
            // Just apply gravity each tick — the arc shape comes from
            // firing at an upward angle in FireProjectile
            Projectile.velocity.Y += 0.25f; // gravity strength, tune this

            Projectile.rotation = Projectile.velocity.ToRotation();
        }
    }
}