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

namespace TheBattleCats.Content.NPCs.NyandamBoss
{
    [AutoloadBossHead]
    public class Nyandam : ModNPC
    {
        private enum ActionState
        {
            Idle,
            Attack1,
            Attack2,
            Attack3,
            Attack4,
            Attack5,
            Transform,
            EnhancedAttack1,
            EnhancedAttack2,
            EnhancedAttack3,
            EnhancedAttack4,
            EnhancedAttack5
        }

        public ref float AIState => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];

        public static Asset<Texture2D> IdleTexture;
        public static Asset<Texture2D> HandForwardTexture;
        public static Asset<Texture2D> HandUpTexture;
        public static Asset<Texture2D> HandSlamTexture;
        public static Asset<Texture2D> TransformTexture;

        private Dictionary<ActionState, int> TotalFrames = new()
        {
            { ActionState.Idle, 22 },
            { ActionState.Attack1, 40 },
            { ActionState.Attack2, 40 },
            { ActionState.Attack3, 40 },
            { ActionState.Attack4, 30 },
            { ActionState.Attack5, 40 },
            { ActionState.Transform, 22 }, //48 for the actual transform
            { ActionState.EnhancedAttack1, 40 },
            { ActionState.EnhancedAttack2, 40 },
            { ActionState.EnhancedAttack3, 40 },
            { ActionState.EnhancedAttack4, 30 },
            { ActionState.EnhancedAttack5, 40 }
        };

        private Dictionary<ActionState, int> FrameSpeeds = new()
        {
            { ActionState.Idle, 4 },
            { ActionState.Attack1, 4 },
            { ActionState.Attack2, 4 },
            { ActionState.Attack3, 4 },
            { ActionState.Attack4, 2 },
            { ActionState.Attack5, 4 },
            { ActionState.Transform, 4 },
            { ActionState.EnhancedAttack1, 4 },
            { ActionState.EnhancedAttack2, 4 },
            { ActionState.EnhancedAttack3, 4 },
            { ActionState.EnhancedAttack4, 2 },
            { ActionState.EnhancedAttack5, 4 }
        };

