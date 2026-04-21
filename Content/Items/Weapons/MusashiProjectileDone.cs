using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheBattleCats.Content.Items.Weapons
{
    public class MusashiProjectileDone : ModProjectile
    {
        // ── Tuning constants ──────────────────────────────────────────────────

        private const int   Lifetime       = 4 * 16;  // frames
        private const float SlowdownRadius = 150f;     // pixels before drag kicks in
        private const float DragFactor     = 0.97f;
        private const float DustAlpha      = 100f;
        private const float DustScale      = 1.5f;
        private const float SweepWidth     = 100f;     // width used by line collision
        private const float LightR         = 0.2f;
        private const float LightG         = 0.4f;
        private const float LightB         = 1.0f;

        // ── State ─────────────────────────────────────────────────────────────

        private Vector2 spawnPosition;
        // Derived from spawn velocity on the first frame — valid on all clients
        // because velocity is set by the owner and synced by Terraria.
        private Vector2 initialDirection;
        private bool    initialized;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public override void SetDefaults()
        {
            Projectile.width       = 16;
            Projectile.height      = 16;
            Projectile.damage      = 0;
            Projectile.friendly    = true;
            Projectile.hostile     = false;
            Projectile.timeLeft    = Lifetime;
            Projectile.penetrate   = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType  = DamageClass.Melee;
        }

        // ── AI ────────────────────────────────────────────────────────────────

        public override void AI()
        {
            if (!initialized)
            {
                spawnPosition = Projectile.position;

                // Derive facing direction from the spawn velocity that the owner
                // already set correctly. This is safe on all clients — no MouseWorld.
                // Fall back to ai[0] (the locked attack direction) if velocity is
                // zero for any reason.
                if (Projectile.velocity != Vector2.Zero)
                    initialDirection = Vector2.Normalize(Projectile.velocity);
                else
                    initialDirection = new Vector2((int)Projectile.ai[0], 0f);

                initialized = true;
            }

            EmitDust();
            EmitLight();
            ApplyDragIfFar();

            // Rotation and flip locked to initial direction so the sprite doesn't
            // warp as drag slows the projectile down.
            Projectile.rotation        = initialDirection.ToRotation();
            Projectile.spriteDirection = (initialDirection.X >= 0) ? 1 : -1;
        }

        private void EmitDust()
        {
            Dust.NewDust(
                Projectile.position,
                Projectile.width,
                Projectile.height,
                DustID.BlueTorch,
                Projectile.velocity.X * 0.2f,
                Projectile.velocity.Y * 0.2f,
                (int)DustAlpha,
                default,
                DustScale
            );
        }

        private void EmitLight()
            => Lighting.AddLight(Projectile.Center, LightR, LightG, LightB);

        private void ApplyDragIfFar()
        {
            if (Vector2.Distance(Projectile.position, spawnPosition) > SlowdownRadius)
                Projectile.velocity *= DragFactor;
        }

        // ── Collision ─────────────────────────────────────────────────────────

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Center - Projectile.velocity,
                Projectile.Center,
                SweepWidth,
                ref collisionPoint
            );
        }

        // ── Drawing ───────────────────────────────────────────────────────────

        public override bool PreDraw(ref Color lightColor)
        {
            MusashiHelper.DrawFramed(
                Projectile,
                lightColor,
                Main.projFrames[Projectile.type],
                SpriteEffects.FlipHorizontally
            );
            return false;
        }
    }
}

