using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;

namespace TheBattleCats.Content.NPCs.NyandamBoss
{
    public class NyandamAttack1Projectile : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = 260; // Default fallback
        }


        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 6;
        }

        public override void OnSpawn(IEntitySource Source)
        {
            // Despawn all projectiles at the same time based on boss's AITimer
            int globalDespawnTime = 260;

            Projectile.timeLeft = globalDespawnTime - (int)Projectile.ai[0];

            // Safety: if spawned too late, make sure it doesn't die instantly
            if (Projectile.timeLeft < 1)
                Projectile.timeLeft = 1;
        }

        public override void AI()
        {


            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 6;
            }

            int bossID = (int)Projectile.ai[2];
            Projectile.rotation = (float)Projectile.ai[1] + MathHelper.PiOver2;

            if (!Main.npc.IndexInRange(bossID) || !Main.npc[bossID].active)
                return;

            NPC boss = Main.npc[bossID];
            Vector2 bossCenter = boss.Center;
            float radius = boss.width * 0.5f + 20f;

            // Save angle and rotation goal
            float currentAngle = Projectile.ai[1]; // starting angle
            float rotationSpeed = MathHelper.ToRadians(1.5f); // rotates 1.5° per frame
            int startRotating = 150;
            int rotateUntil = 140; // stop rotating at this timeLeft
            
            // ====== PART 1: ROTATE ONLY ======

            if (Projectile.timeLeft > rotateUntil && Projectile.timeLeft < startRotating)
            {
                // Gradually rotate angle clockwise
                Projectile.ai[1] += rotationSpeed;

                // Keep it within 0 to 2π
                if (Projectile.ai[1] > MathHelper.TwoPi)
                    Projectile.ai[1] -= MathHelper.TwoPi;

                // Update position based on rotated angle
                Vector2 offset = Projectile.ai[1].ToRotationVector2() * radius;
                Projectile.Center = bossCenter + offset;
            }


            // ====== PART 2: SHOOT OUTWARD ======

            if (Projectile.timeLeft == 120)
            {
                float shootAngle = Projectile.ai[1];
                float speed = 16f;
                Projectile.velocity = shootAngle.ToRotationVector2() * speed;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            // Fade in
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 20;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }
        }

        public override bool PreDraw(ref Color lightColor)
{
    Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
    Rectangle frame = texture.Frame(1, 6, 0, Projectile.frame);
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

}

