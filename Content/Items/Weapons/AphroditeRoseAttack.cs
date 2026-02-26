using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheBattleCats.Content.Items.Weapons
{

public class AphroditeIdle : ModProjectile
    {
        private const int totalIdleFrames = 1;
        public override string Texture => "TheBattleCats/Content/Items/Weapons/AphroditeRoseAttack";

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
        Main.projFrames[Projectile.type] = 50;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.HeldItem.type == ModContent.ItemType<AphroditeRose>())
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

            // Idle hover around player 10,-100
            Vector2 idleOffset = new Vector2(0f, 0f);
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

            Vector2 baseOffset = new Vector2(10f, -100f);

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



    public class AphroditeRoseAttack : ModProjectile
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
        Main.projFrames[Projectile.type] = 50;
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
    if (player.channel && player.HeldItem.type == ModContent.ItemType<AphroditeRose>())
    {
        Projectile.timeLeft = 2; // keeps it alive while holding button
    }
    else if (Projectile.frame >= 12)
    {
        spawnAphroditeEnd();
    }

    // Animation control
    Projectile.frameCounter++;
    if (Projectile.frameCounter >= 3)
    {
        Projectile.frameCounter = 0;
        Projectile.frame++;

        if (Projectile.frame == 36 && Projectile.frameCounter == 0)
        {
            SpawnAphroditeAttack(); 
        }

        if (Projectile.frame >= 38)
        {
            if (player.channel)
            {
                Projectile.frame = 14; // Loop back to attack loop section
            }
            else
            {
                Projectile.Kill(); // Stop attack if not holding anymore
                spawnAphroditeEnd();
            }
        }
    }
}


private void spawnAphroditeEnd()
{
    Player player = Main.player[Projectile.owner];

    // Spawn the wind-down animation projectile
    Projectile.NewProjectile(
        Projectile.GetSource_FromThis(),
        Projectile.Center,
        Vector2.Zero, // No movement, it stays still
        ModContent.ProjectileType<AphroditeEnd>(), // Replace with your actual end animation projectile name
        0, // 0 damage if it's just visual
        0f, // 0 knockback
        Projectile.owner
    );
}


        private void SpawnAphroditeAttack()
        {
    
    Player player = Main.player[Projectile.owner];
    Vector2 target = Main.MouseWorld;
    float ceilingLimit = target.Y;
    if (ceilingLimit > player.Center.Y - 200f)
    {
        ceilingLimit = player.Center.Y - 200f;
    }

    for (int i = 0; i < 5; i++)
    {   
        float xRandom = Main.rand.NextFloat(-200f, 200f);
        Vector2 position = target - new Vector2(xRandom , 600f); // X randomness, Y fixed above
        position.Y -= 100 * i;
        float screenYHeight = Main.screenPosition.Y + Main.screenHeight * 0.4f;
        Vector2 cursorPosition = Main.MouseWorld;

        Vector2 heading = target - position;

        if (heading.Y < 0f)
            heading.Y *= -1f;
        if (heading.Y < 20f)
            heading.Y = 20f;

        heading.Normalize();
        heading *= 16f; // Or any speed you want
        heading.Y += Main.rand.Next(-40, 41) * 0.02f;

        Projectile.NewProjectile(
        Projectile.GetSource_FromThis(),
        position,
        heading,
        ModContent.ProjectileType<AphroditeRoseProjectile>(), // replace with your projectile
        Projectile.damage,
        Projectile.knockBack,
        player.whoAmI,
        screenYHeight,
        cursorPosition.X,
        cursorPosition.Y
        );
    }
}

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            Vector2 baseOffset = new Vector2(10f, -100f);

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

public class AphroditeEnd : ModProjectile
{
    private const int startFrame = 38;
    public override string Texture => "TheBattleCats/Content/Items/Weapons/AphroditeRoseAttack";
    public override void SetDefaults()
    {

        Projectile.timeLeft = 33;
    }

    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 50;
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

            if (Projectile.frame >= 50)
            {

                Projectile.Kill();
            }
        }

    }
    
    public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRectangle = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);

            Vector2 baseOffset = new Vector2(10f, -100f);

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