        private int CurrentFrame = 0;
        private int FrameCounter = 0;
        private int Attack3LoopCount = 0;
        private ActionState PreviousState = ActionState.Idle;
        public bool IsPhase2 => NPC.life <= NPC.lifeMax / 2;
        private bool HasTransitioned = false;
        private bool PendingTransform = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1; // not used, but must be set
        }

        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 200;
            NPC.damage = 80;
            NPC.knockBackResist = 0f;
            NPC.defense = 30;
            NPC.lifeMax = 20000;
            NPC.value = 10000f;
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.friendly = false;
            NPC.lavaImmune = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/NyandamBossfightTheme");
        }

        public override void AI()
        {   
            
            //immunity so player doesnt get hit when spawning boss in
            if (NPC.ai[3] > 0)
            {
                NPC.ai[3]--;
                NPC.dontTakeDamage = true;
                NPC.immortal = true;
                return;
            }

            NPC.dontTakeDamage = false;
            NPC.immortal = false;


            Lighting.AddLight(NPC.Center, 1f, 0.4f, 0f);
            
            //idk y i need this but the boss wasnt despawning
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
                CurrentFrame = 0;
                FrameCounter = 0;
                PreviousState = CurrentState;
            }

            int CurrentFrameSpeed = FrameSpeeds[CurrentState];

            FrameCounter++;
            if (FrameCounter >= CurrentFrameSpeed)
            {
                FrameCounter = 0;
                CurrentFrame++;
                if (CurrentFrame >= TotalFrames[CurrentState])
                {
                    CurrentFrame = 0;
                }
            }

            if (IsPhase2 && !HasTransitioned && !PendingTransform)
            {
                PendingTransform = true; // just flag it, don't interrupt
            }
            
            switch (AIState)
            {
                case (float)ActionState.Idle:
                    Idle();
                    break;
                case (float)ActionState.Attack1:
                    Attack1();
                    break;
                case (float)ActionState.Attack2:
                    Attack2();
                    break;
                case (float)ActionState.Attack3:
                    Attack3();
                    break;
                case (float)ActionState.Attack4:
                    Attack4();
                    break;
                case (float)ActionState.Attack5:
                    Attack5();
                    break;
                case (float)ActionState.Transform:
                    Transform();
                    break;
                case (float)ActionState.EnhancedAttack1:
                    EnhancedAttack1();
                    break;
                case (float)ActionState.EnhancedAttack2:
                    EnhancedAttack2();
                    break;
                case (float)ActionState.EnhancedAttack3:
                    EnhancedAttack3();
                    break;
                case (float)ActionState.EnhancedAttack4:
                    EnhancedAttack4();
                    break;
                case (float)ActionState.EnhancedAttack5:
                    EnhancedAttack5();
                    break;

            }
        }

        public override bool PreDraw(SpriteBatch SpriteBatch, Vector2 ScreenPos, Color DrawColor)
        {
            Texture2D Texture = IdleTexture.Value;
            ActionState State = (ActionState)AIState;

            switch (State)
            {
                case ActionState.Idle:
                    Texture = IdleTexture.Value;
                    break;
                case ActionState.Attack1:
                    Texture = HandForwardTexture.Value;
                    break;
                case ActionState.Attack2:
                    Texture = HandForwardTexture.Value;
                    break;
                case ActionState.Attack3:
                    Texture = HandUpTexture.Value;
                    break;
                case ActionState.Attack4:
                    Texture = HandSlamTexture.Value;
                    break;
                case ActionState.Attack5:
                    Texture = HandForwardTexture.Value;
                    break;
                case ActionState.Transform:
                    Texture = IdleTexture.Value;
                    break;
                case ActionState.EnhancedAttack1:
                    Texture = HandForwardTexture.Value;
                    break;
                case ActionState.EnhancedAttack2:
                    Texture = HandForwardTexture.Value;
                    break;
                case ActionState.EnhancedAttack3:
                    Texture = HandUpTexture.Value;
                    break;
                case ActionState.EnhancedAttack4:
                    Texture = HandSlamTexture.Value;
                    break;
                case ActionState.EnhancedAttack5:
                    Texture = HandForwardTexture.Value;
                    break;
            }

            int FrameHeight = Texture.Height / TotalFrames[State];
            Rectangle SourceRectangle = new Rectangle(0, CurrentFrame * FrameHeight, Texture.Width, FrameHeight);

            SpriteBatch.Draw(
                Texture,
                NPC.Center - ScreenPos - new Vector2(0f, 36f),
                SourceRectangle,
                DrawColor,
                NPC.rotation,
                new Vector2(Texture.Width / 2f, FrameHeight / 2f),
                NPC.scale,
                NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f
            );

            return false;
        }

        public override void Load()
        {
            IdleTexture = ModContent.Request<Texture2D>("TheBattleCats/Content/NPCs/NyandamBoss/Nyandam");
            HandForwardTexture = ModContent.Request<Texture2D>("TheBattleCats/Content/NPCs/NyandamBoss/HandForward");
            HandUpTexture = ModContent.Request<Texture2D>("TheBattleCats/Content/NPCs/NyandamBoss/HandUp");
            HandSlamTexture = ModContent.Request<Texture2D>("TheBattleCats/Content/NPCs/NyandamBoss/HandSlam");
            TransformTexture = ModContent.Request<Texture2D>("TheBattleCats/Content/NPCs/NyandamBoss/Transform");
        }


        private ActionState LastAttack = ActionState.Attack5;


        private void Idle()
        { 
            AITimer++;

            if (AITimer >= 88)
            {
                AITimer = 0;

                switch (LastAttack)
                {
                    case ActionState.Attack1:
                        AIState = (float)ActionState.Attack2;
                        break;
                    case ActionState.Attack2:
                        AIState = (float)ActionState.Attack3;
                        break;
                    case ActionState.Attack3:
                        AIState = (float)ActionState.Attack4;
                        break;
                    case ActionState.Attack4:
                        AIState = (float)ActionState.Attack5;
                        break;
                    case ActionState.Attack5:
                        AIState = (float)ActionState.Attack1;
                        break;
                }

                NPC.netUpdate = true;
            }
        }
        
        
        private float Attack1Increment;
        private float AngleIncrement = 0f;
        private float AngleIncrementP2;
        private float RandomSpawnLocation;
        
        private void Attack1()
        {
            AITimer++;

            if (AITimer >= 160)
            {
                RandomSpawnLocation = Main.rand.NextFloat(0f, 360f);
                Attack1Increment += 1;
                if (Attack1Increment >= 3)
                {
                    
                    if (PendingTransform) // CHECK HERE
                    {   
                        AIState = (float)ActionState.Transform;
                        PendingTransform = false;
                        HasTransitioned = true;
                    }
                    else
                    {
                        LastAttack = ActionState.Attack1;
                        AIState = (float)ActionState.Idle;
                    }
                    Attack1Increment = 0;
                }
                AITimer = 0;
                NPC.netUpdate = true; // sync with clients in multiplayer
                AngleIncrement += 15;
                if (AngleIncrement > 150)
                {
                    AngleIncrement = 0;
                }

            }
                if ((AITimer % 4) == 1 && AITimer < 94)
                {
                    DoAttack1();

                }
            



        }

        private void DoAttack1()
        {   
            
            float angleDegrees = AITimer * 3.75f + AngleIncrement; 
            float angleRadians = MathHelper.ToRadians(angleDegrees);

            Vector2 angleVector = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));

            float radius = NPC.width * 0.5f + 20f; // spawn just outside boss's hitbox

            float speed = 0f;

            Vector2 velocity = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians)) * speed;
            Vector2 spawnPosition = NPC.Center + angleVector * radius;


            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<NyandamAttack1Projectile>(),
                30,
                1f,
                Main.myPlayer,
                AITimer, // Pass the aiTimer for release logic
                angleRadians,
                NPC.whoAmI
            );
        }


        




