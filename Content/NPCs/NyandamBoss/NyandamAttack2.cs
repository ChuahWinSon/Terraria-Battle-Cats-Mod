using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;


namespace TheBattleCats.Content.NPCs.NyandamBoss
{
    public class NyandamAttack2Projectile : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = 260; // Default fallback
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
            int bossID = (int)Projectile.ai[2];
            Projectile.rotation = (float)Projectile.ai[1] + MathHelper.PiOver2;

            if (!Main.npc.IndexInRange(bossID) || !Main.npc[bossID].active)
                return;

            NPC boss = Main.npc[bossID];
            Vector2 bossCenter = boss.Center;
            float radius = boss.width * 0.5f + 20f;

            // Save angle and rotation goal
            float currentAngle = Projectile.ai[1]; // starting angle
            float rotationSpeed = MathHelper.ToRadians(1.2f); 
            int rotateUntil = 120; // stop rotating at this timeLeft

            if (Projectile.timeLeft > rotateUntil)
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

            // When time hits 120, shoot outward
            if (Projectile.timeLeft == rotateUntil)
            {
                float shootAngle = Projectile.ai[1];
                float speed = 16f;
                Projectile.velocity = shootAngle.ToRotationVector2() * speed;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            // Fade in
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 10;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }

            
        }


    }


        public class NyandamEnhancedAttack2Projectile : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = 470; // Default fallback
        }

        public override void OnSpawn(IEntitySource Source)
        {
            // Despawn all projectiles at the same time based on boss's AITimer
            int globalDespawnTime = 470;

            Projectile.timeLeft = globalDespawnTime - (int)Projectile.ai[0];

            // Safety: if spawned too late, make sure it doesn't die instantly
            if (Projectile.timeLeft < 1)
                Projectile.timeLeft = 1;
        }

        public override void AI()
        {
            int bossID = (int)Projectile.ai[2];
            Projectile.rotation = (float)Projectile.ai[1] + MathHelper.PiOver2;

            if (!Main.npc.IndexInRange(bossID) || !Main.npc[bossID].active)
                return;

            NPC boss = Main.npc[bossID];
            Vector2 bossCenter = boss.Center;
            float radius = boss.width * 0.5f;

            // Save angle and rotation goal
            float currentAngle = Projectile.ai[1]; // starting angle
            float rotationSpeed = MathHelper.ToRadians(1.2f); 
            int rotateUntil = 210; // stop rotating at this timeLeft

            if (Projectile.timeLeft > rotateUntil)
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

            // When time hits 120, shoot outward
            if (Projectile.timeLeft == 210)
            {
                float shootAngle = Projectile.ai[1];
                float speed = 16f;
                Projectile.velocity = shootAngle.ToRotationVector2() * speed;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }

            // Fade in
            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 10;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }
        }



    }

}
