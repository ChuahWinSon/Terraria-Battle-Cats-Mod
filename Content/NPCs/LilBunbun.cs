using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using System; 

namespace TheBattleCats.Content.NPCs
{
    public class LilBunbun : ModNPC
    {
        private enum ActionState
        {
            Walking,
            Attacking,
        }

        public ref float AI_State => ref NPC.ai[0]; // Tracks NPC state
        public ref float AI_Timer => ref NPC.ai[1]; // Tracks time in state

        public override void SetStaticDefaults()
        {       
            Main.npcFrameCount[NPC.type] = 24;
        }

        public override void SetDefaults()
        {
            NPC.width = 45;
            NPC.height = 80;
            NPC.damage = 20;
            NPC.knockBackResist = 0f;
            NPC.defense = 20;
            NPC.lifeMax = 1000;
            NPC.value = 10000f;
            NPC.boss = false;
            NPC.npcSlots = 1f;
            NPC.friendly = false;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1; // Custom AI
            NPC.stepSpeed = 5f; // Helps NPC walk up slopes smoothly

        }

        private bool isAttacking = false;
        private int attackDuration = 60; // Attack lasts for 60 frames =60  (1 second)

        public override void AI()
        {
            Player target = Main.player[NPC.target]; // Get player target
            if (!target.active || target.dead)
            {
                NPC.TargetClosest();
                return;
            }



            float distanceToPlayer = Vector2.Distance(NPC.Center, target.Center);

            if (!isAttacking && distanceToPlayer < 60f) // Close enough to attack
            {
                StartAttack();
            }
            else if (!isAttacking) // Otherwise, walk towards the player
            {
                AI_State = (float)ActionState.Walking;
                Vector2 directionToPlayer = target.Bottom - new Vector2(NPC.Center.X, NPC.Bottom.Y);
                directionToPlayer.Normalize(); // Makes the vector have a length of 1

                float speed = 8f; // Adjust speed as needed
                float acceleration = 0.1f; // Adjust acceleration to make movement smoother

                // Smoothly move toward the player
                NPC.velocity = (NPC.velocity * (1f - acceleration)) + (directionToPlayer * (speed * acceleration));
                
            }

            if (isAttacking)
            {
                AI_Timer++; // Increment attack timer


                if (AI_Timer >= attackDuration) // Attack complete
                {
                    isAttacking = false;
                    AI_State = (float)ActionState.Walking;
                    AI_Timer = 0;
                }
                if (AI_Timer == 50) 
                {
                    
                    int projType = ModContent.ProjectileType<LilBunbunAttack>();
                    Vector2 spawnPosition = NPC.Center + new Vector2(NPC.direction *40 , 10); // Offset in front of NPC
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, Vector2.Zero, projType, 50, 10, Main.myPlayer);

                }
            }

            // Make NPC face the player
            if (target.Center.X < NPC.Center.X)
            {
                NPC.direction = -1;
                NPC.spriteDirection = -1;
            }
            else
            {
                NPC.direction = 1;
                NPC.spriteDirection = 1;
            }



        }

        private void StartAttack()
        {
            isAttacking = true;
            NPC.velocity = Vector2.Zero; // Stop moving
            AI_State = (float)ActionState.Attacking;
            AI_Timer = 0;
        }

        public override void FindFrame(int frameHeight)
        {
            if (isAttacking)
            {
                NPC.frame.Y = (12 + (int)(AI_Timer / 6) % 12) * frameHeight; // Attack animation
            }
            else
            {
                NPC.frame.Y = ((int)(NPC.frameCounter / 6) % 12) * frameHeight; // Walking animation
            }

            NPC.frameCounter++;
        }
        public override void DrawBehind(int index) {
            Main.instance.DrawCacheNPCProjectiles.Add(index);
        }
    
    }
}
