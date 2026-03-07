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
     
        
        private enum ActionState
        {
            Idle,
            Attack1,

        }

        public ref float AIState => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];

        private ActionState PreviousState = ActionState.Idle;

        

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
            NPC.width = 140;
            NPC.height = 140;
            NPC.boss = true;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 29;
        }

      

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 4) // 12 fps
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type])
                    NPC.frame.Y = 0;
            }
        }

        public override void AI()
        {
        

            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];
            
            if (!player.active || player.dead)
            {
                NPC.active = false;
                return;
            }

            ActionState CurrentState = (ActionState)AIState;

            if (CurrentState != PreviousState)
            {
                PreviousState = CurrentState;
            }

            switch (AIState)
            {
                case (float)ActionState.Idle:
                    Idle();
                    break;
                case (float)ActionState.Attack1:
                    Attack1();
                    break;

            }


        }

        private void Idle()
        {
            // hover in place, transition to Attack1 after a delay
            AITimer++;
            NPC.velocity *= 0.95f;

            if (AITimer >= 120f)
            {
                AITimer = 0f;
                AIState = (float)ActionState.Attack1;
            }
        }

        // orbit state variables
        private float OrbitAngle = 0f;        // current angle around the player
        private float OrbitRadius = 300f;     // how far from the player
        private const float OrbitSpeed = 0.03f;    // radians per tick
        private const float OrbitLaunchDegrees = 270f; // degrees to orbit before launching
        private Vector2 LaunchDirection = Vector2.Zero;
        private float LaunchTimer = 0f;

        private void Attack1()
        {
            Player player = Main.player[NPC.target];
            AITimer++;

            // Phase 1: disappear (0-60 ticks)
            if (AITimer < 60f)
            {
                NPC.velocity = Vector2.Zero;
                NPC.alpha = (int)MathHelper.Lerp(0, 255, AITimer / 60f); // fade out
                return;
            }

            // Phase 2: teleport to random position around player and start orbiting

            if (AITimer == 60f)
            {
                OrbitAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                Vector2 spawnOffset = new Vector2(OrbitRadius, 0f).RotatedBy(OrbitAngle);
                NPC.Center = player.Center + spawnOffset;
                NPC.alpha = 0; // reappear
                NPC.netUpdate = true;
            }


            // Phase 3: orbit around player
            if (AITimer >= 60f && AITimer < 60f + (OrbitLaunchDegrees / MathHelper.ToDegrees(OrbitSpeed)))
            {
                NPC.alpha = (int)MathHelper.Lerp(255, 0, (AITimer - 60f) / 60f);
                OrbitAngle += OrbitSpeed;

                Vector2 targetPos = player.Center + new Vector2(OrbitRadius, 0f).RotatedBy(OrbitAngle);
                NPC.velocity = targetPos - NPC.Center;

                return;
            }

            // Phase 4: launch at player center
            if (LaunchDirection == Vector2.Zero)
            LaunchDirection = Vector2.Normalize(player.Center - NPC.Center);

            NPC.velocity = LaunchDirection * 14f;

            // reset after reaching player
            LaunchTimer++;
            if (LaunchTimer >= 60 )
            {
                LaunchTimer = 0;
                AITimer = 0f;
                OrbitAngle = 0f;
                LaunchDirection = Vector2.Zero;
                AIState = (float)ActionState.Attack1;
            }
        }
}
}
