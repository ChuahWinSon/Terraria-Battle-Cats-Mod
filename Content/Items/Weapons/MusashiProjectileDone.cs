using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheBattleCats.Content.Items.Weapons
{
    public class MusashiProjectileDone : ModProjectile
    {

        private int direction;
        private Vector2 initialOffset;
        private bool initialized = false;
        private Vector2 spawnPosition;
        public override void SetDefaults()
        {
            // Set the size of the hitbox (adjust as needed)
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.damage = 0; // Damage dealt
            // It doesn't move, it's just a hitbox
            Projectile.friendly = true;
            Projectile.hostile = false; // Make sure it doesn't hurt the player
            Projectile.timeLeft = 4*16; // The hitbox lasts for 10 frames
            Projectile.penetrate = -1; // Doesn't disappear on collision
            Projectile.tileCollide = false; // Don't collide with tiles
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override void AI()
    {


    Dust.NewDust(
    Projectile.position,              // Position
    Projectile.width,                 // Width of spawn area
    Projectile.height,                // Height of spawn area
    DustID.BlueTorch,                     // Dust type (you can change this)
    Projectile.velocity.X * 0.2f,     // X velocity of dust
    Projectile.velocity.Y * 0.2f,     // Y velocity of dust
    100,                              // Alpha (0 = opaque, 255 = invisible)
    default(Color),                   // Color (optional)
    1.5f                              // Scale
    );

    Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 1.0f);

         if (!initialized)
    {
        spawnPosition = Projectile.position;
        initialized = true;
    }

    float distanceTraveled = Vector2.Distance(Projectile.position, spawnPosition);

    // Reduce velocity after traveling 200 pixels
    if (distanceTraveled > 150f)
    {
        Projectile.velocity *= 0.97f; // Reduce speed gradually
    }



        
        Player player = Main.player[Projectile.owner];

    // Only set direction once
    if (direction == 0)
    {
        Vector2 toCursor = Main.MouseWorld - player.Center;
        direction = toCursor.X >= 0 ? 1 : -1;
        
        initialOffset = toCursor.SafeNormalize(Vector2.UnitX) * 100f;
    }


    // Rotate to face the mouse
    Projectile.rotation = initialOffset.ToRotation();

    // Flip sprite if needed
    Projectile.spriteDirection = (initialOffset.X >= 0) ? 1 : -1;
    }


public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
{
    float collisionPoint = 0f;

    Vector2 previousPosition = Projectile.Center - Projectile.velocity; // Where it was last frame
    Vector2 currentPosition = Projectile.Center;                        // Where it is now

    return Collision.CheckAABBvLineCollision(
        targetHitbox.TopLeft(),
        targetHitbox.Size(),
        previousPosition,
        currentPosition,
        100f, // Width of the trail / sweep
        ref collisionPoint
    );
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
                SpriteEffects.FlipHorizontally,
                0f
            );

            return false;
        }


    }
}
