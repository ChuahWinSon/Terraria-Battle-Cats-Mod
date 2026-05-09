using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace TheBattleCats.Content.NPCs.ClionelBoss
{
    public class ClionelBallProjectile : ModProjectile
    {
        // -------------------------------------------------------
        // ai[0] = launch direction X (set by boss when launching)
        // ai[1] = launch direction Y
        // ai[2] = state: 0 = fading in, 1 = waiting, 2 = launched
        // -------------------------------------------------------
        private ref float DirX    => ref Projectile.ai[0];
        private ref float DirY    => ref Projectile.ai[1];
        private ref float State   => ref Projectile.ai[2];

        public const float LaunchSpeed   = 12f;
        public const int   FadeInTicks   = 16; // ticks to fully fade in
        private int fadeTimer = 0;

        public override void SetDefaults()
        {
            Projectile.width    = 26;
            Projectile.height   = 26;
            Projectile.hostile  = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.damage   = Clionel.AllProjectileDamage;
            Projectile.Opacity  = 0f;
        }

        public override void AI()
        {
            switch ((int)State)
            {
                // ------------------------------------------------
                // Fading in — stay in place, increase opacity
                // ------------------------------------------------
                case 0:
                    fadeTimer++;
                    Projectile.Opacity = MathHelper.Clamp((float)fadeTimer / FadeInTicks, 0f, 1f);
                    Projectile.velocity = Vector2.Zero;

                    if (fadeTimer >= FadeInTicks)
                    {
                        Projectile.Opacity = 1f;
                        State = 1f; // waiting for launch signal
                    }
                    break;

                // ------------------------------------------------
                // Waiting — fully visible, stationary
                // ------------------------------------------------
                case 1:
                    Projectile.Opacity  = 1f;
                    Projectile.velocity = Vector2.Zero;
                    break;

                // ------------------------------------------------
                // Launched — fly in set direction
                // ------------------------------------------------
                case 2:
                    Projectile.Opacity  = 1f;
                    Projectile.velocity = new Vector2(DirX, DirY) * LaunchSpeed;
                    break;
            }
        }

        /// <summary>Called by the boss to fire all waiting balls in the curtain direction.</summary>
        public void Launch(Vector2 direction)
        {
            DirX  = direction.X;
            DirY  = direction.Y;
            State = 2f;
            Projectile.netUpdate = true;
        }

    }
}
