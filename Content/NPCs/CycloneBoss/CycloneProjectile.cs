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


   public class CycloneProjectile : ModProjectile
{
    // Store which variant (0-3) in ai[0]
    private int Variant => (int)Projectile.ai[0];

    // Texture paths for each variant
    private static readonly string[] TexturePaths = new string[]
    {
        "TheBattleCats/Content/NPCs/CycloneBoss/CycloneProjectile",
        "TheBattleCats/Content/NPCs/CycloneBoss/CycloneProjectile2",
        "TheBattleCats/Content/NPCs/CycloneBoss/CycloneProjectile3",
        "TheBattleCats/Content/NPCs/CycloneBoss/CycloneProjectile4",
    };

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 1;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
    }

    public override void SetDefaults()
    {
        Projectile.width       = 16;
        Projectile.height      = 16;
        Projectile.hostile     = true;
        Projectile.friendly    = false;
        Projectile.tileCollide = false;
        Projectile.timeLeft    = 300;
        Projectile.aiStyle     = -1;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        if (Projectile.ai[1] == 1f)
        {
            Projectile.ai[2]++;
            if (Projectile.ai[2] % 10 == 0)
            {
                Projectile.velocity.X += Projectile.velocity.X > 0 ? 1f : -1f;
            }
        }

        if (Main.rand.NextBool(5))
        {
            Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
            d.velocity = Main.rand.NextVector2Circular(2f, 2f);
            d.scale = Main.rand.NextFloat(0.8f, 1.4f);
            d.noGravity = false;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // Load the correct variant texture
        Texture2D texture = ModContent.Request<Texture2D>(TexturePaths[Variant]).Value;

        Color drawColor = Color.White;
        drawColor.A = 0;
        drawColor *= 0.5f;

        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + new Vector2(Projectile.width / 2f, Projectile.height / 2f);
            float trailOpacity = (1f - (float)i / Projectile.oldPos.Length) * 0.5f;
            Main.EntitySpriteDraw(texture, drawPos, null, drawColor * trailOpacity, Projectile.rotation, texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
        }

        return true;
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

public class ClusteredRock : ModProjectile
{
    private bool haspeaked = false;

    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 600;
        Projectile.penetrate = -1;
    }

    public override void AI()
    {

        if (Main.rand.NextBool(5))
        {
            Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Stone);
            d.velocity = Main.rand.NextVector2Circular(2f, 2f);
            d.scale = Main.rand.NextFloat(0.8f, 1.4f);
            d.noGravity = false;
        }

        Projectile.velocity.Y += 0.2f;

        if (Projectile.velocity.Y >= 0f)
            haspeaked = true;

        Projectile.rotation += Projectile.velocity.X * 0.05f;

        if (haspeaked)
        {
            // Check the bottom edge of the projectile, not the center
            // This fires as soon as the feet touch a tile rather than when
            // the center has already buried itself inside one
            Vector2 bottomCenter = new Vector2(Projectile.Center.X, Projectile.Bottom.Y);
            Point tilePos = bottomCenter.ToTileCoordinates();
            Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);

            if (tile.HasTile && Main.tileSolid[tile.TileType])
            {

                OnHitTile();
            }
        }
    }

    private void OnHitTile()
    {
        if (Projectile.active && Main.netMode != NetmodeID.MultiplayerClient)
        {
            // Start scan from the bottom edge tile, not the center
            // This avoids re-scanning tiles the projectile already passed through
            Vector2 bottomCenter = new Vector2(Projectile.Center.X, Projectile.Bottom.Y);
            Point tilePos = bottomCenter.ToTileCoordinates();

            // Scan upward first in case the bottom edge overshot into a tile
            // This recovers the true top surface even if we're a pixel or two deep
            while (tilePos.Y > 0)
            {
                Tile above = Framing.GetTileSafely(tilePos.X, tilePos.Y - 1);
                if (!above.HasTile || !Main.tileSolid[above.TileType])
                    break;
                tilePos.Y--;
            }

            int Yoffset = 4; // make it sink in the block alittle

            // tilePos.Y is now the topmost solid tile in the column at this X.
            // Spawn LingeringRock so its bottom sits flush on the tile surface.
            // LingeringRock height = 26, tile top = tilePos.Y * 16
            // So center.Y = (tilePos.Y * 16) - (26 / 2) = tilePos.Y * 16 - 13



            int lingeringRockHeight = 26;
            Vector2 spawnPos = new Vector2(
                tilePos.X * 16 + 8,
                tilePos.Y * 16 - lingeringRockHeight / 2 + Yoffset
            );

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                spawnPos,
                Vector2.Zero,
                ModContent.ProjectileType<LingeringRock>(),
                Projectile.damage,
                0f,
                Main.myPlayer
            );

            // Impact dust burst
            for (int i = 0; i < 18; i++)
            {
                Dust d = Dust.NewDustDirect(
                    spawnPos - new Vector2(16, 16),
                    32, 32,
                    DustID.Stone
                );
                d.velocity = Main.rand.NextVector2Circular(5f, 5f);
                d.velocity.Y -= Main.rand.NextFloat(1f, 4f); // bias upward
                d.scale = Main.rand.NextFloat(1f, 1.8f);
                d.noGravity = false;
            }
        }

        Projectile.Kill();
    }
}

    public class LingeringRock : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 26;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2700;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero; 

            // Optional: pulse or glow effect over time
            Lighting.AddLight(Projectile.Center, 0.8f, 0.3f, 0.1f);
        }

    }

    public class SplittingRock : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300; // safety net
        }


        public override void AI()
        {
            Projectile.velocity *= 0.96f;
            Projectile.ai[0]++;

            // --- Glow red effect ---
            float progress = Projectile.ai[0] / 120f; // 0.0 → 1.0 over 2 seconds

            if (Projectile.ai[0] >= 90) // last 30 ticks: flash rapidly
            {
                // alternates every 4 ticks between red and dark-red
                bool flash = (int)(Projectile.ai[0] / 4) % 2 == 0;
                Projectile.alpha = flash ? 0 : 150;
            }

            // Emit red dust as it charges up
            if (Main.rand.NextBool(Math.Max(1, (int)(10 * (1f - progress)))))
            {
                Dust dust = Dust.NewDustDirect(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, // orange-red fire dust
                    0f, 0f,
                    100,
                    Color.Red,
                    1.5f * progress // grows bigger as it charges
                );
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            if (Projectile.ai[0] >= 120)
            {
                Explode();
                Projectile.Kill();
            }
        }

        // Tint the sprite red, getting more intense over time
        public override Color? GetAlpha(Color lightColor)
        {
            float progress = Math.Min(Projectile.ai[0] / 120f, 1f);

            // Lerp from normal color → full red
            Color tint = Color.Lerp(lightColor, Color.Red, progress);
            tint.A = (byte)(255 - Projectile.alpha);
            return tint;
        }

        private void Explode()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int numProjectiles = 5;
            float speed = 8f;

            for (int i = 0; i < numProjectiles; i++)
            {
                // evenly spread 5 directions in a full circle
                float angle = MathHelper.TwoPi / numProjectiles * i;
                Vector2 velocity = new Vector2(speed, 0f).RotatedBy(angle);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    velocity,
                    ModContent.ProjectileType<CycloneProjectile>(),
                    Projectile.damage,
                    Projectile.knockBack,
                    Main.myPlayer
                );
            }
        }
    }

    public class MiniCyclone : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5; 
            Main.projFrames[Type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {

            Projectile.rotation = Projectile.velocity.ToRotation();

            // animate — advances frame every 5 ticks (12fps at 60fps)
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 5;
            }
        }

       public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            Color drawColor = Color.White;
            drawColor.A = 0; // keeps additive blending working correctly
            drawColor *= 0.5f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition 
                    + new Vector2(Projectile.width / 2f, Projectile.height / 2f);

                float trailOpacity = (1f - (float)i / Projectile.oldPos.Length) * 0.5f;

                Main.EntitySpriteDraw(
                    texture, drawPos, sourceRect, drawColor * trailOpacity,
                    Projectile.rotation, origin, Projectile.scale,
                    SpriteEffects.None, 0);
            }

            return true;
        }

    }
}