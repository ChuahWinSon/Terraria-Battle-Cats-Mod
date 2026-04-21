using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheBattleCats.Content.Items.Weapons
{
    // ── Shared hitbox base ────────────────────────────────────────────────────
    //
    // MusashiProjectile1 and MusashiProjectile2 are identical apart from their
    // hitbox width. Both inherit from this base class to avoid duplication.

    public abstract class MusashiHitboxBase : ModProjectile
    {
        // Subclasses override this to set the hitbox width.
        protected abstract int HitboxWidth { get; }

        private const int HitboxHeight   = 360;
        private const int HitboxLifetime = 10;

        public override void SetDefaults()
        {
            Projectile.width       = HitboxWidth;
            Projectile.height      = HitboxHeight;
            Projectile.damage      = 0;
            Projectile.friendly    = true;
            Projectile.hostile     = false;
            Projectile.timeLeft    = HitboxLifetime;
            Projectile.penetrate   = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType  = DamageClass.Melee;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // ai[0] holds the locked attack direction (+1 or -1).
            // Fall back to cursor direction when the caller didn't pass one.
            int direction = Projectile.ai[0] != 0
                ? (int)Projectile.ai[0]
                : MusashiHelper.DirectionToCursor(player);

            Projectile.Center = player.Center + new Vector2(
                MusashiHelper.HitboxOffsetX * direction,
                MusashiHelper.HitboxOffsetY
            );
        }
    }

    // ── Concrete hitboxes ─────────────────────────────────────────────────────

    public class MusashiProjectile1 : MusashiHitboxBase
    {
        protected override int HitboxWidth => 140;
    }

    public class MusashiProjectile2 : MusashiHitboxBase
    {
        protected override int HitboxWidth => 180;
    }
}
