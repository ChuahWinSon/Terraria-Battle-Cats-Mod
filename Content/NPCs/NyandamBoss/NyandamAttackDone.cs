// using Microsoft.Xna.Framework;
// using Terraria;
// using Terraria.ModLoader;
// using Terraria.ID;
// using System;
// using Microsoft.Xna.Framework.Graphics;

// namespace TheBattleCats.Content.NPCs.NyandamBoss
// {
//     public class NyandamAttackDone : ModProjectile
//     {
//         public override void SetStaticDefaults()
//         {
//             Main.projFrames[Projectile.type] = 28;
//         }

//         public override void SetDefaults()
//         {
//             Projectile.width = 100; // Width of hitbox
//             Projectile.height = 120; // Height of hitbox
//             Projectile.timeLeft = 300; // Lasts for 60 frames
//             Projectile.penetrate = -1; // Doesn't disappear on hit
//             Projectile.tileCollide = false; // Doesn't hit tiles
//             Projectile.friendly = false; // Doesn't hurt other enemies
//             Projectile.hostile = true; // Damages the player
//             Projectile.damage = 0; // Damage dealt
//         }

//         public override void AI()
//         {   
//             Lighting.AddLight(Projectile.Center, 1f, 0.5f, 0f);
//             Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);

//             if (++Projectile.frameCounter >= 1) // Change every 5 ticks
//             {
//                 Projectile.frameCounter = 0; // Reset counter
//                 Projectile.frame++; // Move to the next frame
                
//                 // If we reached the last frame, loop back to the first frame
//                 if (Projectile.frame >= Main.projFrames[Projectile.type])
//                 {
//                     Projectile.frame = 0;
//                 }
//             }
//             Projectile.spriteDirection = (Projectile.direction == 1) ? -1 : 1;
            
//         }


//         public override bool PreDraw(ref Color lightColor)
// {
//     Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
//     Rectangle frame = texture.Frame(1, Main.projFrames[Projectile.type], 0, Projectile.frame);

//     SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
//     Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);

//     Vector2 baseOffset = new Vector2(48f, 0f); 

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
// }
