using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using System;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics; 
using Terraria.ID;

namespace TheBattleCats.Content.NPCs.NyandamBoss
{


public class NyandamPortal : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 24; // same as old Attack2
    }

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 120;
        Projectile.timeLeft = 150; // long enough to still be visible when proj fires
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
        Projectile.hostile = false; // portal itself doesnt damage
        Projectile.damage = 0;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 0.7f, 0f, 0.9f);

        if (++Projectile.frameCounter >= 10)
        {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;
        }
    }

public override bool PreDraw(ref Color lightColor)
{
    Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
    Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
    Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

    Main.spriteBatch.Draw(
        texture,
        Projectile.Center - Main.screenPosition,
        frame,
        lightColor,
        MathHelper.PiOver2, // 90 degrees, faces up
        origin,
        Projectile.scale,
        SpriteEffects.None,
        0f
    );

    return false;
}
}



public class NyandamAttack5Projectile : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 28; // same as old AttackDone
    }

    public override void SetDefaults()
    {
        Projectile.width = 120;
        Projectile.height = 120;
        Projectile.timeLeft = 300;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.damage = 0;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 1f, 0.5f, 0f);
        Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);

        if (++Projectile.frameCounter >= 1)
        {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;
        }
    }

public override bool PreDraw(ref Color lightColor)
{
    Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
    Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
    Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

    Main.spriteBatch.Draw(
        texture,
        Projectile.Center - Main.screenPosition,
        frame,
        lightColor,
        MathHelper.PiOver2, // 90 degrees, faces up
        origin,
        Projectile.scale,
        SpriteEffects.None,
        0f
    );

    return false;
}
}


public class EnhancedNyandamPortal : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 24;
    }

    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 120;
        Projectile.timeLeft = 92; // long enough to still be visible when proj fires
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.friendly = false;
        Projectile.hostile = false; // portal itself doesnt damage
        Projectile.damage = 0;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 0.7f, 0f, 0.9f);

        if (++Projectile.frameCounter >= 6)
        {
            Projectile.frameCounter = 0;
            Projectile.frame++;
            if (Projectile.frame >= Main.projFrames[Projectile.type])
                Projectile.frame = 0;
        }
    }

public override bool PreDraw(ref Color lightColor)
{
    Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
    Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);
    Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

    Main.spriteBatch.Draw(
        texture,
        Projectile.Center - Main.screenPosition,
        frame,
        lightColor,
        MathHelper.PiOver2, // 90 degrees, faces up
        origin,
        Projectile.scale,
        SpriteEffects.None,
        0f
    );

    return false;
}
}


}









//     public class NyandamAttack2 : ModProjectile
//     {
//         public override void SetStaticDefaults()
//         {
//             Main.projFrames[Projectile.type] = 24; 
//         }

//         public override void SetDefaults()
//         {
//             Projectile.width = 10; // Width of hitbox
//             Projectile.height = 120; // Height of hitbox
//             Projectile.timeLeft = 60; // Lasts for 120 frames
//             Projectile.penetrate = -1; // Doesn't disappear on hit
//             Projectile.tileCollide = false; // Doesn't hit tiles
//             Projectile.friendly = false; // Doesn't hurt other enemies
//             Projectile.hostile = true; // Damages the player
//             Projectile.damage = 0; // Damage dealt
//         }

// public override void AI()
// {   
//     Lighting.AddLight(Projectile.Center, 0.7f, 0f, 0.9f); 
//     // Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
//     Projectile.direction = (int)Projectile.ai[0];
//     Projectile.spriteDirection = Projectile.ai[0] < 0 ? 1 : -1;

//     Projectile.ai[1] = Projectile.spriteDirection;

//     // Every 5 ticks, move to the next frame
//     if (++Projectile.frameCounter >= 10) // Change every 5 ticks
//     {
//         Projectile.frameCounter = 0; // Reset counter
//         Projectile.frame++; // Move to the next frame
        
//         // If we reached the last frame, loop back to the first frame
//         if (Projectile.frame >= Main.projFrames[Projectile.type])
//         {
//             Projectile.frame = 0;
//         }
//     }

//     // When it's about to disappear (timeLeft == 2), spawn the next projectile
//     if (Projectile.timeLeft == 2) 
//     {
//         int newProjType = ModContent.ProjectileType<NyandamAttackDone>(); // Next projectile

//         // **Flipping logic**
//         // If the projectile is facing right (1), the next attack spawns on the left but faces left.
//         // If the projectile is facing left (-1), the next attack spawns on the right but faces right.
//         Vector2 spawnPosition = Projectile.Center - new Vector2(Projectile.direction, 0); // Adjust spawn position based on direction
//         Projectile.spriteDirection = (Projectile.direction == 1) ? -1 : 1; // Flip sprite direction
//         Vector2 velocity = new Vector2(8f * Projectile.spriteDirection, 0);
//         // Now the projectiles flip based on direction
//         Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity, newProjType, Projectile.damage, 30, Projectile.owner, 0f, Projectile.spriteDirection);
//     }
// }


// public override bool PreDraw(ref Color lightColor)
// {
//     Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
//     Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);

//     SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
//     Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

//     Vector2 baseOffset = new Vector2(20f, 0f); 

// 	// Flip X offset when spriteDirection is -1
// 	Vector2 offset = new Vector2(baseOffset.X * Projectile.spriteDirection, baseOffset.Y);

//     Main.spriteBatch.Draw(
//         texture,
//         Projectile.Center - Main.screenPosition + offset,
//         frame,
//         lightColor,
//         Projectile.rotation,
//         origin,
//         Projectile.scale,
//         effects,
//         0f
//     );

//     return false; // Prevent default drawing
// }


//     }