private float Attack2Increment;
private void Attack2()
        {
            AITimer++;

            if (AITimer >= 160)
            {
                RandomSpawnLocation = Main.rand.NextFloat(0f, 360f);
                Attack2Increment += 1;
                if (Attack2Increment >= 3)
                {
                    
                    if (PendingTransform) // CHECK HERE
                    {   
                        AIState = (float)ActionState.Transform;
                        PendingTransform = false;
                        HasTransitioned = true;
                    }
                    else
                    {
                        LastAttack = ActionState.Attack2;
                        AIState = (float)ActionState.Idle;
                    }
                    Attack2Increment = 0;
                }
                AITimer = 0;
                NPC.netUpdate = true; // sync with clients in multiplayer

                AngleIncrementP2 += 60;
                if (AngleIncrementP2 > 305)
                {
                    AngleIncrementP2 = 0;
                }

            }

                if ((AITimer % 3) == 1 && AITimer < 120)
                {   
                    
                    DoAttack2();

                }
            

        }

        private void DoAttack2()
        {   
            
            float angleDegrees = AITimer * 3.8f + AngleIncrementP2 + RandomSpawnLocation; 
            float angleRadians = MathHelper.ToRadians(angleDegrees);

            Vector2 angleVector = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));

            float radius = NPC.width * 0.5f + 20f; // spawn just outside boss's hitbox

            float speed = 0f;

            Vector2 velocity = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians)) * speed;
            Vector2 spawnPosition = NPC.Center + angleVector * radius;


            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<NyandamAttack2Projectile>(),
                30,
                1f,
                Main.myPlayer,
                AITimer, // Pass the aiTimer for release logic
                angleRadians,
                NPC.whoAmI
            );
        }     
      
      



        private void Attack3()
        {
            AITimer++;

            if (Attack3LoopCount < 5)
            {
                if (CurrentFrame >= 30)
                {
                    Attack3LoopCount++;
                    CurrentFrame = 15;
                    FrameCounter = 0;
                }
            }


            if (AITimer >= (25 + (15 * 6)) * 4) //460
            {   

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.type == ModContent.ProjectileType<NyandamAttack3Projectile>())
                    {
                        p.ai[0] = 1f; // signal to start fading
                        p.hostile = false; // so they can't damage during fadeout
                    }
                }
                AITimer = 0;

                if (PendingTransform) 
                {
                    AIState = (float)ActionState.Transform;
                    PendingTransform = false;
                    HasTransitioned = true;
                }
                else
                {
                    LastAttack = ActionState.Attack3;
                    AIState = (float)ActionState.Idle;
                }



                FrameCounter = 0;
                NPC.netUpdate = true;
                Attack3LoopCount = 0;
            }

            if ((AITimer % 120 ) == 1 ) // Every 2 seconds
            {
                DoAttack3();

            }


        }

        private void DoAttack3()
        {
            int bulletCount = 10;
                float spiralSpread = MathHelper.TwoPi;

                for (int i = 0; i < bulletCount; i++)
                {
                    float angle = i / (float)bulletCount * spiralSpread;
                    Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        dir, // Initial direction (doesn't matter much for our logic)
                        ModContent.ProjectileType<NyandamAttack3Projectile>(),
                        30, 1f, NPC.target
                        
                    );
                }
        }

        private float Attack4LoopCount;
        private float Attack4Timer;
        private bool Attack4LinesSpawned;

        private int Attack4Part1Random;

        private void Attack4()
        {
            AITimer++;

            if (AITimer == 1)
            {   
                int Attack4Part1direction = Main.rand.NextBool() ? 1 : -1;
                Attack4Part1Random = Attack4Part1direction * Main.rand.Next(3, 6) * 16;
                Attack4LinesSpawned = false;
                SpawnTelegraphLines();
            }

            if (AITimer == 40 && !Attack4LinesSpawned)
            {
                SpawnLaserProjectiles();
                Attack4LinesSpawned = true;
            }

            if (CurrentFrame >= TotalFrames[ActionState.Attack4] - 1)
            {
                Attack4LoopCount++;
                AITimer = 0;
                CurrentFrame = 0;
                FrameCounter = 0;

                if (Attack4LoopCount >= 6)
                {
                    Attack4LoopCount = 0;
                    if (PendingTransform) 
                    {
                        AIState = (float)ActionState.Transform;
                        PendingTransform = false;
                        HasTransitioned = true;
                    }
                    else
                    {
                        LastAttack = ActionState.Attack4;
                        AIState = (float)ActionState.Idle;
                    }
                    NPC.netUpdate = true;
                }
            }
        }

        // private void Attack4() FOR IDLE BETWEEN ATTACKS 
        // {
        //     AITimer++;

        //     // Phase 1: spawn telegraph lines at start of each cycle
        //     if (AITimer == 1)
        //     {   
        //         Attack4Part1Random = Main.rand.Next(-2, 5) * 16;
        //         Attack4LinesSpawned = false;
        //         SpawnTelegraphLines();
        //     }

        //     // Phase 2: spawn real projectiles after telegraph duration
        //     if (AITimer == 61 && !Attack4LinesSpawned) // 60 tick telegraph
        //     {
        //         SpawnLaserProjectiles();
        //         Attack4LinesSpawned = true;
        //     }

        //     // Wait for animation to finish then pause
        //     if (CurrentFrame >= TotalFrames[ActionState.Attack4] - 1)
        //     {
        //         Attack4Timer++;
        //         CurrentFrame = TotalFrames[ActionState.Attack4] - 1;
        //         FrameCounter = 0;

        //         if (Attack4Timer >= 40)
        //         {
        //             Attack4LoopCount++;
        //             AITimer = 0;
        //             Attack4Timer = 0;
        //             CurrentFrame = 0;
        //             FrameCounter = 0;

        //             if (Attack4LoopCount >= 3)
        //             {
        //                 Attack4LoopCount = 0;
        //                 AIState = (float)ActionState.Idle;
        //                 NPC.netUpdate = true;
        //             }
        //         }
        //     }
        // }

        private void SpawnTelegraphLines()
        {
            int lineCount = 24;
            int lineSpacing = 5 * 16;

            for (int i = 0; i < lineCount; i++)
            {
                int x = (int)(NPC.Center.X + Attack4Part1Random - lineSpacing * (lineCount / 2) + i * lineSpacing);
                Vector2 telePos = new Vector2(x, NPC.Center.Y - 550);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    telePos,
                    Vector2.Zero,
                    ModContent.ProjectileType<TelegraphLine>(),
                    0, 0, Main.myPlayer
                );
            }
        }

        private void SpawnLaserProjectiles()
        {
            int lineCount = 24;
            int lineSpacing = 5 * 16;
            int projSpeed = -30;
            int projDamage = 20;

            for (int i = 0; i < lineCount; i++)
            {
                int x = (int)(NPC.Center.X + Attack4Part1Random- lineSpacing * (lineCount / 2) + i * lineSpacing);
                Vector2 projPos = new Vector2(x, NPC.Center.Y + 500);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    projPos,
                    new Vector2(0, projSpeed),
                    ModContent.ProjectileType<LaserProjectile>(),
                    projDamage, 2f, Main.myPlayer
                );
            }
        }


        private Vector2[] PortalPositions = new Vector2[8]; // store portal positions
        private int Attack5WaveCount = 0;
        private int Attack5SkippedIndex = 4; // start in the middle
      
        private void Attack5()
        {
            AITimer++;

            if (AITimer == 1)
            {
                int min = Math.Max(0, Attack5SkippedIndex - 4);
                int max = Math.Min(7, Attack5SkippedIndex + 4);
                Attack5SkippedIndex = Main.rand.Next(min, max + 1);
            }


            if (AITimer == 10)
            {
                int portalWidth = 10 * 16;
                int gapWidth = 0 * 16;
                int totalWidth = 8 * (portalWidth + gapWidth);
                int startX = (int)(NPC.Center.X - totalWidth / 2);

                for (int i = 0; i < 8; i++)
                {
                    float x = startX + i * (portalWidth + gapWidth) + portalWidth / 2;
                    float y = NPC.Center.Y + 20 * 16;
                    PortalPositions[i] = new Vector2(x, y);

                    if (i == Attack5SkippedIndex) continue;

                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        PortalPositions[i],
                        Vector2.Zero,
                        ModContent.ProjectileType<NyandamPortal>(),
                        0, 0, Main.myPlayer
                    );
                }
            }

            if (AITimer == 140)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (i == Attack5SkippedIndex) continue;

                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        PortalPositions[i],
                        new Vector2(0, -10),
                        ModContent.ProjectileType<NyandamAttack5Projectile>(),
                        40, 1f, Main.myPlayer
                    );
                }
            }

            if (AITimer >= 160)
            {
                AITimer = 0;
                Attack5WaveCount++;

                if (Attack5WaveCount >= 5)
                {
                    Attack5WaveCount = 0;
                    if (PendingTransform)
                    {
                        AIState = (float)ActionState.Transform;
                        PendingTransform = false;
                        HasTransitioned = true;
                    }
                    else
                    {
                        LastAttack = ActionState.Attack5;
                        AIState = (float)ActionState.Idle;
                    }
                }

                NPC.netUpdate = true;
            }
        }


        // private void DoAttack1Part2()
        // {   
            
        //     float angleDegrees = AITimer * 3.8f + AngleIncrementP2 + RandomSpawnLocation; 
        //     float angleRadians = MathHelper.ToRadians(angleDegrees);

        //     Vector2 angleVector = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));

        //     float radius = NPC.width * 0.5f + 20f; // spawn just outside boss's hitbox

        //     float speed = 0f;

        //     Vector2 velocity = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians)) * speed;
        //     Vector2 spawnPosition = NPC.Center + angleVector * radius;


        //     Projectile.NewProjectile(
        //         NPC.GetSource_FromAI(),
        //         spawnPosition,
        //         velocity,
        //         ModContent.ProjectileType<NyandamAttack1Projectile2>(),
        //         60,
        //         1f,
        //         Main.myPlayer,
        //         AITimer, // Pass the aiTimer for release logic
        //         angleRadians,
        //         NPC.whoAmI
        //     );
        // }




        //--------------PHASE 2-------------------//

        private ActionState EnhancedLastAttack = ActionState.EnhancedAttack5;

        private void Transform()
        { 
            AITimer++;

            if (AITimer >= 88)
            {
                AITimer = 0;

                switch (EnhancedLastAttack)
                {
                    case ActionState.EnhancedAttack1:
                        AIState = (float)ActionState.EnhancedAttack2;
                        break;
                    case ActionState.EnhancedAttack2:
                        AIState = (float)ActionState.EnhancedAttack3;
                        break;
                    case ActionState.EnhancedAttack3:
                        AIState = (float)ActionState.EnhancedAttack4;
                        break;
                    case ActionState.EnhancedAttack4:
                        AIState = (float)ActionState.EnhancedAttack5;
                        break;
                    case ActionState.EnhancedAttack5:
                        AIState = (float)ActionState.EnhancedAttack1;
                        break;
                }

                NPC.netUpdate = true;
            }
        }


        private void EnhancedAttack1()
        {
            AITimer++;

            if (AITimer >= 160)
            {
                //STILL USING THE SAME INCREMENT AS ATTACK 1 SO IT MIGHT CAUSE SOME PROBLEMS BUT
                // IF THERE ARE U CAN JUST MAKE ENHANCEDATTACK1INCREMENT
                Attack1Increment += 1;
                if (Attack1Increment >= 3)
                {
                    EnhancedLastAttack = ActionState.EnhancedAttack1;
                    AIState = (float)ActionState.Transform;

                    Attack1Increment = 0;
                }
                AITimer = 0;
                NPC.netUpdate = true; // sync with clients in multiplayer
                AngleIncrement += 15;
                if (AngleIncrement > 150)
                {
                    AngleIncrement = 0;
                }
                AngleIncrementP2 += 60;
                if (AngleIncrementP2 > 305)
                {
                    AngleIncrementP2 = 0;
                }

            }


            if ((AITimer % 3) == 1 && AITimer < 120)
            {
                DoEnhancedAttack1();

            }

        }

        private void DoEnhancedAttack1()
        {   
            float randomspawn = Main.rand.NextFloat(0f, 360f);
            float angleDegrees = AITimer * 3.8f + AngleIncrementP2 +randomspawn; 
            float angleRadians = MathHelper.ToRadians(angleDegrees);

            Vector2 angleVector = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));

            float radius = NPC.width * 0.5f + 20f; // spawn just outside boss's hitbox

            float speed = 0f;

            Vector2 velocity = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians)) * speed;
            Vector2 spawnPosition = NPC.Center + angleVector * radius;


            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<NyandamAttack1Projectile>(),
                30,
                1f,
                Main.myPlayer,
                AITimer, // Pass the aiTimer for release logic
                angleRadians,
                NPC.whoAmI
            );
        }




