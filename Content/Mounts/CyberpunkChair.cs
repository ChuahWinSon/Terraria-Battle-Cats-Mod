using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace TheBattleCats.Content.Mounts
{
    public class CyberpunkChair : ModMount
    {
        public override void SetStaticDefaults()
        {
            // Movement
            MountData.jumpHeight       = 0;
            MountData.jumpSpeed        = 0f;
            MountData.blockExtraJumps  = true;
            MountData.constantJump     = false;
            MountData.heightBoost      = 0;
            MountData.fallDamage       = 0f;
            MountData.runSpeed         = 9f;
            MountData.dashSpeed        = 9f;
            MountData.acceleration     = 0.4f;
            MountData.flightTimeMax    = int.MaxValue; // infinite flight like UFO

            // Misc
            MountData.fatigueMax = 0;
            MountData.buff       = ModContent.BuffType<Buffs.CyberpunkChairBuff>();

            // Frame data and player offsets
            MountData.totalFrames      = 1;
            MountData.playerYOffsets   = Enumerable.Repeat(20, MountData.totalFrames).ToArray();
            MountData.xOffset          = 6;
            MountData.yOffset          = -24;
            MountData.playerHeadOffset = 0;
            MountData.bodyFrame        = 3;

           
            if (!Main.dedServ) {
				MountData.textureWidth = MountData.backTexture.Width() + 20;
				MountData.textureHeight = MountData.backTexture.Height();
			}
        }

        public override void UpdateEffects(Player player)
        {
            // Suppress gravity so the mount hovers (UFO-style)
            player.noFallDmg = true;
            player.wingTime  = 2;

            if (!player.controlDown)
                player.velocity.Y -= 0.3f;

            player.velocity.Y = MathHelper.Clamp(player.velocity.Y, -9f, 9f);

            // Idle bob
            if (!player.controlUp && !player.controlDown)
            {
                float bob = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 3f;
                player.position.Y += bob * 0.05f;
            }

            // Electric dust exhaust underneath
            if (Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(
                    player.position + new Vector2(Main.rand.Next(-20, 20), player.height + 4),
                    4, 4,
                    226, // DustID.Electric
                    player.velocity.X * 0.5f,
                    2f,
                    150, default, 0.8f);

                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
        }
    }
}