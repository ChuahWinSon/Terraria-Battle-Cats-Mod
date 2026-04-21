using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheBattleCats.Content.Items.Weapons
{
    // ── Shared helpers ────────────────────────────────────────────────────────

    internal static class MusashiHelper
    {
        // Offsets & physics
        public const float IdleHoverY       = -40f;
        public const float IdleBobAmplitude =   5f;
        public const float IdleBobSpeed     = 0.05f;
        public const float SlashKnockback   =  10f;
        public const float ProjectileSpeed  =  40f;
        public const int   HitboxOffsetX    = 100;
        public const int   HitboxOffsetY    = -30;

        /// <summary>
        /// Returns +1 if the cursor is to the right of the player, -1 otherwise.
        /// Only valid on the local client — never call this on a remote projectile's AI.
        /// </summary>
        public static int DirectionToCursor(Player player)
            => (Main.MouseWorld.X >= player.Center.X) ? 1 : -1;

        /// <summary>
        /// Spawns a stationary melee hitbox centred on the player.
        /// Must only be called by the projectile owner (gate with IsOwner check).
        /// </summary>
        public static void SpawnHitbox<T>(Projectile source, Player player, int direction)
            where T : ModProjectile
        {
            Projectile.NewProjectile(
                source.GetSource_FromThis(),
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<T>(),
                source.damage,
                SlashKnockback,
                player.whoAmI,
                direction   // ai[0] → locked direction
            );
        }

        /// <summary>
        /// Spawns a travelling slash-wave.
        /// Velocity is computed from the pre-locked direction so all clients agree.
        /// Must only be called by the projectile owner.
        /// </summary>
        public static void SpawnSlashWave(Projectile source, Player player, int direction)
        {
            // Use the locked direction rather than live cursor so remote clients see
            // the same velocity.
            Vector2 velocity = new Vector2(direction, 0f) * ProjectileSpeed;
            Projectile.NewProjectile(
                source.GetSource_FromThis(),
                player.Center,
                velocity,
                ModContent.ProjectileType<MusashiProjectileDone>(),
                source.damage,
                SlashKnockback,
                player.whoAmI,
                direction   // ai[0] → locked direction (used for sprite flip)
            );
        }

        /// <summary>Standard sprite draw for a vertically-stripped spritesheet.</summary>
        public static void DrawFramed(
            Projectile proj,
            Color lightColor,
            int totalFrames,
            SpriteEffects effects)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;
            int frameHeight = texture.Height / totalFrames;

            Main.spriteBatch.Draw(
                texture,
                proj.Center - Main.screenPosition,
                new Rectangle(0, proj.frame * frameHeight, texture.Width, frameHeight),
                lightColor,
                proj.rotation,
                new Vector2(texture.Width / 2f, frameHeight / 2f),
                proj.scale,
                effects,
                0f
            );
        }
    }

    // ── MusashiIdle ───────────────────────────────────────────────────────────

    public class MusashiIdle : ModProjectile
    {
        private const int TotalFrames   = 1;
        private const int FrameDuration = 3; // ticks per frame
        private const int KeepAliveTime = 2;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = TotalFrames;
        }

        public override void SetDefaults()
        {
            Projectile.width       = 60;
            Projectile.height      = 60;
            Projectile.friendly    = false;
            Projectile.penetrate   = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft    = 20;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            if (player.HeldItem.type == ModContent.ItemType<MusashisKatana>())
                Projectile.timeLeft = KeepAliveTime;
            else
            {
                Projectile.Kill();
                return;
            }

            // Bob up and down above the player
            float bob = (float)System.Math.Sin(Main.GameUpdateCount * MusashiHelper.IdleBobSpeed)
                        * MusashiHelper.IdleBobAmplitude;
            Projectile.Center = player.Center + new Vector2(0f, MusashiHelper.IdleHoverY + bob);

            // Advance animation
            if (++Projectile.frameCounter >= FrameDuration)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= TotalFrames)
                    Projectile.frame = 0;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            MusashiHelper.DrawFramed(Projectile, lightColor, TotalFrames, SpriteEffects.None);
            return false;
        }
    }

    // ── MusashiAttack1 ────────────────────────────────────────────────────────

    public class MusashiAttack1 : ModProjectile
    {
        private const int TotalFrames   = 24;
        private const int HitboxFrame   = 11; // frame on which the hitbox + wave spawn
        private const int TicksPerFrame = 1;

        // ai[0] stores the attack direction, locked on the first AI tick by the
        // owner and then read by every client from that point on.
        private int LockedDirection => (int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = TotalFrames;
        }

        public override void SetDefaults()
        {
            Projectile.width       = 60;
            Projectile.height      = 60;
            Projectile.timeLeft    = TotalFrames;
            Projectile.penetrate   = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType  = DamageClass.Melee;
            Projectile.friendly    = false;
            Projectile.hostile     = false;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // Lock direction from cursor on the very first tick (owner only).
            // ai[] fields are automatically synced to other clients by Terraria,
            // so every client will read the same value from frame 2 onward.
            if (Projectile.frame == 0 && Projectile.frameCounter == 0)
            {
                if (Projectile.owner == Main.myPlayer)
                    Projectile.ai[0] = MusashiHelper.DirectionToCursor(player);

                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
            }

            int direction = LockedDirection;
            player.direction           = direction;
            Projectile.direction       = direction;
            Projectile.spriteDirection = -direction;
            Projectile.Center          = player.Center;

            if (++Projectile.frameCounter < TicksPerFrame)
                return;

            Projectile.frameCounter = 0;
            Projectile.frame++;

            // Only the owner spawns child projectiles; Terraria replicates them.
            if (Projectile.frame == HitboxFrame && Projectile.owner == Main.myPlayer)
            {
                MusashiHelper.SpawnHitbox<MusashiProjectile1>(Projectile, player, direction);
                MusashiHelper.SpawnSlashWave(Projectile, player, direction);
            }

            if (Projectile.frame >= TotalFrames)
            {
                if (Projectile.owner == Main.myPlayer)
                    SpawnAttack2(player, direction);
                Projectile.Kill();
            }
        }

        private void SpawnAttack2(Player player, int direction)
        {
            int index = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<MusashiAttack2>(),
                Projectile.damage,
                Projectile.knockBack,
                player.whoAmI,
                direction   // ai[0] → locked direction for Attack2
            );

            // Terraria won't run Attack2's AI until the next tick, so PreDraw
            // can fire this frame before spriteDirection is set. Prime it now
            // to prevent a one-frame flicker.
            if (index >= 0 && index < Main.maxProjectiles)
                Main.projectile[index].spriteDirection = -direction;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects fx = Projectile.spriteDirection == -1
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;
            MusashiHelper.DrawFramed(Projectile, lightColor, TotalFrames, fx);
            return false;
        }
    }

    // ── MusashiAttack2 ────────────────────────────────────────────────────────

    public class MusashiAttack2 : ModProjectile
    {
        private const int TotalFrames   = 40;
        private const int HitboxFrame   = 7;  // frame on which the hitbox + wave spawn
        private const int TicksPerFrame = 1;

        // Direction was locked by Attack1 and passed in via ai[0] at spawn.
        private int LockedDirection => (int)Projectile.ai[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = TotalFrames;
        }

        public override void SetDefaults()
        {
            Projectile.width       = 60;
            Projectile.height      = 60;
            Projectile.timeLeft    = TotalFrames;
            Projectile.penetrate   = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType  = DamageClass.Melee;
            Projectile.friendly    = false;
            Projectile.hostile     = false;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.frame == 0 && Projectile.frameCounter == 0)
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);

            // ai[0] was set at spawn by Attack1 — safe to read on all clients.
            int direction = LockedDirection;
            player.direction           = direction;
            Projectile.direction       = direction;
            Projectile.spriteDirection = -direction;
            Projectile.Center          = player.Center;

            if (++Projectile.frameCounter < TicksPerFrame)
                return;

            Projectile.frameCounter = 0;
            Projectile.frame++;

            // Only the owner spawns child projectiles; Terraria replicates them.
            if (Projectile.frame == HitboxFrame && Projectile.owner == Main.myPlayer)
            {
                MusashiHelper.SpawnHitbox<MusashiProjectile2>(Projectile, player, direction);
                MusashiHelper.SpawnSlashWave(Projectile, player, direction);
            }

            if (Projectile.frame >= TotalFrames)
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects fx = Projectile.spriteDirection == -1
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;
            MusashiHelper.DrawFramed(Projectile, lightColor, TotalFrames, fx);
            return false;
        }
    }
}
