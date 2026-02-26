using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;

namespace TheBattleCats.Content.Items.Weapons
{
    public class AmaterasuProjectile : ModProjectile
    {
        public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.penetrate = -1; // Infinite penetration
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 6;
    }

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        Vector2 spawnPosition = new Vector2(Projectile.ai[0], Projectile.ai[1]);
        
        // Aim where player is aiming (mouse)
            // Projectile.Center = player.Center + spawnPosition;
        Vector2 direction = Main.MouseWorld - spawnPosition;
        direction.Normalize();
            
        Projectile.velocity = direction;

        // You can adjust length by raycasting tiles or limiting max length
        // For example, max laser length 1000 pixels:
        float maxLaserLength = 1000f;
        Vector2 laserEnd = Projectile.Center + direction * maxLaserLength;

        // TODO: Add collision detection and shorten laser length if it hits tiles

        // Save laserEnd position for drawing and collision checks
        Projectile.localAI[0] = maxLaserLength; // length
    }

    // Colliding check to hit enemies along laser beam line
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float laserLength = Projectile.localAI[0];
        Vector2 start = Projectile.Center;
        Vector2 end = start + Projectile.velocity * laserLength;

        // Check collision between line and target rectangle, allow some width (say 20 pixels)
        float collisionPoint = 0f;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 20, ref collisionPoint);
    }

    // Optional: Custom drawing to stretch laser texture along the line
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>("TheBattleCats/Content/Items/Weapons/AmaterasuProjectile").Value;

        float length = Projectile.localAI[0];
        Vector2 start = Projectile.Center;
        Vector2 direction = Projectile.velocity;
        direction.Normalize();

        float rotation = direction.ToRotation() - MathHelper.PiOver2;

        int tileCount = (int)(length / texture.Height);

        for (int i = 0; i < tileCount; i++)
        {
            Vector2 drawPos = start + direction * texture.Height * i - Main.screenPosition;
            Main.spriteBatch.Draw(texture, drawPos, null, Color.White, rotation, new Vector2(texture.Width / 2, 0), 1f, SpriteEffects.None, 0f);
        }

        return false; // Don't draw default sprite
    }
    }
}