private float EnhancedAttack2Increment;

private bool EnhancedAttack2P2 = false;
private void EnhancedAttack2()
        {
            AITimer++;


            if (CurrentFrame > 8 && CurrentFrame < 20 && EnhancedAttack2P2 == true)
            {
                
                CurrentFrame = 20;
                AITimer += 44;
            }

            

            
            if (AITimer >= 160)
            {
                RandomSpawnLocation = Main.rand.NextFloat(0f, 360f);
                EnhancedAttack2Increment += 1;
                if (EnhancedAttack2Increment >= 3)
                {
        
                    EnhancedLastAttack = ActionState.EnhancedAttack2;
                    AIState = (float)ActionState.Transform;

                    EnhancedAttack2Increment = 0;
                }
                AITimer = 0;

                if (EnhancedAttack2P2 == true)
                {
                  EnhancedAttack2P2 = false ; 
                }
                else
                {
                    EnhancedAttack2P2 = true;
                }

                NPC.netUpdate = true; // sync with clients in multiplayer

                AngleIncrementP2 += 60;
                if (AngleIncrementP2 > 305)
                {
                    AngleIncrementP2 = 0;
                }

            }

            

                if ((AITimer % 3) == 1 && AITimer < 120 && EnhancedAttack2P2 == false)
                {   
                    
                    DoAttack2();
                    DoEnhancedAttack2();
                }
            

        }

        private void DoEnhancedAttack2()
        {   
            
            float angleDegrees = AITimer * 3.8f + AngleIncrementP2; 
            float angleRadians = MathHelper.ToRadians(angleDegrees);

            Vector2 angleVector = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));

            float radius = NPC.width * 0.5f; // spawn just outside boss's hitbox

            float speed = 0f;

            Vector2 velocity = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians)) * speed;
            Vector2 spawnPosition = NPC.Center + angleVector * radius;


            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                spawnPosition,
                velocity,
                ModContent.ProjectileType<NyandamEnhancedAttack2Projectile>(),
                30,
                1f,
                Main.myPlayer,
                AITimer, // Pass the aiTimer for release logic
                angleRadians,
                NPC.whoAmI
            );
        }     
      





        private void EnhancedAttack3()
        {
            AITimer++;

            if (Attack3LoopCount < 5)
            {
                if (CurrentFrame >= 30)
                {
                    Attack3LoopCount++;
                    CurrentFrame = 15;
                    FrameCounter = 0;
                }
            }


            if (AITimer >= (25 + (15 * 6)) * 4) //460
            {   

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.type == ModContent.ProjectileType<NyandamAttack3Projectile>())
                    {
                        p.ai[0] = 1f; // signal to start fading
                        p.hostile = false; // so they can't damage during fadeout
                    }
                }
                AITimer = 0;


                EnhancedLastAttack = ActionState.EnhancedAttack3;
                AIState = (float)ActionState.Transform;
    



                FrameCounter = 0;
                NPC.netUpdate = true;
                Attack3LoopCount = 0;
            }

            if ((AITimer % 120 ) == 1 ) // Every 2 seconds
            {
                DoAttack3();

            }
            if ((AITimer % 10) == 1) // tweak frequency to taste
            {
                SpawnEnhancedAttack3();
            }


        }

