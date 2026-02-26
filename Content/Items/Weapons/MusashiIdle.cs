using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.ID;

namespace TheBattleCats.Content.Items.Weapons
{
    public class MusashiIdle : ModProjectile
    {
        private const int totalIdleFrames = 1;

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 20; // Stay active
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.HeldItem.type == ModContent.ItemType<MusashisKatana>())
            {
                Projectile.timeLeft = 2; // keep it alive forever while held
            }
            else
            {
                Projectile.Kill(); // kill if not holding the item
            }
            // Projectile.spriteDirection = player.direction;

            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            // Idle hover around player
            Vector2 idleOffset = new Vector2(0f, -40f);
            Projectile.Center = player.Center + idleOffset + new Vector2(0f, (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 5f);

            // Idle animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 3)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
                if (Projectile.frame >= totalIdleFrames)
                {
                    Projectile.frame = 0;
                }
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
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, // Vertical flip for direction
                0f
            );

            return false;
        }
    }

    public class MusashiAttack1 : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.timeLeft = 24; // Match number of frames (1 frame per tick)
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true; // Optional: only hits enemies in front
            Projectile.friendly = false;
            Projectile.hostile = false;
        }

        public override void AI()
        {

            if (Projectile.frame == 0 && Projectile.frameCounter == 0)
            {
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
            }

            Player player = Main.player[Projectile.owner];

            // Optional: Stick projectile to player
            Projectile.Center = player.Center;

            // Face the same direction as the player
            Projectile.direction = player.direction;
            Projectile.spriteDirection = -Projectile.direction;

            // Animation control
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 1) // 1 tick per frame
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;

                // After 24 frames, spawn MusashiAttack2 and kill this projectile

                if (Projectile.frame == 11 && Projectile.frameCounter == 0)
                {
                    SpawnMusashiProjectile();
                    SpawnMusashiProjectileDone();
                    
                }

                if (Projectile.frame >= 24)
                {
                    SpawnMusashiAttack2();
                    Projectile.Kill(); // Kill MusashiAttack1
                }
            }
        }

        private void SpawnMusashiProjectile()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 toCursor = Main.MouseWorld - player.Center;
            int direction = toCursor.X >= 0 ? 1 : -1;

            // Lock the direction during the entire attack sequence
            player.direction = direction; // Lock direction during the attack

            // Spawn the hitbox projectile
            Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            player.Center,
            Vector2.Zero, // No velocity, stationary hitbox
            ModContent.ProjectileType<MusashiProjectile1>(),
            Projectile.damage, // Set this to your intended damage
            10f, // Knockback
            player.whoAmI,
            direction
        );
        }


        private void SpawnMusashiProjectileDone()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 toCursor = Main.MouseWorld - player.Center;
            int direction = toCursor.X >= 0 ? 1 : -1;

            // Lock the direction during the entire attack sequence
            player.direction = direction; // Lock direction during the attack

            // Spawn the hitbox projectile
            Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            player.Center,
            toCursor.SafeNormalize(Vector2.Zero) * 40f,
            ModContent.ProjectileType<MusashiProjectileDone>(),
            Projectile.damage, // Set this to your intended damage
            10f, // Knockback
            player.whoAmI,
            direction
        );
        }


        private void SpawnMusashiAttack2()
        {
            Player player = Main.player[Projectile.owner];
            Vector2 toCursor = Main.MouseWorld - player.Center;
            int direction = toCursor.X >= 0 ? 1 : -1;

            // Lock the direction during the entire attack sequence
            player.direction = direction; // Lock direction during the attack

            // Spawn MusashiAttack2
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                player.Center,
                Vector2.Zero, // No velocity, stationary
                ModContent.ProjectileType<MusashiAttack2>(),
                Projectile.damage,
                Projectile.knockBack,
                player.whoAmI,
                direction // pass cursor direction into ai[0] for attack direction
            );
        }




        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 24;
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
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, // Horizontal flip for direction
                0f
            );

            return false;
        }
    }
    
    public class MusashiAttack2 : ModProjectile
    {
        public int totalFrames = 40; // Frames for MusashiAttack2

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.timeLeft = totalFrames; // Duration of MusashiAttack2 animation
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.friendly = false;
            Projectile.hostile = false;
        }

public override void AI()
{


    if (Projectile.frame == 0 && Projectile.frameCounter == 0)
    {
        SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
    }

    Player player = Main.player[Projectile.owner];

    // Direction passed in
    int direction = (int)Projectile.ai[0];  // The locked direction for attack

    // Keep the direction locked during the attack
    Projectile.direction = direction;
    Projectile.spriteDirection = -direction;

    // Stick to player and handle animation
    Projectile.Center = player.Center;

    // Animation frame control
    Projectile.frameCounter++;
    if (Projectile.frameCounter >= 1)
    {
        Projectile.frameCounter = 0;
        Projectile.frame++;

        if (Projectile.frame >= totalFrames)
        {
            Projectile.Kill(); // Kill after finishing frames

            // Reset player direction if necessary (only after attack sequence)
            player.direction = direction; // Ensure player remains locked in the correct direction
        }

        if (Projectile.frame == 7 && Projectile.frameCounter == 0)
        {
            SpawnMusashiProjectileDone();
            SpawnMusashiProjectile();
        
        }
    }
}

private void SpawnMusashiProjectile()
{
    Player player = Main.player[Projectile.owner];
    Vector2 toCursor = Main.MouseWorld - player.Center;
    int direction = toCursor.X >= 0 ? 1 : -1;

    // Lock the direction during the entire attack sequence
    player.direction = direction; // Lock direction during the attack

    // Spawn the hitbox projectile
    Projectile.NewProjectile(
    Projectile.GetSource_FromThis(),
    player.Center,
    Vector2.Zero, // No velocity, stationary hitbox
    ModContent.ProjectileType<MusashiProjectile2>(),
    Projectile.damage, // Set this to your intended damage
    10f, // Knockback
    player.whoAmI,
    direction
);
}

private void SpawnMusashiProjectileDone()
{
    Player player = Main.player[Projectile.owner];
    Vector2 toCursor = Main.MouseWorld - player.Center;
    int direction = toCursor.X >= 0 ? 1 : -1;

    // Lock the direction during the entire attack sequence
    player.direction = direction; // Lock direction during the attack

    // Spawn the hitbox projectile
    Projectile.NewProjectile(
    Projectile.GetSource_FromThis(),
    player.Center,
    toCursor.SafeNormalize(Vector2.Zero) * 40f, 
    ModContent.ProjectileType<MusashiProjectileDone>(),
    Projectile.damage, // Set this to your intended damage
    10f, // Knockback
    player.whoAmI,
    direction
);
}

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = totalFrames; // Set the number of frames for MusashiAttack2
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
                Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, // Horizontal flip for direction
                0f
            );

            return false;
        }
    }

}
