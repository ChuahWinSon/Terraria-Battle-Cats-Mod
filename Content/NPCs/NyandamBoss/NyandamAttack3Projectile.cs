using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace TheBattleCats.Content.NPCs.NyandamBoss
{
    public class NyandamAttack3Projectile : ModProjectile
    {
        private float angle;
        private float radius;
        private Vector2 center;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 1000;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
        }


public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 4;
    }

        public override void AI()
{

    if (++Projectile.frameCounter >= 4)
        {
            Projectile.frameCounter = 0;
            Projectile.frame = (Projectile.frame + 1) % 4;
        }

    // --- Initialization (run once) ---
    if (Projectile.localAI[1] == 0) // use localAI[1] instead of ai[0]
    {
        center = Projectile.Center;                     // store starting point
        angle = Projectile.velocity.ToRotation();       // initial angle
        Projectile.localAI[1] = 1;                      // mark as initialized
    }

    // --- Count ticks since spawn ---
    Projectile.localAI[0]++;
    float t = Projectile.localAI[0];

    // --- Spiral logic ---
    float spiralSpeed = 0.02f;
    radius = (float)Math.Pow(t, 0.9f) * 3f; // you can tweak this for spiral tightness
    angle += spiralSpeed;

    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
    Projectile.Center = center + offset;

    Projectile.rotation += 0.2f;

    if (Projectile.ai[0] == 1f) // fading out
    {
        Projectile.Opacity -= 0.05f; // adjust speed to taste
        if (Projectile.Opacity <= 0f)
        {
            Projectile.Kill();
        }
        return; // skip normal spiral logic
    }
    
    
}

public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Rectangle frame = texture.Frame(1, 4, 0, Projectile.frame);
        Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

        Main.spriteBatch.Draw(
            texture,
            Projectile.Center - Main.screenPosition,
            frame,
            Color.White * Projectile.Opacity,
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }
}

    public class NyandamEnhancedAttack3Projectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 8; // swapped from vertical laser
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 1f, 0.4f, 0f);

            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 2;
            }
        }

        public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Rectangle frame = texture.Frame(1, 2, 0, Projectile.frame);
        Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

        Main.spriteBatch.Draw(
        texture,
        Projectile.Center - Main.screenPosition,
        frame,
        Color.White * ((255 - Projectile.alpha) / 255f),
        Projectile.rotation + MathHelper.PiOver2,
        origin,
        Projectile.scale,
        SpriteEffects.None,
        0f
    );

        return false;
    }
    }

}