private void SpawnEnhancedAttack3()
{
    float boxHalfWidth = (BoxWidth / 2f) * 16f;
    float boxHalfHeight = (BoxHeight / 2f) * 16f;
    float yOffset = -9 * 16f; // same offset as OnSpawn

    float y = NPC.Center.Y + yOffset + Main.rand.NextFloat(-boxHalfHeight, boxHalfHeight);

    bool fromLeft = Main.rand.NextBool();

    Vector2 spawnPos = new Vector2(
        NPC.Center.X + (fromLeft ? -boxHalfWidth : boxHalfWidth),
        y
    );

    Vector2 velocity = new Vector2(fromLeft ? 10f : -10f, 0f);

    Projectile.NewProjectile(
        NPC.GetSource_FromAI(),
        spawnPos,
        velocity,
        ModContent.ProjectileType<NyandamEnhancedAttack3Projectile>(),
        30, 2f, Main.myPlayer
    );
}

        private void EnhancedAttack4()
        {
            AITimer++;

            if (AITimer == 1)
            {   
                int Attack4Part1direction = Main.rand.NextBool() ? 1 : -1;
                Attack4Part1Random = Attack4Part1direction * Main.rand.Next(3, 6) * 16;
                Attack4LinesSpawned = false;
                SpawnTelegraphLines();
                SpawnEnhancedTelegraphLines();
            }

            if (AITimer == 40 && !Attack4LinesSpawned)
            {
                SpawnLaserProjectiles();
                SpawnEnhancedLaserProjectiles();
                Attack4LinesSpawned = true;
            }

            if (CurrentFrame >= TotalFrames[ActionState.Attack4] - 1)
            {
                Attack4LoopCount++;
                AITimer = 0;
                CurrentFrame = 0;
                FrameCounter = 0;

                if (Attack4LoopCount >= 6)
                {
                    Attack4LoopCount = 0;

                    EnhancedLastAttack = ActionState.EnhancedAttack4;
                    AIState = (float)ActionState.Transform;
                
                    NPC.netUpdate = true;
                }
            }
        }
        private void SpawnEnhancedTelegraphLines()
        {
            int lineCount = 24;
            int lineSpacing = 5 * 16;

            for (int i = 0; i < lineCount; i++)
            {
                int y = (int)(NPC.Center.Y + Attack4Part1Random - lineSpacing * (lineCount / 2) + i * lineSpacing);
                Vector2 telePos = new Vector2(NPC.Center.X, y);
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    telePos,
                    Vector2.Zero,
                    ModContent.ProjectileType<EnhancedTelegraphLine>(), // CHANGED
                    0, 0, Main.myPlayer
                );
            }
        }

        private void SpawnEnhancedLaserProjectiles()
        {
            int lineCount = 24;
            int lineSpacing = 5 * 16;
            int projDamage = 20;

            for (int i = 0; i < lineCount; i++)
            {
                int y = (int)(NPC.Center.Y + Attack4Part1Random - lineSpacing * (lineCount / 2) + i * lineSpacing);
                Vector2 projPos = new Vector2(NPC.Center.X - 1200, y); // spawn off screen left
                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    projPos,
                    new Vector2(30, 0), // moves right
                    ModContent.ProjectileType<EnhancedLaserProjectile>(), // CHANGED
                    projDamage, 2f, Main.myPlayer
                );
            }
        }
        

