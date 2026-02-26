using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheBattleCats.Content.Items.Weapons
{


    public class AphroditeRoseProjectile : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = 3;
            Projectile.DamageType = DamageClass.Melee; // or Magic/Summon
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1; // smoother movement
        }

        public override void AI()
        {

            // Add light and trail
            Lighting.AddLight(Projectile.Center, Color.Pink.ToVector3() * 0.6f);
            Vector2 cursorPosition = new Vector2(Projectile.ai[1], Projectile.ai[2]);


            // Dust for visual effect
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PinkTorch);
            }

            if (Projectile.Center.Y > Projectile.ai[0])
            {
                Projectile.tileCollide = true;
            }

            if (Projectile.Center.Y >= cursorPosition.Y)
            {
                // Projectile.Kill(); // or do something else
            }
        }




        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Lovestruck, 120);
            Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.HeartCrystal);
            }

            Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            Vector2.Zero, // no velocity
            ModContent.ProjectileType<AphroditeDone>(), // replace with your projectile
            Projectile.damage,
            Projectile.knockBack,
            Projectile.owner
        );

        }
         public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            Vector2 baseOffset = new Vector2(0f, 0f);

            // Flip X offset when spriteDirection is -1
            Vector2 offset = new Vector2(baseOffset.X * Projectile.spriteDirection, baseOffset.Y);

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition + offset,
                sourceRectangle,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None, // Vertical flip for direction
                0f
            );


            return false;
        }

    }

    public class AphroditeDone : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 30;
        }

        public override void AI()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 1)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
            }
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 30;
            Projectile.width = 140;
            Projectile.height = 100;
            Projectile.tileCollide = false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            Vector2 baseOffset = new Vector2(0f, 0f);

            // Flip X offset when spriteDirection is -1
            Vector2 offset = new Vector2(baseOffset.X * Projectile.spriteDirection, baseOffset.Y);

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition + offset,
                sourceRectangle,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                SpriteEffects.None, // Vertical flip for direction
                0f
            );


            return false;
        }
    }


}
    


