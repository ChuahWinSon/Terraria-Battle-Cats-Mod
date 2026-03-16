using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;

namespace TheBattleCats.Content.NPCs.CycloneBoss
{

    public class TelegraphLines : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 400; // tall vertical line
            Projectile.penetrate = -1;
            Projectile.timeLeft = 40; 
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

    public class CycloneProjectile1 : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width       = 16;
            Projectile.height      = 16;
            Projectile.hostile     = true;
            Projectile.friendly    = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft    = 300;
            Projectile.aiStyle     = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }

    public class CycloneProjectile2 : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width       = 16;
            Projectile.height      = 16;
            Projectile.hostile     = true;
            Projectile.friendly    = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft    = 300;
            Projectile.aiStyle     = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }

    public class CycloneProjectile3 : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width       = 16;
            Projectile.height      = 16;
            Projectile.hostile     = true;
            Projectile.friendly    = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft    = 300;
            Projectile.aiStyle     = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }

    public class CycloneProjectile4 : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width       = 16;
            Projectile.height      = 16;
            Projectile.hostile     = true;
            Projectile.friendly    = false;
            Projectile.tileCollide = true;
            Projectile.timeLeft    = 300;
            Projectile.aiStyle     = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }

    public class CycloneTelegraph : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width       = 20;
            Projectile.height      = 600; // tall to show the fall path
            Projectile.friendly    = false;
            Projectile.hostile     = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft    = 60; // lasts 1 second then disappears
            Projectile.aiStyle     = -1;
            Projectile.alpha       = 180; // semi transparent
        }

        public override void AI()
        {
            // Fade out as timeLeft decreases
            Projectile.alpha = (int)MathHelper.Lerp(180, 255, 1f - Projectile.timeLeft / 60f);
        }
    }
}