private void EnhancedAttack5()
{
    AITimer++;

    if (CurrentFrame > 8 && CurrentFrame < 20)
    {
        CurrentFrame = 20;
        AITimer += 48;
    }

    if (AITimer == 1)
    {
        int min = Math.Max(0, Attack5SkippedIndex - 4);
        int max = Math.Min(7, Attack5SkippedIndex + 4);
        Attack5SkippedIndex = Main.rand.Next(min, max + 1);
    }

    if (AITimer == 10)
    {
        int portalWidth = 10 * 16;
        int gapWidth = 0 * 16;
        int totalWidth = 8 * (portalWidth + gapWidth);
        int startX = (int)(NPC.Center.X - totalWidth / 2);

        for (int i = 0; i < 8; i++)
        {
            float x = startX + i * (portalWidth + gapWidth) + portalWidth / 2;
            float y = NPC.Center.Y + 20 * 16;
            PortalPositions[i] = new Vector2(x, y);

            if (i == Attack5SkippedIndex) continue;

            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                PortalPositions[i],
                Vector2.Zero,
                ModContent.ProjectileType<EnhancedNyandamPortal>(),
                0, 0, Main.myPlayer
            );
        }
    }

    if (AITimer == 140)
    {
        for (int i = 0; i < 8; i++)
        {
            if (i == Attack5SkippedIndex) continue;

            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                PortalPositions[i],
                new Vector2(0, -10),
                ModContent.ProjectileType<NyandamAttack5Projectile>(),
                40, 1f, Main.myPlayer
            );
        }
    }

    if (AITimer >= 160)
    {
        AITimer = 0;
        Attack5WaveCount++;

        if (Attack5WaveCount >= 5)
        {
            Attack5WaveCount = 0;

            EnhancedLastAttack = ActionState.EnhancedAttack5;
            AIState = (float)ActionState.Transform;

        }

        NPC.netUpdate = true;
    }
}

        private const int BoxWidth = 80;
        private const int BoxHeight = 60;

        public override void OnSpawn(IEntitySource Source)
        {

            SoundEngine.PlaySound(new SoundStyle("TheBattleCats/Assets/Effects/BossShockwave"), NPC.Center);


            int CenterX = (int)(NPC.Center.X / 16);
            int CenterY = (int)(NPC.Center.Y / 16 - 9); //- BoxHeight / 2 + 8;

            for (int i = -BoxWidth / 2; i <= BoxWidth / 2; i++)
            {
                for (int j = -BoxHeight / 2; j <= BoxHeight / 2; j++)
                {
                    bool IsEdge = (i == -BoxWidth / 2 || i == BoxWidth / 2 || j == -BoxHeight / 2 || j == BoxHeight / 2);
                    if (IsEdge)
                    {
                        int X = CenterX + i;
                        int Y = CenterY + j;

                        WorldGen.PlaceTile(X, Y, ModContent.TileType<NyandamBlock>(), true, true);
                    }
                }
            }




        }


        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
{
    Player player = Main.player[Main.myPlayer];
    
    int CenterX = (int)(NPC.Center.X / 16);
    int CenterY = (int)(NPC.Center.Y / 16 - 9);
    
    // Box bounds in tile coordinates
    int left   = CenterX - BoxWidth / 2;
    int right  = CenterX + BoxWidth / 2;
    int top    = CenterY - BoxHeight / 2;
    int bottom = CenterY + BoxHeight / 2;
    
    // Player position in tile coordinates
    int playerTileX = (int)(player.Center.X / 16);
    int playerTileY = (int)(player.Center.Y / 16);
    
    bool insideBox = playerTileX > left && playerTileX < right &&
                     playerTileY > top  && playerTileY < bottom;
    
    if (!insideBox)
    {
        modifiers.SetMaxDamage(0); // no damage if outside
    }
}




    }
}









