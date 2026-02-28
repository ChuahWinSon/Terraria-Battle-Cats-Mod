using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace TheBattleCats.Content.Items.Weapons
{
    public class BunnyWeaponRecoil : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6; // however many frames your animation has
        }

        public override void SetDefaults()
        {
            Projectile.width       = 20;
            Projectile.height      = 20;
            Projectile.friendly    = false;
            Projectile.hostile     = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft    = 20; // short lived
            Projectile.aiStyle     = -1;
            Projectile.alpha       = 0;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Follow the gun barrel position
            Vector2 spawnPos = owner.Center + new Vector2(owner.direction * 1f, -1f);
            Projectile.position =spawnPos - new Vector2(Projectile.width / 2f, Projectile.height / 2f);

            // Rotate to match shoot direction
            Projectile.rotation = (Main.MouseWorld - owner.Center).ToRotation() + 90f;

            // Animate through frames
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 7)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.Kill();
            }
        }
    }
}