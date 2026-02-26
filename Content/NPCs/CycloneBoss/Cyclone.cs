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

namespace TheBattleCats.Content.NPCs.CycloneBoss
{
    [AutoloadBossHead]
    public class Cyclone : ModNPC
    {
        // ai[0] = state (-1=transition, 0=dash, 1=spawn minions, 2=reposition above, 3=shoot stingers, 4=catch up, 5=flee/dead)
        // ai[1] = state timer / last state
        // ai[2] = sub-state flag
        // ai[3] unused at top level

        public override void SetDefaults()
        {
            NPC.aiStyle = 0;

            NPC.damage = 50;
            NPC.defense = 12;
            NPC.lifeMax = 8000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.width = 300;
            NPC.height = 300;
            NPC.boss = true;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 10;
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[0] = 2f; // start by repositioning above player
            NPC.netUpdate = true;
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 5) // 12 fps
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type])
                    NPC.frame.Y = 0;
            }
        }

        public override void AI()
        {
            // Retarget if target is invalid or dead
            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
                NPC.TargetClosest();

            Player player = Main.player[NPC.target];
            bool playerDead = player.dead;

            float distToPlayer = Vector2.Distance(NPC.Center, player.Center);

            // Keep alive timer refreshed
            if (NPC.ai[0] != 5f && NPC.timeLeft < 60)
                NPC.timeLeft = 60;

            // If player too far, go into catch-up state
            if (NPC.ai[0] != 5f && distToPlayer > 3000f)
            {
                NPC.ai[0] = 4f;
                NPC.netUpdate = true;
            }

            // If player is dead, flee
            if (playerDead)
            {
                NPC.ai[0] = 5f;
                NPC.netUpdate = true;
            }

            if (NPC.ai[0] == 5f)
            {
                DoFlee();
                return;
            }

            if (NPC.ai[0] == -1f)
                DoTransition();
            else if (NPC.ai[0] == 0f)
                DoDash(player);
            else if (NPC.ai[0] == 1f)
                DoSpawnMinions(player);
            else if (NPC.ai[0] == 2f)
                DoReposition(player);
            else if (NPC.ai[0] == 3f)
                DoShootStingers(player, distToPlayer);
            else if (NPC.ai[0] == 4f)
                DoCatchUp(player, distToPlayer);

            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
        }

        private void DoFlee()
        {
            NPC.velocity.Y *= 0.98f;
            NPC.direction = NPC.velocity.X < 0f ? -1 : 1;

            if (NPC.position.X < Main.maxTilesX * 8)
            {
                if (NPC.velocity.X > 0f) NPC.velocity.X *= 0.98f;
                NPC.velocity.X -= 0.08f;
            }
            else
            {
                if (NPC.velocity.X < 0f) NPC.velocity.X *= 0.98f;
                NPC.velocity.X += 0.08f;
            }
        }

        private void DoTransition()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            float lastState = NPC.ai[1];
            int nextState;
            do
            {
                nextState = Main.rand.Next(3);
                switch (nextState)
                {
                    case 1: nextState = 2; break;
                    case 2: nextState = 3; break;
                }
            } while ((float)nextState == lastState);

            NPC.ai[0] = nextState;
            NPC.ai[1] = 0f;
            NPC.ai[2] = 0f;
            NPC.netUpdate = true;
        }

        private void DoDash(Player player)
        {
            int dashCount = 2;
            if (NPC.life < NPC.lifeMax / 2) dashCount++;
            if (NPC.life < NPC.lifeMax / 3) dashCount++;
            if (NPC.life < NPC.lifeMax / 5) dashCount++;

            if (NPC.ai[1] > (float)(2 * dashCount) && NPC.ai[1] % 2f == 0f)
            {
                NPC.ai[0] = -1f;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.netUpdate = true;
                return;
            }

            if (NPC.ai[1] % 2f == 0f)
            {
                // Even: align Y with player then dash
                NPC.TargetClosest();
                float yThreshold = 20f;

                if (Math.Abs(NPC.Center.Y - player.Center.Y) < yThreshold)
                {
                    // Aligned - launch the dash
                    NPC.localAI[0] = 1f;
                    NPC.ai[1] += 1f;
                    NPC.ai[2] = 0f;
                    NPC.netUpdate = true;

                    float dashSpeed = 12f;
                    if (NPC.life < NPC.lifeMax * 0.75f) dashSpeed = 14f;
                    if (NPC.life < NPC.lifeMax * 0.5f)  dashSpeed = 16f;
                    if (NPC.life < NPC.lifeMax * 0.25f) dashSpeed = 18f;
                    if (NPC.life < NPC.lifeMax * 0.1f)  dashSpeed = 20f;

                    Vector2 toPlayer = player.Center - NPC.Center;
                    float dist = toPlayer.Length();
                    NPC.velocity.X = (toPlayer.X / dist) * dashSpeed;
                    NPC.velocity.Y = (toPlayer.Y / dist) * dashSpeed;
                    return;
                }

                // Not aligned - move to player Y level
                NPC.localAI[0] = 0f;
                float maxSpeed = 12f;
                float accel = 0.15f;
                if (NPC.life < NPC.lifeMax * 0.75f) { maxSpeed = 13f; accel += 0.05f; }
                if (NPC.life < NPC.lifeMax * 0.5f)  { maxSpeed = 14f; accel += 0.05f; }
                if (NPC.life < NPC.lifeMax * 0.25f) { maxSpeed = 16f; accel += 0.05f; }
                if (NPC.life < NPC.lifeMax * 0.1f)  { maxSpeed = 18f; accel += 0.1f; }

                if (NPC.Center.Y < player.Center.Y) NPC.velocity.Y += accel;
                else NPC.velocity.Y -= accel;

                if (NPC.velocity.Y < -maxSpeed) NPC.velocity.Y = -maxSpeed;
                if (NPC.velocity.Y > maxSpeed)  NPC.velocity.Y = maxSpeed;

                float diffX = Math.Abs(NPC.Center.X - player.Center.X);
                if (diffX > 600f)      NPC.velocity.X += 0.15f * NPC.direction;
                else if (diffX < 300f) NPC.velocity.X -= 0.15f * NPC.direction;
                else                   NPC.velocity.X *= 0.8f;

                if (NPC.velocity.X < -16f) NPC.velocity.X = -16f;
                if (NPC.velocity.X > 16f)  NPC.velocity.X = 16f;
                return;
            }

            // Odd: decelerate after dash, wait until slow then go again
            NPC.direction = NPC.velocity.X < 0f ? -1 : 1;

            int slowThreshold = 600;
            if (NPC.life < NPC.lifeMax * 0.1f)       slowThreshold = 300;
            else if (NPC.life < NPC.lifeMax * 0.25f) slowThreshold = 450;
            else if (NPC.life < NPC.lifeMax * 0.5f)  slowThreshold = 500;
            else if (NPC.life < NPC.lifeMax * 0.75f) slowThreshold = 550;

            int oppositeDir = (NPC.position.X + NPC.width / 2) < (player.position.X + player.width / 2) ? -1 : 1;

            bool flag = false;
            if (NPC.direction == oppositeDir && Math.Abs(NPC.Center.X - player.Center.X) > slowThreshold)
            {
                NPC.ai[2] = 1f;
                flag = true;
            }
            if (Math.Abs(NPC.Center.Y - player.Center.Y) > slowThreshold * 1.5f)
            {
                NPC.ai[2] = 1f;
                flag = true;
            }

            if (NPC.ai[2] == 1f)
            {
                NPC.TargetClosest();
                NPC.velocity *= 0.9f;
                float stopSpeed = 0.1f;

                if (Math.Abs(NPC.velocity.X) + Math.Abs(NPC.velocity.Y) < stopSpeed)
                {
                    NPC.ai[2] = 0f;
                    NPC.ai[1] += 1f;
                    NPC.netUpdate = true;
                }
            }
            else
            {
                NPC.localAI[0] = 1f;
            }
        }

        private void DoSpawnMinions(Player player)
        {
            // Placeholder - add custom minion spawning here later
            NPC.ai[0] = -1f;
            NPC.ai[1] = 1f;
            NPC.netUpdate = true;
        }

        private void DoReposition(Player player)
        {
            NPC.TargetClosest();
            float accel = 0.07f;

            Vector2 target = new Vector2(player.Center.X, player.Center.Y - 200f);
            Vector2 toTarget = target - NPC.Center;
            float dist = toTarget.Length();

            if (dist < 200f)
            {
                NPC.ai[0] = -1f;
                NPC.ai[1] = 2f;
                NPC.netUpdate = true;
                return;
            }

            if (NPC.velocity.X < toTarget.X) { NPC.velocity.X += accel; if (NPC.velocity.X < 0f && toTarget.X > 0f) NPC.velocity.X += accel; }
            else { NPC.velocity.X -= accel; if (NPC.velocity.X > 0f && toTarget.X < 0f) NPC.velocity.X -= accel; }

            if (NPC.velocity.Y < toTarget.Y) { NPC.velocity.Y += accel; if (NPC.velocity.Y < 0f && toTarget.Y > 0f) NPC.velocity.Y += accel; }
            else { NPC.velocity.Y -= accel; if (NPC.velocity.Y > 0f && toTarget.Y < 0f) NPC.velocity.Y -= accel; }
        }

        private void DoShootStingers(Player player, float distToPlayer)
        {
            NPC.TargetClosest();

            Vector2 spawnOffset = new Vector2(
                NPC.Center.X + Main.rand.Next(20) * NPC.direction,
                NPC.position.Y + NPC.height * 0.8f
            );

            Vector2 toPlayer = player.Center - NPC.Center;
            float dist = toPlayer.Length();

            NPC.ai[1] += 1f;

            int fireRate = 40;
            if (NPC.life < NPC.lifeMax * 0.1f)       fireRate = 15;
            else if (NPC.life < NPC.lifeMax * 0.25f) fireRate = 25;
            else if (NPC.life < NPC.lifeMax * 0.5f)  fireRate = 30;
            else if (NPC.life < NPC.lifeMax * 0.75f) fireRate = 35;

            if (NPC.ai[1] % (float)fireRate == (float)(fireRate - 1)
                && NPC.position.Y + NPC.height < player.position.Y
                && Collision.CanHit(spawnOffset, 1, 1, player.position, player.width, player.height))
            {
                SoundEngine.PlaySound(SoundID.Item17, NPC.position);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float projSpeed = 8f;
                    if (NPC.life < NPC.lifeMax * 0.1f) projSpeed = 13f;

                    float spreadX = player.Center.X - spawnOffset.X + Main.rand.Next(-80, 81);
                    float spreadY = player.Center.Y - spawnOffset.Y + Main.rand.Next(-40, 41);
                    float len = (float)Math.Sqrt(spreadX * spreadX + spreadY * spreadY);
                    len = projSpeed / len;
                    spreadX *= len;
                    spreadY *= len;

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnOffset.X, spawnOffset.Y,
                        spreadX, spreadY, ProjectileID.Stinger, NPC.damage / 2, 0f, Main.myPlayer);
                }
            }

            if (dist > 400f || !Collision.CanHit(new Vector2(spawnOffset.X, spawnOffset.Y - 30f), 1, 1, player.position, player.width, player.height))
            {
                float hoverAccel = 0.1f;
                Vector2 hoverTarget = player.Center - NPC.Center;

                if (NPC.velocity.X < hoverTarget.X) { NPC.velocity.X += hoverAccel; if (NPC.velocity.X < 0f && hoverTarget.X > 0f) NPC.velocity.X += hoverAccel; }
                else { NPC.velocity.X -= hoverAccel; if (NPC.velocity.X > 0f && hoverTarget.X < 0f) NPC.velocity.X -= hoverAccel; }

                if (NPC.velocity.Y < hoverTarget.Y) { NPC.velocity.Y += hoverAccel; if (NPC.velocity.Y < 0f && hoverTarget.Y > 0f) NPC.velocity.Y += hoverAccel; }
                else { NPC.velocity.Y -= hoverAccel; if (NPC.velocity.Y > 0f && hoverTarget.Y < 0f) NPC.velocity.Y -= hoverAccel; }
            }
            else
            {
                NPC.velocity *= 0.9f;
            }

            float stateLength = 20f;
            if (NPC.ai[1] > (float)fireRate * stateLength)
            {
                NPC.ai[0] = -1f;
                NPC.ai[1] = 3f;
                NPC.netUpdate = true;
            }
        }

        private void DoCatchUp(Player player, float distToPlayer)
        {
            float speed = 14f;
            float inertia = 14f;
            Vector2 toPlayer = player.Center - NPC.Center;
            toPlayer.Normalize();
            toPlayer *= speed;
            NPC.velocity = (NPC.velocity * inertia + toPlayer) / (inertia + 1f);

            NPC.direction = NPC.velocity.X < 0f ? -1 : 1;

            if (distToPlayer < 2000f)
            {
                NPC.ai[0] = -1f;
                NPC.localAI[0] = 0f;
            }
        }
    }
}