//FIRE ATTACK IF U WANNA USE NEXT TIME FOR PART 2

        // private void Attack1()
        // {
        //     AITimer++;

        //     if (AITimer >= 160)
        //     {
        //         Attack1Increment += 1;
        //         if (Attack1Increment > 5)
        //         {
                    
        //         AIState = (float)ActionState.Attack2;

        //             Attack1Increment = 0;
        //         }
        //         AITimer = 0;
        //         NPC.netUpdate = true; // sync with clients in multiplayer
        //         AngleIncrement += 15;
        //         if (AngleIncrement > 150)
        //         {
        //             AngleIncrement = 0;
        //         }
        //         AngleIncrementP2 += 60;
        //         if (AngleIncrementP2 > 305)
        //         {
        //             AngleIncrementP2 = 0;
        //         }

        //     }
        //     if (Attack1Increment < 2)
        //     {
        //         if ((AITimer % 4) == 1 && AITimer < 94)
        //         {
        //             DoAttack1Part1();

        //         }
        //     }

        //     if (Attack1Increment > 1)
        //     {
        //         if ((AITimer % 3) == 1 && AITimer < 120)
        //         {
        //             DoAttack1Part2();

        //         }
        //     }

        // }

        // private void DoAttack1Part1()
        // {   
            
        //     float angleDegrees = AITimer * 3.75f + AngleIncrement; 
        //     float angleRadians = MathHelper.ToRadians(angleDegrees);

        //     Vector2 angleVector = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));

        //     float radius = NPC.width * 0.5f + 20f; // spawn just outside boss's hitbox

        //     float speed = 0f;

        //     Vector2 velocity = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians)) * speed;
        //     Vector2 spawnPosition = NPC.Center + angleVector * radius;


        //     Projectile.NewProjectile(
        //         NPC.GetSource_FromAI(),
        //         spawnPosition,
        //         velocity,
        //         ModContent.ProjectileType<NyandamAttack1Projectile1>(),
        //         20,
        //         1f,
        //         Main.myPlayer,
        //         AITimer, // Pass the aiTimer for release logic
        //         angleRadians,
        //         NPC.whoAmI
        //     );
        // }

        // private void DoAttack1Part2()
        // {   
        //     float randomspawn = Main.rand.NextFloat(0f, 360f);
        //     float angleDegrees = AITimer * 3.8f + AngleIncrementP2 +randomspawn; 
        //     float angleRadians = MathHelper.ToRadians(angleDegrees);

        //     Vector2 angleVector = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));

        //     float radius = NPC.width * 0.5f + 20f; // spawn just outside boss's hitbox

        //     float speed = 0f;

        //     Vector2 velocity = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians)) * speed;
        //     Vector2 spawnPosition = NPC.Center + angleVector * radius;


        //     Projectile.NewProjectile(
        //         NPC.GetSource_FromAI(),
        //         spawnPosition,
        //         velocity,
        //         ModContent.ProjectileType<NyandamAttack1Projectile2>(),
        //         20,
        //         1f,
        //         Main.myPlayer,
        //         AITimer, // Pass the aiTimer for release logic
        //         angleRadians,
        //         NPC.whoAmI
        //     );
        // }





        //OLD TRANSFORM CODE FOR THE ACTUAL TRANSFORMING 


        // private void Transform()
        // {   
        //     NPC.dontTakeDamage = true;
        //     AITimer++;

        //     if (AITimer >= 192)
            
        //     {   
        //         NPC.dontTakeDamage = false;
        //         AITimer = 0;
        //         AIState = (float)ActionState.EnhancedAttack1;
        //         NPC.netUpdate = true; // sync with clients in multiplayer
        //     }
        // }




        //OLD ATTACK 2 CODE 

        
