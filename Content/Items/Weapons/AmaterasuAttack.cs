using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace TheBattleCats.Content.Items.Weapons
{

public class AmaterasuIdle : ModProjectile
    {
        private const int totalIdleFrames = 1;
        public override string Texture => "TheBattleCats/Content/Items/Weapons/AmaterasuAttack";

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 40; // Stay active
        }

        public override void SetStaticDefaults()
        {
        Main.projFrames[Projectile.type] = 37;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.HeldItem.type == ModContent.ItemType<Amaterasu>())
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

            // Idle hover around player 10,-10
            Vector2 idleOffset = new Vector2(0f, 0f);
            Projectile.Center = player.Center + idleOffset + new Vector2(0f, (float)System.Math.Sin(Main.GameUpdateCount * 0.05f) * 2f);

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

        public override void DrawBehind(
            int index, 
            List<int> behindNPCsAndTiles, 
            List<int> behindNPCs, 
            List<int> behindProjectiles, 
            List<int> overPlayers, 
            List<int> overWiresUI)
        {
            // Make projectile draw over the player
            overPlayers.Add(index);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            Vector2 baseOffset = new Vector2(10f, -80f);

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



    public class AmaterasuAttack : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.timeLeft = 2; // Match number of frames (1 frame per tick)
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true; // Optional: only hits enemies in front
            Projectile.friendly = false;
            Projectile.hostile = false;
        }

        public override void SetStaticDefaults()
        {
        Main.projFrames[Projectile.type] = 37;
        }

        public override void AI()
{
    Player player = Main.player[Projectile.owner];

    // Optional: Stick projectile to player
    Projectile.Center = player.Center;

    // Face the same direction as the player
    Projectile.direction = player.direction;
    // Projectile.spriteDirection = -Projectile.direction;

    // ⏳ Keep the projectile alive if holding left-click
    if (player.channel && player.HeldItem.type == ModContent.ItemType<Amaterasu>())
    {
        Projectile.timeLeft = 2; // keeps it alive while holding button
    }
    else if (Projectile.frame >= 12)
    {
        spawnAmaterasuEnd();
    }

    // Animation control
    Projectile.frameCounter++;
    if (Projectile.frameCounter >= 3)
    {
        Projectile.frameCounter = 0;
        Projectile.frame++;

        if (Projectile.frame > 24 && Projectile.frameCounter == 0 && Projectile.frame % 3 == 0)
        {
            SpawnAmaterasuAttack(); 
        }

        if (Projectile.frame >= 28)
        {
            if (player.channel)
            {
                Projectile.frame = 24; // Loop back to attack loop section
            }
            else
            {
                Projectile.Kill(); // Stop attack if not holding anymore
                spawnAmaterasuEnd();
            }
        }
    }
}


private void spawnAmaterasuEnd()
{
    Player player = Main.player[Projectile.owner];

    // Spawn the wind-down animation projectile
    Projectile.NewProjectile(
        Projectile.GetSource_FromThis(),
        Projectile.Center,
        Vector2.Zero, // No movement, it stays still
        ModContent.ProjectileType<AmaterasuEnd>(), // Replace with your actual end animation projectile name
        0, // 0 damage if it's just visual
        0f, // 0 knockback
        Projectile.owner
    );
}


  private void SpawnAmaterasuAttack()
{
    Player player = Main.player[Projectile.owner];

    // Grid settings
    float horizontalSpacing = 30f; // space between pentagons
    float verticalSpacing = 30f;

    // Grid origin (top row is above player's head)
    Vector2 gridOrigin = player.Center + new Vector2(0f, -150f);

    // Store grid points in a list
    List<Vector2> pentagonCenters = new List<Vector2>();

    // Row 1: 3 pentagons
    for (int i = 0; i < 3; i++)
    {
        float x = (i - 1) * horizontalSpacing;
        float y = 0;
        pentagonCenters.Add(gridOrigin + new Vector2(x, y));
    }

    // Row 2: 4 pentagons (staggered)
    for (int i = 0; i < 4; i++)
    {
        float x = (i - 1.5f) * horizontalSpacing;
        float y = verticalSpacing;
        pentagonCenters.Add(gridOrigin + new Vector2(x, y));
    }

    // Row 3: 3 pentagons (same as row 1)
    for (int i = 0; i < 3; i++)
    {
        float x = (i - 1) * horizontalSpacing;
        float y = verticalSpacing * 2f;
        pentagonCenters.Add(gridOrigin + new Vector2(x, y));
    }

    // Pick a random pentagon position
    Vector2 spawnPosition = pentagonCenters[Main.rand.Next(pentagonCenters.Count)];

    // Aim at the mouse cursor
    Vector2 mouseWorld = Main.MouseWorld;
    Vector2 direction = mouseWorld - spawnPosition;
    Vector2 velocity = direction.SafeNormalize(Vector2.UnitY) * 8f;

    // Fire a test projectile
    int projType = ModContent.ProjectileType<AmaterasuProjectile>();

    Projectile.NewProjectile(
        Projectile.GetSource_FromThis(),
        spawnPosition,
        velocity,
        // ProjectileID.LaserMachinegunLaser,
        projType,
        Projectile.damage,
        2f,
        Projectile.owner,
        spawnPosition.X,
        spawnPosition.Y
    );
}


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            Vector2 baseOffset = new Vector2(10f, -80f);

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

public class AmaterasuEnd : ModProjectile
{
    private const int startFrame = 29;
    public override string Texture => "TheBattleCats/Content/Items/Weapons/AmaterasuAttack";
    public override void SetDefaults()
    {

        Projectile.timeLeft = 33;
    }

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 37;
    }

    public override void AI()
    {

        Player player = Main.player[Projectile.owner];

        // Optional: Stick projectile to player
        Projectile.Center = player.Center;

        // Face the same direction as the player
        Projectile.direction = player.direction;
        // Projectile.spriteDirection = -Projectile.direction;


        if (Projectile.frame < startFrame)
        {
            Projectile.frame = startFrame;
        }
        // Animation control
        Projectile.frameCounter++;
        if (Projectile.frameCounter >= 3)
        {
            Projectile.frameCounter = 0;
            Projectile.frame++;

            if (Projectile.frame >= 37)
            {

                Projectile.Kill();
            }
        }

    }
    
    public override void DrawBehind(
            int index, 
            List<int> behindNPCsAndTiles, 
            List<int> behindNPCs, 
            List<int> behindProjectiles, 
            List<int> overPlayers, 
            List<int> overWiresUI)
        {
            // Make projectile draw over the player
            overPlayers.Add(index);
        }
    
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

        int frameHeight = texture.Height / Main.projFrames[Projectile.type];
        Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
        Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

        Vector2 baseOffset = new Vector2(10f, -80f);

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