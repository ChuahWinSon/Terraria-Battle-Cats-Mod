using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using System; 
using TheBattleCats.Content.NPCs.TeacherBunbunBoss;
using Terraria.Audio;


namespace TheBattleCats.Content.NPCs.TeacherBunbunBoss
{

    [AutoloadBossHead]
    public class TeacherBunbun : ModNPC
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
            Main.npcFrameCount[NPC.type] = 64;
        }

        public override void SetDefaults()
        {
            NPC.width = 90;
            NPC.height = 145;
            NPC.damage = 40;
            NPC.knockBackResist = 0f;
            NPC.defense = 20;
            NPC.lifeMax = 4000;
            NPC.value = 10000f;
            NPC.boss = true;
            NPC.npcSlots = 1f;
            NPC.friendly = false;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1; // Custom AI
            NPC.stepSpeed = 5f; 
            NPC.npcSlots = 10f;
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/BossFight1");

        }

        

        private bool isAttacking = false;
        private bool SpawnedMinions = false;
        private int attackDuration = 100; // Attack lasts for 60 frames (1 second)

        public override void AI()
        {
            Player target = Main.player[NPC.target]; // Get player target
            if (!target.active || target.dead)
            {
                NPC.TargetClosest();
                return;
            }



            float distanceToPlayer = Vector2.Distance(NPC.Center, target.Center);

            if (!isAttacking && distanceToPlayer < 100f) // Close enough to attack
            {
                StartAttack();
            }
            else if (!isAttacking) // Otherwise, walk towards the player
            {
                AI_State = (float)ActionState.Walking;
                Vector2 directionToPlayer = target.Bottom - new Vector2(NPC.Center.X, NPC.Bottom.Y);
                directionToPlayer.Normalize(); // Makes the vector have a length of 1

                float speed = 6f; // Adjust speed as needed
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
                if (AI_Timer == 70) 
                {
                    
                    int projType = ModContent.ProjectileType<TeacherBunbunAttack>();
                    Vector2 spawnPosition = NPC.Center + new Vector2(NPC.direction * 80 , 50); // Offset in front of NPC
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, Vector2.Zero, projType, 50, 10, Main.myPlayer);
                    int projType2 = ModContent.ProjectileType<TeacherBunbunSplash>();
                    Vector2 spawnPosition2 = NPC.Center + new Vector2(NPC.direction * 60 , 50);
                    for (int angle = 0; angle < 360; angle += 30) // 12 projectiles (every 30°)
                    {
                        float radians = MathHelper.ToRadians(angle);
                        Vector2 velocity = new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians)) * 7f;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition2, velocity, projType2, 20, 10, Main.myPlayer);
                    }

				    // If the player using the item is the client
				    // (explicitly excluded serverside here)
				    SoundStyle customSound = new SoundStyle("TheBattleCats/Assets/Effects/CriticalHit")

				    {
    			        Volume = 0.9f,  // 90%
    			        Pitch = 0.0f,   // Normal pitch
    			        PitchVariance = 0.0f // Adds random pitch variation
				    };

				    // Play the sound at NPC's position
				    SoundEngine.PlaySound(customSound, NPC.position);
                    
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
            SpawnMinions();

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
                NPC.frame.Y = (32 + (int)(AI_Timer / 3) % 32) * frameHeight; // Attack animation
            }
            else
            {
                NPC.frame.Y = ((int)(NPC.frameCounter / 3) % 32) * frameHeight; // Walking animation
            }

            NPC.frameCounter++;
        }

        private void SpawnMinions() {
            if (!SpawnedMinions && NPC.life <= NPC.lifeMax / 2) // 50% HP check
            {     
			    SpawnedMinions = true;

			    if (Main.netMode == NetmodeID.MultiplayerClient) {
				// Because we want to spawn minions, and minions are NPCs, we have to do this on the server (or singleplayer, "!= NetmodeID.MultiplayerClient" covers both)
				// This means we also have to sync it after we spawned and set up the minion
				    return;
			    }   

                // Get the player target
                Player target = Main.player[NPC.target];

                // Define spawn distance from player
                float spawnDistanceX = 800f; // Adjust for farther spawns
                float spawnDistanceY = 400f;

                // Randomly choose left (-1) or right (1)
                int directionX = Main.rand.NextBool() ? -1 : 1;

                // Calculate spawn position relative to the player
                Vector2 spawnPosition = new Vector2(
                target.Center.X + (spawnDistanceX * directionX), 
                target.Center.Y + (spawnDistanceY * -1)
                );

                // Get entity source
                var entitySource = NPC.GetSource_FromAI();

                // Spawn minion off-screen
                NPC minionNPC = NPC.NewNPCDirect(entitySource, (int)spawnPosition.X, (int)spawnPosition.Y, ModContent.NPCType<LilBunbun>(), NPC.whoAmI);
            }
        }

    }
}