// private void Attack2()
        // {
        //     AITimer++;

        //     if (AITimer >= 375)
        //     {
        //         AITimer = 0;
                
        //         if (PendingTransform) 
        //         {
        //             AIState = (float)ActionState.Transform;
        //             PendingTransform = false;
        //             HasTransitioned = true;
        //         }
        //         else
        //         {
        //             AIState = (float)ActionState.Attack3;
        //         }

        //         NPC.netUpdate = true;
        //     }

        //     if (CurrentFrame > 8 && CurrentFrame < 30)
        //     {
        //         CurrentFrame = 30;
        //     }

        //     if (CurrentFrame == 0 && FrameCounter == 0)
        //     {
        //         DoAttack2();
        //     }
        // }
        // private void DoAttack2()
        // {
        //     int projType = ModContent.ProjectileType<NyandamAttack2>();
        //     Player player = Main.player[NPC.target]; // Get the player targeted by the NPC

        //     // Randomly choose left (-1) or right (1)
        //     int direction = Main.rand.NextBool() ? -1 : 1;

        //     // Randomize the horizontal offset
        //     float xOffset = direction * 33 * 16;
        //     // float yOffset = Main.rand.Next(-45*16, 2*16); 
        //     float yOffset = player.Center.Y - NPC.Center.Y;


        //     float minYOffset = -100 * 16;
        //     float maxYOffset = 100 * 16;

        //     // Check if verticalOffset is within the range
        //     if (yOffset < minYOffset)
        //     {
        //         yOffset = minYOffset; // If below the min range, set to min limit
        //     }
        //     else if (yOffset > maxYOffset)
        //     {
        //         yOffset = maxYOffset; // If above the max range, set to max limit
        //     }

        //     Vector2 spawnPosition = NPC.Center + new Vector2(xOffset, yOffset);

        //     // Spawn the projectile
        //     Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition, Vector2.Zero, projType, 10, 20, Main.myPlayer, direction, direction);


        // }



//SOMEHOW ACCIDENTALLY MADE THE SPIRAL ATTACK OML 


// private Vector2 Attack5TargetPosition; 
// private void Attack5()
//         {
//             AITimer++;
            
//             if (AITimer == 1)
//             {
//                 Player player = Main.player[NPC.target];
//                 Attack5TargetPosition = player.Center;
//             }
//             if (AITimer >= 160)
//             {
//                 RandomSpawnLocation = Main.rand.NextFloat(0f, 360f);
//                 Attack1Increment += 1;
//                 if (Attack1Increment > 5)
//                 {
                    
//                     if (PendingTransform) // CHECK HERE
//                     {   
//                         AIState = (float)ActionState.Transform;
//                         PendingTransform = false;
//                         HasTransitioned = true;
//                     }
//                     else
//                     {
//                         LastAttack = ActionState.Attack5;
//                         AIState = (float)ActionState.Idle;
//                     }
//                     Attack1Increment = 0;
//                 }
//                 AITimer = 0;
//                 NPC.netUpdate = true; // sync with clients in multiplayer
//                 AngleIncrement += 15;
//                 if (AngleIncrement > 150)
//                 {
//                     AngleIncrement = 0;
//                 }
//                 AngleIncrementP2 += 60;
//                 if (AngleIncrementP2 > 305)
//                 {
//                     AngleIncrementP2 = 0;
//                 }

//             }

//             if ((AITimer % 3) == 1 && AITimer < 94)
//             {
//                 DoAttack5Part1();

//             }
            



//         }

//         private void DoAttack5Part1()
//         {   
        

//             float angleDegrees = AITimer * 3.75f + AngleIncrement; 
//             float angleRadians = MathHelper.ToRadians(angleDegrees);

//             Vector2 angleVector = new Vector2((float)Math.Cos(angleRadians), (float)Math.Sin(angleRadians));

//             float radius = NPC.width * 0.5f + 20f; // spawn just outside boss's hitbox

//             float speed = 0f;

//             Vector2 spawnPosition = Attack5TargetPosition + angleVector * radius;
//             Vector2 velocity = Vector2.Normalize(Attack5TargetPosition - spawnPosition) * 8f;
            
//             Projectile.NewProjectile(
//                 NPC.GetSource_FromAI(),
//                 spawnPosition,
//                 velocity,
//                 ModContent.ProjectileType<NyandamAttack5Projectile1>(),
//                 60,
//                 1f,
//                 Main.myPlayer,
//                 AITimer, // Pass the aiTimer for release logic
//                 angleRadians,
//                 NPC.whoAmI
//             );
//         }
