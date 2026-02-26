using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;

namespace TheBattleCats.Content.Projectiles
{
    public class ChronosAttack2 : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.hide = true;

            Main.projFrames[Projectile.type] = 20;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1.0f, 0.9f, 0.5f);
            
            if (Projectile.frame == 5 && Projectile.frameCounter == 0)
            {
                SoundEngine.PlaySound(new SoundStyle("TheBattleCats/Assets/Music/chronosAttack2"), Projectile.Center);
            }

            Player player = Main.player[Projectile.owner];

            if (player.channel && player.active && !player.dead)
            {
                Vector2 directionToMouse = Main.MouseWorld - player.Center;
                directionToMouse.Normalize();

                float distanceFromPlayer = 100f;
                Projectile.Center = player.Center + directionToMouse * distanceFromPlayer;

                Projectile.rotation = directionToMouse.ToRotation(); // Rotate sprite
                Projectile.direction = Projectile.spriteDirection = (Main.MouseWorld.X < player.Center.X) ? -1 : 1;
                player.direction = Projectile.direction;
                player.heldProj = Projectile.whoAmI;
            }
            else
            {
                Projectile.Kill();
                return;
            }

            if (++Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 20)
                    Projectile.frame = 0;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                sourceRectangle,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None, // Vertical flip for direction
                0f
            );

            return false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return Projectile.frame >= 9 && Projectile.frame <= 10;
        }
    }
}
