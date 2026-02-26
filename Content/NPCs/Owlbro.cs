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

namespace TheBattleCats.Content.NPCs
{
    public class Owlbro : ModNPC
    {
        private enum ActionState
        {
            Idle,
            Flying,
            Attack
        }

        private ActionState State
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        // How long the NPC has been in the flying state
        private ref float FlyTimer => ref NPC.ai[1];

        // How long before doing another attack after landing
        private ref float AttackCooldown => ref NPC.ai[2];

        private ref float DashVelocityX => ref NPC.ai[3];

        // How long to fly before attacking (180 ticks = 3 seconds)
        private const int FlyTimeBeforeAttack = 180;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7; // 1 idle + 5 flying + 1 attack
        }

        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 36;
            NPC.aiStyle = 0;
            NPC.damage = 15;
            NPC.defense = 4;
            NPC.lifeMax = 40;
            NPC.knockBackResist = 0.8f;
            NPC.HitSound = SoundID.NPCHit28;
            NPC.DeathSound = SoundID.NPCDeath31;
            NPC.value = 60f;
            NPC.noGravity = false;
        }

        public override void AI()
        {
            if (State == ActionState.Idle)
                DoIdle();
            else if (State == ActionState.Flying)
                DoFlying();
            else if (State == ActionState.Attack)
                DoAttack();

            // Float out of water
            if (NPC.wet)
            {
                if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y *= 0.95f;
                NPC.velocity.Y -= 0.5f;
                if (NPC.velocity.Y < -4f)
                    NPC.velocity.Y = -4f;
                NPC.TargetClosest();
            }
        }

        private void DoIdle()
        {
            NPC.noGravity = false;
            NPC.TargetClosest();

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (NPC.velocity.X != 0f || NPC.velocity.Y < 0f || NPC.velocity.Y > 0.3f)
                {
                    State = ActionState.Flying;
                    FlyTimer = 0f;
                    NPC.netUpdate = true;
                }
                else
                {
                    Player player = Main.player[NPC.target];
                    Rectangle playerRect = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
                    Rectangle aggroRect = new Rectangle((int)NPC.position.X - 100, (int)NPC.position.Y - 100, NPC.width + 200, NPC.height + 200);

                    if (aggroRect.Intersects(playerRect) || NPC.life < NPC.lifeMax)
                    {
                        State = ActionState.Flying;
                        FlyTimer = 0f;
                        NPC.velocity.Y -= 6f;
                        NPC.netUpdate = true;
                    }
                }
            }
        }

        private void DoFlying()
        {
            NPC.noGravity = true;

            Player player = Main.player[NPC.target];

            if (player.dead)
                return;

            // Bounce off walls
            if (NPC.collideX)
            {
                NPC.velocity.X = NPC.oldVelocity.X * -0.5f;
                if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 2f) NPC.velocity.X = 2f;
                if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -2f) NPC.velocity.X = -2f;
            }

            if (NPC.collideY)
            {
                NPC.velocity.Y = NPC.oldVelocity.Y * -0.5f;
                if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1f) NPC.velocity.Y = 1f;
                if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1f) NPC.velocity.Y = -1f;
            }

            NPC.TargetClosest();

            // Horizontal movement toward player
            if (NPC.direction == -1 && NPC.velocity.X > -3f)
            {
                NPC.velocity.X -= 0.1f;
                if (NPC.velocity.X > 3f)  NPC.velocity.X -= 0.1f;
                else if (NPC.velocity.X > 0f) NPC.velocity.X -= 0.05f;
                if (NPC.velocity.X < -3f) NPC.velocity.X = -3f;
            }
            else if (NPC.direction == 1 && NPC.velocity.X < 3f)
            {
                NPC.velocity.X += 0.1f;
                if (NPC.velocity.X < -3f) NPC.velocity.X += 0.1f;
                else if (NPC.velocity.X < 0f) NPC.velocity.X += 0.05f;
                if (NPC.velocity.X > 3f) NPC.velocity.X = 3f;
            }

            // Hover above player
            float distX = Math.Abs(NPC.Center.X - player.Center.X);
            float targetY = player.position.Y - NPC.height / 2;
            if (distX > 50f)
                targetY -= 100f;

            if (NPC.position.Y < targetY)
            {
                NPC.velocity.Y += 0.03f;
                if (NPC.velocity.Y < 0f) NPC.velocity.Y += 0.01f;
            }
            else
            {
                NPC.velocity.Y -= 0.05f;
                if (NPC.velocity.Y > 0f) NPC.velocity.Y -= 0.01f;
            }

            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -3f, 3f);

            // Count up fly timer, switch to attack after threshold
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                FlyTimer++;
                if (FlyTimer >= FlyTimeBeforeAttack && AttackCooldown <= 0f)
                {
                    State = ActionState.Attack;
                    FlyTimer = 0f;

                    // Aim dash toward where the player is moving
                    Vector2 dashTarget = player.Center + player.velocity * 10f;
                    Vector2 dashDir = dashTarget - NPC.Center;
                    dashDir.Normalize();

                    // Dash mostly downward with some horizontal lean toward player movement
                    NPC.velocity.X = dashDir.X * 6f;
                    NPC.velocity.Y = 1f;
                    DashVelocityX = NPC.velocity.X; 

                    NPC.netUpdate = true;
                }
            }
        }

        private void DoAttack()
        {
            NPC.noGravity = false;
            NPC.velocity.X = DashVelocityX; // reapply every tick

            if (NPC.velocity.Y == 0f || NPC.collideY )
            {
                AttackCooldown = 60f;
                State = ActionState.Flying;
                FlyTimer = 0f;
                NPC.noGravity = true;
                NPC.velocity.Y = -4f;
                NPC.netUpdate = true;
                return;
            }

            NPC.TargetClosest();
        }

        public override void PostAI()
        {
            if (AttackCooldown > 0f)
                AttackCooldown--;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.spriteDirection = NPC.direction;
            NPC.rotation = NPC.velocity.X * 0.1f;

            if (State == ActionState.Idle)
            {
                // Frame 0 = idle
                NPC.frame.Y = 0;
                NPC.frameCounter = 0;
            }
            else if (State == ActionState.Flying)
            {
                // Frames 1-5 = flying cycle
                int frameSpeed = 5;
                int flyingFrames = 5;
                NPC.frameCounter++;
                if (NPC.frameCounter >= frameSpeed * flyingFrames)
                    NPC.frameCounter = 0;

                int frame = (int)(NPC.frameCounter / frameSpeed);
                NPC.frame.Y = (frame + 1) * frameHeight;
            }
            else if (State == ActionState.Attack)
            {
                // Flip sprite based on which horizontal direction we're dashing
                NPC.spriteDirection = NPC.velocity.X < 0f ? 1 : -1;
                
                // Frame 6 = attack
                NPC.frame.Y = 6 * frameHeight;
                NPC.frameCounter = 0;
            }
        }


        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Spawn on surface forest during daytime
            if (spawnInfo.Player.ZoneForest && 
                spawnInfo.SpawnTileY <= Main.worldSurface &&
                Main.dayTime)
            {
                return 0.1f;
            }

            return 0f;
        }
    }
}