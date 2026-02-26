using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;

namespace TheBattleCats.Content.NPCs.NyandamBoss
{
     // ---------------- TELEGRAPH LINE ----------------
    public class TelegraphLine : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 400; // tall vertical line
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40; // lasts 1 second
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            // Optional: make the line fade out
            Projectile.alpha += 8;
        }

            public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;
            
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle lineRect = new Rectangle(
                (int)drawPos.X - 2,  // centered, 4px wide
                (int)drawPos.Y - 300, // top of line
                4,    // width
                2000   // height (stretch the pixel into a tall line)
            );

            Main.spriteBatch.Draw(
                pixel,
                lineRect,
                Color.Red * ((255 - Projectile.alpha) / 255f)
            );

            return false;
        }
    }

    // ---------------- LASER PROJECTILE ----------------
    public class LaserProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 2;
        }


        public override void AI()
        {

            // optional: add light
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
            Projectile.rotation,
            origin,
            Projectile.scale,
            SpriteEffects.None,
            0f
        );

        return false;
    }
    }

        // ---------------- HORIZONTAL TELEGRAPH LINE ----------------
    public class EnhancedTelegraphLine : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 400;
            Projectile.height = 4;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.alpha += 8;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle lineRect = new Rectangle(
                (int)drawPos.X - 800, // far left of line
                (int)drawPos.Y - 2,   // centered vertically, 4px tall
                2000,                  // width
                4                      // height
            );

            Main.spriteBatch.Draw(
                pixel,
                lineRect,
                Color.Red * ((255 - Projectile.alpha) / 255f)
            );

            return false;
        }
    }

    // ---------------- HORIZONTAL LASER PROJECTILE ----------------
    public class EnhancedLaserProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 8; // swapped from vertical laser
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
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