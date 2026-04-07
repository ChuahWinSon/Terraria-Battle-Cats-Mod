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
using Terraria.GameContent.LootSimulation.LootSimulatorConditionSetterTypes;
using TheBattleCats.Common.Graphics.Particles;
using TheBattleCats.Common.Systems; 
using Terraria.Graphics.CameraModifiers;

namespace TheBattleCats.Content.NPCs.CycloneBoss
{
    [AutoloadBossHead]
    public class Cyclone : ModNPC
    {
     
        
        private enum ActionState
        {
            Spawn,
            Reset,
            
            TripleDashAttack,
            Dash,
            CircleAndShoot,
            SansWall,
            FallingRocks,
            LingeringRocks,
            MiniCyclones,
            EnhancedAttack1,
            EnhancedAttack4,
            GroundSmash,

            FinalTurn
        }

        public ref float AIState => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];
        public ref float AttackTimer => ref NPC.ai[2];
        public ref float ExtraTimer => ref NPC.ai[3];
        

        public static int DashSpreadDamage => 10;
        

        public override void SetDefaults()
        {
            NPC.aiStyle = 0;

            NPC.damage = 40;
            NPC.defense = 12;
            NPC.lifeMax = 6000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.width = 110;
            NPC.height = 110;
            NPC.boss = true;
        }

        public bool IsPhase2 => NPC.life <= NPC.lifeMax / 2;
        private const int CloneOffset = 100; // pixels left/right, tune this
        private const int CloneAlpha  = 200; // transparency, tune this

        public const float FinalPhaseLifeRatio = 0.1f;


        private static readonly SoundStyle ProjectileSound = new SoundStyle("TheBattleCats/Assets/Boss/Cyclone/CycloneProjectile")
        {
            PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
        };

        private static readonly SoundStyle CycloneRoarGor = new SoundStyle("TheBattleCats/Assets/Boss/Cyclone/CycloneRoarGor")
        {
            PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
        };

        private static readonly SoundStyle CycloneRoarDrag = new SoundStyle("TheBattleCats/Assets/Boss/Cyclone/CycloneRoarDrag")
        {
            PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
        };

        private static readonly SoundStyle CycloneSlam = new SoundStyle("TheBattleCats/Assets/Boss/Cyclone/CycloneSlam")
        {
            PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
        };

        
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 29;
        }

        // For sprite glitches
        public override void OnKill()
        {
            roarTimer = 0;
            Main.npcFrameCount[NPC.type] = 29;
        }

        public override void OnSpawn(IEntitySource source)
        {
            roarTimer = 0;
            NPC.frame.Y = 0;
            Main.npcFrameCount[NPC.type] = 29;
        }

        public override void FindFrame(int frameHeight)
        {
            // Use the correct frame height for whichever sheet is active
            int activeFrameCount = Main.npcFrameCount[NPC.type]; // already swapped by TriggerRoar
            Texture2D activeTex  = IsRoaring ? RoarTexture.Value : TextureAssets.Npc[NPC.type].Value;
            int correctHeight    = activeTex.Height / activeFrameCount;

            NPC.frameCounter++;
            if (NPC.frameCounter >= 4)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += correctHeight;
                if (NPC.frame.Y >= correctHeight * activeFrameCount)
                    NPC.frame.Y = 0;
            }
        }

        // public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        // {
        //     Texture2D tex         = TextureAssets.Npc[NPC.type].Value;
        //     int frameCount        = Main.npcFrameCount[NPC.type];
        //     int correctFrameHeight = tex.Height / frameCount;
        //     Rectangle frame       = new Rectangle(0, NPC.frame.Y, tex.Width, correctFrameHeight);
        //     Vector2 origin        = frame.Size() / 2f;
        //     SpriteEffects flip    = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        //     Color cloneColor = drawColor * (1f - CloneAlpha / 255f);
        //     float cloneScale = NPC.scale * 0.7f; // 70% size


        //     if (IsPhase2)
        //     {
        //         // Left clone
        //         spriteBatch.Draw(tex, NPC.Center - screenPos + new Vector2(-CloneOffset, 0f),
        //         frame, cloneColor, NPC.rotation, origin, cloneScale, flip, 0f);

        //         // Right clone
        //         spriteBatch.Draw(tex, NPC.Center - screenPos + new Vector2(CloneOffset, 0f),
        //         frame, cloneColor, NPC.rotation, origin, cloneScale, flip, 0f);
        //     }

        //     // Main boss (u can use this or u can just return true)
        //     spriteBatch.Draw(tex, NPC.Center - screenPos,
        //     frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, flip, 0f);

        //     return false;
        // }


        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = IsRoaring
                ? RoarTexture.Value
                : TextureAssets.Npc[NPC.type].Value;

            int frameCount = Main.npcFrameCount[NPC.type];
            int correctFrameHeight = tex.Height / frameCount;
            Rectangle frame    = new Rectangle(0, NPC.frame.Y, tex.Width, correctFrameHeight);
            Vector2 origin     = frame.Size() / 2f;
            Vector2 pos        = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
            SpriteEffects flip = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(tex, pos, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, flip, 0f);

            return false;

        }

        public override void HitEffect(NPC.HitInfo hit) {
			// If the NPC dies, spawn gore and play a sound
			if (Main.netMode == NetmodeID.Server) {
				// We don't want Mod.Find<ModGore> to run on servers as it will crash because gores are not loaded on servers
				return;
			}

			if (NPC.life <= 0) {
				// These gores work by simply existing as a texture inside any folder which path contains "Gores/"
				int backGoreType = Mod.Find<ModGore>("CycloneBossBody_Bottom").Type;
				int frontGoreType = Mod.Find<ModGore>("CycloneBossBody_Top").Type;
                int eyeGoreType = Mod.Find<ModGore>("CycloneBossEye").Type;

				var entitySource = NPC.GetSource_Death();

				for (int i = 0; i < 2; i++) {
					Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), backGoreType);
					Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), frontGoreType);
                    
				}
                Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-3, 3), Main.rand.Next(-3, 3)), eyeGoreType);

				// This adds a screen shake (screenshake) similar to Deerclops
				SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);

                if (Main.netMode != NetmodeID.Server)
                    BossCameraSystem.TriggerShake(10f); // increase for stronger shake
			}
		}

        public static Asset<Texture2D> RoarTexture;
        private int roarTimer = 0;
        private bool IsRoaring => roarTimer > 0;
        public override void Load()
        {
            RoarTexture = ModContent.Request<Texture2D>("TheBattleCats/Content/NPCs/CycloneBoss/Cyclone_Roar");
        }
        private void TriggerRoar(int duration = 60)
        {
            roarTimer = duration;
            NPC.frame.Y = 0;
            Main.npcFrameCount[NPC.type] = 6; // however many frames your roar sheet has
        }

        public override void Unload() => RoarTexture = null;


        private float LifeRatio;
        
        private ActionState PreviousState = ActionState.Reset;
        public override void AI()
        {

            // makes movement look more fluid
            NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.04f, -MathHelper.Pi / 6f, MathHelper.Pi / 6f);
        
            if (roarTimer > 0)
            {
                roarTimer--;
                if (roarTimer == 0)
                {
                    NPC.frame.Y = 0;
                    Main.npcFrameCount[NPC.type] = 29; // back to normal
                }
            }

            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];
            
            if (!player.active || player.dead)
            {
                NPC.active = false;
                return;
            }

            LifeRatio = (float)NPC.life / NPC.lifeMax;

            ActionState CurrentState = (ActionState)AIState;

            if (CurrentState != PreviousState)
            {
                PreviousState = CurrentState;
            }

            switch (AIState)
            {
                case (float)ActionState.Reset:
                    DoBehavior_ResetAI();
                    break;
                case (float)ActionState.Spawn:
                    DoBehavior_SpawnAnimation();
                    break;
                case (float)ActionState.TripleDashAttack:
                    DoBehavior_TripleDashAttack();
                    break;
                case (float)ActionState.Dash:
                    DoBehavior_Dash();
                    break;
                case (float)ActionState.CircleAndShoot:
                    DoBehavior_CircleAndShoot();
                    break;
                case (float)ActionState.SansWall:
                    DoBehavior_SansWall();
                    break;
                case (float)ActionState.FallingRocks:
                    DoBehavior_FallingRocks();
                    break;
                case (float)ActionState.LingeringRocks:
                    DoBehavior_LingeringRocks();
                    break;
                case (float)ActionState.MiniCyclones:
                    DoBehavior_SpawnMiniCyclones(player);
                    break;
                case (float)ActionState.EnhancedAttack1:
                    EnhancedAttack1();
                    break;
                case (float)ActionState.EnhancedAttack4:
                    EnhancedAttack4();
                    break;
                case (float)ActionState.GroundSmash:
                    DoBehavior_GroundSmash();
                    break;
                case (float)ActionState.FinalTurn:
                    DoBehavior_DisappearingDash();
                    break;
                

            }


            //spliting rocks
            if (LifeRatio < 0.5 && AITimer % 100 == 99)
            {
                Vector2 velocity = new Vector2(0f, 5f) ;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,                          // position (boss center)
                        velocity,                        // velocity (adjust as needed)
                        ModContent.ProjectileType<SplittingRock>(),
                        10,                              // your damage value
                        2f,                           // your knockback value
                        Main.myPlayer
                    );
                }

            }

            //spliting rocks
            if (LifeRatio < 0.7 && AITimer % 150 == 149)
            {
                Vector2 velocity = new Vector2(0f, 5f) ;

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<ClusteredRock>(),
                    10,
                    2f,
                    Main.myPlayer
                );

            }



        }

        private bool _groundSmashHit = false;

private void DoBehavior_GroundSmash()
{
    Player player = Main.player[NPC.target];


    // ── Phase 1: reposition above player (negative timer ramp-up, same pattern as TripleDash) ──
    if (AITimer <= 0)
    {
        Vector2 targetPos   = player.Center + new Vector2(0f, -280f);
        Vector2 toTarget    = targetPos - NPC.Center;
        float   dist        = toTarget.Length();
        float   rampFrames  = -AITimer;
        float   maxSpeed    = MathHelper.Clamp(rampFrames * 0.3f, 1f, 40f);
        Vector2 ideal       = Vector2.Normalize(toTarget) * Math.Min(dist * 0.08f, maxSpeed);
        NPC.velocity        = Vector2.Lerp(NPC.velocity, ideal, 0.07f);
        NPC.spriteDirection = player.Center.X > NPC.Center.X ? 1 : -1;

        bool closeEnough = dist <= 200f && AITimer <= -120;
        bool tookTooLong = AITimer < -300;
        if (closeEnough || tookTooLong)
        {
            NPC.velocity = Vector2.Zero;
            AITimer = 1f;   // kick to wind-up
        }
        else
        {
            AITimer--;
        }
        return;
    }

    AITimer++;

    // ── Phase 2: wind-up (frames 1-40) ──
    if (AITimer <= 40f)
    {
        NPC.velocity        = Vector2.Zero;
        NPC.spriteDirection = NPC.Center.X < player.Center.X ? 1 : -1;

        if (AITimer == 10f)
        {
            SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
            TriggerRoar(40);
            if (Main.netMode != NetmodeID.Server)
                BossCameraSystem.TriggerShake(6f);
        }
        return;
    }

    // ── Phase 3: dash downward & tile-check every frame ──
    if (!_groundSmashHit)
    {
        NPC.velocity = new Vector2(0f, 30f);

        // Check the tile directly under the boss centre
        int tileX = (int)((NPC.Center.X) / 16f);
        int tileY = (int)((NPC.Bottom.Y) / 16f);      // bottom edge
        int tileYSafeGuard = ((int)((NPC.Bottom.Y) / 16f)) - 1 ;      // prevent phase through blocks

        bool hitTile = WorldGen.SolidTile(tileX, tileY) || WorldGen.SolidTile(tileX, tileYSafeGuard);
        bool timedOut = AITimer >= 160f;

        if (hitTile || timedOut)
        {
            _groundSmashHit = true;
            NPC.velocity    = Vector2.Zero;

            if (Main.netMode != NetmodeID.Server)
                BossCameraSystem.TriggerShake(14f);

            SoundEngine.PlaySound(CycloneSlam, NPC.Center);

            // ── Rock spawn: check ±25, ±50, ±75 tile offsets ──
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int[] offsets = { -75, -50, -25, 25, 50, 75 };

                foreach (int tileOffsetX in offsets)
                {
                    int checkX = tileX + tileOffsetX;

                    // Walk down from the boss Y to find the surface at this X
                    int surfaceY = tileY;
                    for (int scanY = tileY; scanY < tileY + 40; scanY++)
                    {
                        if (WorldGen.SolidTile(checkX, scanY))
                        {
                            surfaceY = scanY;
                            break;
                        }
                    }

                    if (WorldGen.SolidTile(checkX, surfaceY))
                    {
                        // Spawn rock just above the solid tile
                        Vector2 rockPos = new Vector2(
                            checkX * 16f + 8f,
                            surfaceY * 16f - 8f
                        );

                        // Velocity: arc upward, slight outward spread from centre
                        float horizontalDir   = tileOffsetX > 0 ? 1f : -1f;
                        float horizontalSpeed = Math.Abs(tileOffsetX) / 25f * 2.5f; // further = more spread
                        Vector2 rockVel       = new Vector2(horizontalDir * horizontalSpeed, -8f);

                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            rockPos,
                            rockVel,
                            ModContent.ProjectileType<ClusteredRock>(),
                            10,
                            2f,
                            Main.myPlayer
                        );
                    }
                }
            }
        }
        return;
    }

    // ── Phase 4: brief pause after impact, then reset ──

    // if (AITimer >= 120f)
    // {
    //     _groundSmashHit = false;
    //     AITimer = 0f;

    //     ExtraTimer++;
    //     if (ExtraTimer >= 3)
    //     {
    //         ExtraTimer = 0;
    //         AIState = (float)ActionState.Reset;
    //     }

    //     NPC.netUpdate = true;
    // }

    if (AITimer >= 120f)
    {
        _groundSmashHit = false;
        AITimer = 0f;
        AIState = (float)ActionState.Reset;
        NPC.netUpdate = true;
    }
}

        private Vector2 DashDirection;

        private void DoBehavior_DisappearingDash() //Final turn!! Stand up, my Vanguard!!
        {
            Player player = Main.player[NPC.target];
            AITimer++;


            // Phase 1: Boss disappears (fade out)
            if (AITimer <= 30f)
            {
                NPC.alpha = (int)MathHelper.Lerp(0, 255, AITimer / 30f); // fade OUT
                NPC.velocity = Vector2.Zero;

                // Force the boss to face the direction it is actually moving (idk why ts glitched)
                NPC.spriteDirection = NPC.direction = (DashDirection.X > 0 ? 1 : -1);
                return;
            }

            // Phase 2: At frame 31, spawn telegraph + teleport boss to radius point
            if (AITimer == 31f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float angle  = Main.rand.NextFloat(0, MathHelper.TwoPi);
                float radius = 400f;

                Vector2 targetPos = player.Center + radius * new Vector2(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle)
                );

                CreateTeleportTelegraph(targetPos);

                NPC.Center    = targetPos;
                NPC.netUpdate = true;
            }

            if (AITimer == 59)
            {
                ShootSpreadProjectiles(Main.player[NPC.target]);
            }

            // Phase 3: Wait at the teleport point (still invisible), lock dash direction
            if (AITimer <= 60f)
            {
                NPC.velocity = Vector2.Zero;
                NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                // Lock in dash direction once near the launch frame
                if (AITimer == 55f)
                {
                    DashDirection = Vector2.Normalize(player.Center - NPC.Center);
                    SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
                }
                return;
            }


            NPC.velocity = DashDirection * 30f;

            // Force the boss to face the direction it is actually moving (idk why ts glitched)
            NPC.spriteDirection = NPC.direction = (DashDirection.X > 0 ? 1 : -1);
            
            if (AITimer > 60)
            {
                NPC.velocity *= 0.90f; // friction; tweak for feel
            }

            // Fade boss back in during dash
            NPC.alpha = (int)MathHelper.Lerp(255, 0, Math.Min((AITimer - 60) / 30f, 1f));

            if (AITimer > 90)
            {
                NPC.velocity = Vector2.Zero;
            }

            // End attack after dash completes
            if (AITimer >= 120)
            {
                AITimer = 0;
                DashDirection  = Vector2.Zero;
                AIState = (float)ActionState.FinalTurn;
                NPC.alpha      = 0; // fully visible again
                // transition to next AI state here
            }
        }

        
        private void DoBehavior_SpawnAnimation()
        {
            if (AITimer == 0)
            {
                // Only trigger camera on client
                if (Main.netMode != NetmodeID.Server)
                    BossCameraSystem.StartBossPan(NPC.Center, panDuration: 90, holdDuration: 210);
            }
            AITimer++;

            if (AITimer < 120)
            {
                NPC.alpha = (int)MathHelper.Lerp(255, 0, (AITimer-60) / 60f);
                return;
            }

            if (AITimer == 180)
            {
                SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
                TriggerRoar(60);

                if (Main.netMode != NetmodeID.Server)
                    BossCameraSystem.TriggerShake(60f); // increase for stronger shake
            }


            if (AITimer >= 300f)
            {
                AITimer = 0f;
                AIState = (float)ActionState.TripleDashAttack;
            }
        }

        private ActionState previousAttack = ActionState.Reset;

        private void DoBehavior_ResetAI()
        {

            // reset all shared state here so individual attacks don't have to remember
            OrbitAngle = 0f;
            LaunchDirection = Vector2.Zero;
            LaunchTimer = 0f;
            AttackTimer = 0f;
            AITimer = 0f;
            ExtraTimer = 0f;

            
            NPC.TargetClosest(false);
            NPC.velocity *= 0.95f;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {   
                ActionState nextAttack;
                do
                {
                    if (LifeRatio < FinalPhaseLifeRatio)
                    {
                        nextAttack = ActionState.FinalTurn;
                    }
                    else if (LifeRatio > 0.7f)
                    {
                        nextAttack = Main.rand.Next(4) switch
                        {
                            0 => ActionState.CircleAndShoot,
                            1 => ActionState.TripleDashAttack,
                            2 => ActionState.LingeringRocks,
                            _ => ActionState.SansWall,
                        };
                    }
                    else if (LifeRatio > 0.4)
                    {
                        nextAttack = Main.rand.Next(5) switch
                        {
                            0 => ActionState.CircleAndShoot,
                            1 => ActionState.SansWall,
                            2 => ActionState.GroundSmash,
                            3 => ActionState.LingeringRocks,
                            _ => ActionState.MiniCyclones,
                        };  
                    }
                    else
                    {
                        nextAttack = Main.rand.Next(5) switch
                        {
                            0 => ActionState.LingeringRocks,
                            1 => ActionState.TripleDashAttack,
                            2 => ActionState.GroundSmash,
                            3 => ActionState.CircleAndShoot,
                            _ => ActionState.MiniCyclones,
                        };  
                    }
                }

                while (nextAttack == previousAttack); // never repeat the same attack twice


                // ActionState nextAttack = ActionState.GroundSmash; //testing


                previousAttack = nextAttack;
                AIState = (float)nextAttack;
                AITimer = 0f;
                NPC.netUpdate = true;
            }
        }




private void DoBehavior_TripleDashAttack()
{
    Player player = Main.player[NPC.target];

    // Phase 1: move to above player while AITimer is negative
    if (AITimer <= 0)
    {
        Vector2 targetPos = player.Center + new Vector2(0f, -200f);
        Vector2 toTarget = targetPos - NPC.Center;
        float dist = toTarget.Length();

        float rampFrames = -AITimer; // AITimer is negative, so this gives a positive frame count
        float maxSpeed = MathHelper.Clamp(rampFrames * 0.3f, 1f, 40f);
        Vector2 idealVelocity = Vector2.Normalize(toTarget) * Math.Min(dist * 0.08f, maxSpeed);
        NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.07f);
        NPC.spriteDirection = player.Center.X > NPC.Center.X ? 1 : -1;

        if (dist <= 240f && AITimer <= -120 || AITimer < -240) // at least 180 frames and close or already been 600 frames
        {
            NPC.velocity = Vector2.Zero;

            AITimer = 1f; // kick into phase 2
        }
        else
        {
            AITimer--; // keep decrementing while still moving
        }

        return;
    }


    // Phase 2
    AITimer++;


    int localFrame = (int)AITimer-1;


    // --- Dash 1: DOWN ---
    if (localFrame == 1) // windup done, launch
    {
        Vector2 direction = Vector2.Normalize(player.Center - NPC.Center); // aim at player
        NPC.velocity = direction * 24f;

        SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
TriggerRoar(30);
    }

    // --- Dash 2: UP (back above player) ---
    if (localFrame == 45)
    {
        Vector2 direction = Vector2.Normalize(player.Center - NPC.Center); // aim at player
        NPC.velocity = direction * 24f;

        SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
TriggerRoar(30);
    }

    // --- Dash 3: DOWN + projectiles ---
    if (localFrame == 105)
    {
        // Shoot 5 projectiles in a spread BEFORE the dash
        ShootSpreadProjectiles(Main.player[NPC.target]);

        SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
TriggerRoar(50);
    }

    if (localFrame == 106)
    {
        Vector2 direction = Vector2.Normalize(player.Center - NPC.Center); // aim at player again
        NPC.velocity = direction * 24f;
    }

    // --- Decelerate after each dash launch ---
    if (localFrame > 0)
    {
        NPC.velocity *= 0.98f; // friction; tweak for feel
    }

    // --- Reset after full sequence ---
    if (localFrame >= 166)
    {
        AITimer = 0f;
        NPC.velocity = Vector2.Zero;
        AIState = (float)ActionState.Reset; // ← add this
        NPC.netUpdate = true;               // ← and this
    }
}

private void ShootSpreadProjectiles(Player target)
{
    if (!Main.dedServ) // skip visual-only logic on server if needed
    {
        // Optional: spawn a telegraph dust/particle burst here
    }

    float projSpeed = 14f;
    int spreadCount = 5;
    float spreadAngle = MathHelper.ToRadians(45f); // total arc in degrees

    // Direction toward the player (downward in this context)
    Vector2 baseDirection = Vector2.Normalize(target.Center - NPC.Center);

    for (int i = 0; i < spreadCount; i++)
    {
        // Evenly space projectiles across the spread arc
        float lerpT   = spreadCount == 1 ? 0.5f : i / (float)(spreadCount - 1);
        float rotation = MathHelper.Lerp(-spreadAngle / 2f, spreadAngle / 2f, lerpT);

        Vector2 velocity = baseDirection.RotatedBy(rotation) * projSpeed;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                NPC.Center,
                velocity,
                ModContent.ProjectileType<CycloneProjectile>(),
                DashSpreadDamage,
                2f,        // knockback
                Main.myPlayer,
                ai0: Main.rand.Next(4)
            );
        }
    }
}

            private void DoBehavior_LingeringRocks()
            {
             AITimer++;

            Player player = Main.player[NPC.target];


                // Phase 1: Reposition above player 
            if (AITimer <= 120)
            {
                Vector2 targetPos = player.Center + new Vector2(0f, -100f);
                Vector2 toTarget = targetPos - NPC.Center;
                float dist = toTarget.Length();

                // Spiral: orbit angle spins faster as it closes in
                float spiralAngle = AITimer * 0.18f;
                float spiralRadius = MathHelper.Clamp(dist * 0.4f, 0f, 120f); // shrinks as it gets closer
                Vector2 spiralOffset = new Vector2(
                    (float)Math.Cos(spiralAngle),
                    (float)Math.Sin(spiralAngle)
                ) * spiralRadius;

                // Wobble on top of the spiral
                float wobble = (float)Math.Sin(AITimer * 0.3f) * (dist * 0.05f); // fades as dist shrinks
                Vector2 wobbleOffset = Vector2.Normalize(toTarget).RotatedBy(MathHelper.PiOver2) * wobble;

                float maxSpeed = MathHelper.Clamp(AITimer * 0.25f, 1f, 18f);
                Vector2 idealVelocity = Vector2.Normalize(toTarget + spiralOffset + wobbleOffset)
                                        * Math.Min(dist * 0.08f, maxSpeed);

                NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.07f);

                NPC.spriteDirection = player.Center.X > NPC.Center.X ? 1 : -1;

                if (AITimer == 120)
                {
                    NPC.velocity = Vector2.Zero;
                }
                return;
            }

            if (AITimer == 121)
            {
                SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
                TriggerRoar(30);

                if (Main.netMode != NetmodeID.Server)
                    BossCameraSystem.TriggerShake(10f); // increase for stronger shake
                ShootLingeringRockProjectiles();
            }

            if (AITimer >= 210f)
            {
                AITimer = 0f;
                AIState = (float)ActionState.Reset;
            }


        }

        private void ShootLingeringRockProjectiles()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            int rockCount = 5;
            float spreadWidth = 200f; // total horizontal spread


            for (int i = 0; i < rockCount; i++)
            {
                // Evenly space rocks across the spread, centered on boss
                float t = rockCount == 1 ? 0.5f : (float)i / (rockCount - 1); // 0..1
                float xOffset = MathHelper.Lerp(-spreadWidth / 2f, spreadWidth / 2f, t);

                Vector2 spawnPos = NPC.Center + new Vector2(xOffset, 0f);

                // Arc upward: center rock goes most upward, edges go less
                // Horizontal velocity fans outward from center
                float normalizedT = t - 0.5f; // -0.5..0.5, center = 0
                float horizontalSpeed = normalizedT * 6f; // fans outward
                float verticalSpeed = -MathHelper.Lerp(18f, 14f, Math.Abs(normalizedT) * 2f);

                Vector2 velocity = new Vector2(horizontalSpeed, verticalSpeed);

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    spawnPos,
                    velocity,
                    ModContent.ProjectileType<ClusteredRock>(),
                    10,
                    2f,
                    Main.myPlayer
                );
            }
        }
                    

        // orbit state variables
        private float OrbitAngle = 0f;        // current angle around the player
        private float OrbitRadius = 300f;     // how far from the player
        private const float OrbitSpeed = 0.03f;    // radians per tick
        private const float OrbitLaunchDegrees = 270f; // degrees to orbit before launching
        private Vector2 LaunchDirection = Vector2.Zero;
        private float LaunchTimer = 0f;

        private void DoBehavior_Dash()
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

                Vector2 targetPos     = player.Center + new Vector2(OrbitRadius, 0f).RotatedBy(OrbitAngle);
                Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
                NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.15f);

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
                AIState = (float)ActionState.Reset;

            }
        }


private float Attack2OrbitAngle = 0f;
private int   Attack2ShootTimer = 0;
private float OrbitRadius2 = 400f;
private const float OrbitSpeed2 = 0.02f;

private void DoBehavior_CircleAndShoot()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    // Phase 1: move to nearest orbit point (0-60 ticks)
    if (AITimer == 1f)
    {
        // Calculate the angle from player to NPC, snap to that orbit point
        OrbitAngle = (NPC.Center - player.Center).ToRotation();
        NPC.netUpdate = true;
    }

    if (AITimer <= 60f)
    {
        Vector2 targetPos     = player.Center + new Vector2(OrbitRadius2, 0f).RotatedBy(OrbitAngle);
        float dist            = Vector2.Distance(NPC.Center, targetPos);
        NPC.velocity          = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(dist * 0.1f, 2f, 20f);
        return;
    }

    if (AITimer == 61f && LifeRatio < 0.7f)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            float clone1Angle = OrbitAngle + MathHelper.Pi;
            Vector2 clone1Pos = player.Center + new Vector2(OrbitRadius2, 0f).RotatedBy(clone1Angle);

            int clone1Index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clone1Pos.X, (int)clone1Pos.Y, ModContent.NPCType<CycloneClone>());
            Main.npc[clone1Index].ai[1] = 2f;
            Main.npc[clone1Index].ai[2] = clone1Angle;
            Main.npc[clone1Index].ai[3] = OrbitRadius2;
            Main.npc[clone1Index].netUpdate = true;
        }
    }

    // Phase 2: orbit continuously while shooting

        if (AITimer >= 61f && AITimer < 420f)
    {

        OrbitAngle += OrbitSpeed2;

        Vector2 targetPos     = player.Center + new Vector2(OrbitRadius2, 0f).RotatedBy(OrbitAngle);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.10f);

        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;

        AttackTimer++;
        if (AttackTimer >= 50)
        {
            AttackTimer = 0;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
                int damage       = 10;

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    shootDir * 12f,
                    ModContent.ProjectileType<CycloneProjectile>(),
                    damage,
                    2f,
                    Main.myPlayer,
                    ai0: Main.rand.Next(4)
                );

                SoundEngine.PlaySound(ProjectileSound, NPC.Center);
            }
        }

        return;
    }


    if (AITimer >= 420f)
    {
        NPC.alpha                 = 0;
        AITimer                   = 0f;
        OrbitAngle = 0f;
        AttackTimer = 0;


        AIState    = (float)ActionState.Reset;

        NPC.netUpdate = true;
    }
}

private float OrbitRadius3    = 400f;
private int   Attack3ShootTimer = 0;
private const float Attack3Height = 160f; // total spread height of the 5 bullets
private float Attack3SubAttack = 0f;
private float Attack3StartY    = 0f;
private float Attack3EndY      = 0f;

private void DoBehavior_SansWall()
{
    Player player = Main.player[NPC.target];
    NPC.spriteDirection = player.Center.X > NPC.Center.X ? 1 : -1;
    AITimer++;

     if (AITimer <= 60)
{
    float xOffset = NPC.Center.X < player.Center.X ? -400f : 400f;
    Vector2 targetPos = player.Center + new Vector2(xOffset, 0f);
    Vector2 toTarget = targetPos - NPC.Center;
    float dist = toTarget.Length();

    float maxSpeed = MathHelper.Clamp(AITimer * 0.25f, 10f, 20f);
    Vector2 idealVelocity = Vector2.Normalize(toTarget) * Math.Min(dist * 0.08f, maxSpeed);

    NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.07f);


    return;
}

    if (AITimer == 61f && LifeRatio < 0.7f)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            // Boss is on the left → clone goes right (+), boss on right → clone goes left (-)
            float bossSide = NPC.Center.X < player.Center.X ? 1f : -1f;
            float cloneOffset = bossSide * OrbitRadius3;

            Vector2 clonePos = player.Center + new Vector2(cloneOffset, 0f);
            int cloneIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clonePos.X, (int)clonePos.Y, ModContent.NPCType<CycloneClone>());
            Main.npc[cloneIndex].ai[1] = 3f;
            Main.npc[cloneIndex].ai[2] = cloneOffset; // signed: positive = right, negative = left
            Main.npc[cloneIndex].netUpdate = true;
        }
    }

    // Phase 3: track player and fire volleys
    if (AITimer >= 61f && AITimer < 420f)
    {


        float verticalOffset = 0f;
        float yDiff = NPC.Center.Y - player.Center.Y;
        if (Math.Abs(yDiff) < 20f)
            verticalOffset = yDiff == 0f ? (NPC.whoAmI % 2 == 0 ? 40f : -40f) : Math.Sign(yDiff) * 40f;

        float xOffset = NPC.Center.X < player.Center.X ? -OrbitRadius3 : OrbitRadius3;
        Vector2 targetPos = new Vector2(player.Center.X + xOffset, player.Center.Y + verticalOffset);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        AttackTimer++;
        if (AttackTimer >= 70)
        {
            AttackTimer = 0;
            AttackTimer  = Main.rand.Next(3);
            PickAttack3Range(player);
            FireWallVolley(player);
        }

        return;
    }


    if (AITimer >= 420f)
    {
        NPC.alpha  = 0;
        AttackTimer = 0;
        AITimer    = 0f;
        AIState = (float)ActionState.Reset;

    }
}


private void PickAttack3Range(Player player)
{
    switch ((int)Attack3SubAttack)
    {
        case 0: // middle
            Attack3StartY = player.Center.Y - Attack3Height / 2f;
            Attack3EndY   = player.Center.Y + Attack3Height / 2f;
            break;
        case 1: // upper
            Attack3StartY = player.Center.Y - Attack3Height;
            Attack3EndY   = player.Center.Y;
            break;
        case 2: // lower
            Attack3StartY = player.Center.Y;
            Attack3EndY   = player.Center.Y + Attack3Height;
            break;
    }
}

private void FireWallVolley(Player player)
{
    if (Main.netMode == NetmodeID.MultiplayerClient) return;

    int   bulletCount = 5;
    float speed       = 4f;
    int   damage      = 10;
    float direction   = NPC.Center.X < player.Center.X ? 1f : -1f;

    for (int i = 0; i < bulletCount; i++)
    {
        float t      = (float)i / (bulletCount - 1);
        float spawnY = MathHelper.Lerp(Attack3StartY, Attack3EndY, t);

        Vector2 spawnPos = new Vector2(NPC.Center.X, spawnY);
        int     variant  = Main.rand.Next(4);

        Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            spawnPos,
            new Vector2(speed * direction, 0f),
            ModContent.ProjectileType<CycloneProjectile>(),
            damage,
            2f,
            Main.myPlayer,
            ai0: variant,
            ai1: 1f
        );
    }
}

private float Attack4LoopCount = 0f;
private bool  Attack4LinesSpawned = false;
private int   Attack4Part1Random = 0;
private float Attack4Angle     = 0f;
private float Attack4HoverTimer = 0f;
private float Attack4ShootTimer = 0f;
private float OrbitRadius4 = 500f;

private void DoBehavior_FallingRocks()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    // Phase 1: disappear (0-60 ticks)
    if (AITimer < 60f)
    {
        NPC.velocity = Vector2.Zero;
        NPC.alpha    = (int)MathHelper.Lerp(0, 255, AITimer / 60f);
        return;
    }

    // Phase 2: teleport to bottom of player
    if (AITimer == 61f)
    {
        Attack4Angle          = MathHelper.PiOver2;
        NPC.Center            = player.Center + new Vector2(0f, OrbitRadius4);
        NPC.alpha             = 255;
        NPC.velocity          = Vector2.Zero;
        Attack4LinesSpawned   = false;
        int dir               = Main.rand.NextBool() ? 1 : -1;
        Attack4Part1Random    = dir * Main.rand.Next(3, 6) * 16;
        NPC.netUpdate         = true;

        
    }

    // Phase 3: arc from bottom to top
    float targetAngle = -MathHelper.PiOver2;
    if (AITimer >= 61f && Attack4Angle > targetAngle)
    {
        NPC.alpha    = (int)MathHelper.Lerp(255, 0, Math.Min((AITimer - 61f) / 60f, 1f));
        Attack4Angle -= OrbitSpeed;

        Vector2 idealPos      = player.Center + new Vector2(OrbitRadius4, 0f).RotatedBy(Attack4Angle);
        Vector2 idealVelocity = Vector2.Normalize(idealPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, idealPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;
        return;
    }

    // Phase 4: hover above player
    if (AITimer >= 61f && Attack4Angle <= targetAngle)
    {
        NPC.alpha = 0;

        float horizontalOffset = 0f;
        float xDiff = NPC.Center.X - player.Center.X;
        if (Math.Abs(xDiff) < 20f)
            horizontalOffset = xDiff == 0f ? (NPC.whoAmI % 2 == 0 ? 140f : -140f) : Math.Sign(xDiff) * 40f;

        Vector2 targetPos     = new Vector2(player.Center.X + horizontalOffset, player.Center.Y - OrbitRadius4);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;

        Attack4HoverTimer++;

        if (Attack4HoverTimer == 60f && !Attack4LinesSpawned)
        {
            SpawnTelegraphLines(player);
        }
        // Fire lasers 40 ticks after telegraph
        if (Attack4HoverTimer == 90f && !Attack4LinesSpawned)
        {
            SpawnLaserProjectiles(player);
            Attack4LinesSpawned = true;
        }

        Attack4ShootTimer++;
        if (Attack4ShootTimer >= 50)
        {
            Attack4ShootTimer = 0;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
                int damage       = 10;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir * 10f,ModContent.ProjectileType<CycloneProjectile>(), damage, 2f, Main.myPlayer, ai0: Main.rand.Next(4)); 
                
            }
        }

        // Each loop: wait 120 ticks then repeat
        if (Attack4HoverTimer >= 180f)
        {
            Attack4LoopCount++;
            Attack4HoverTimer   = 0f;
            Attack4LinesSpawned = false;
            int dir             = Main.rand.NextBool() ? 1 : -1;
            Attack4Part1Random  = dir * Main.rand.Next(3, 6) * 16;


            if (Attack4LoopCount >= 4)
            {
                Attack4LoopCount  = 0f;
                Attack4HoverTimer = 0f;
                Attack4Angle      = 0f;
                AITimer           = 0f;
                AIState = (float)ActionState.Reset;

            }
        }
    }
}

private void SpawnTelegraphLines(Player player)
{
    int lineCount   = 16;
    int lineSpacing = 8 * 16;

    for (int i = 0; i < lineCount; i++)
    {
        int x = (int)(NPC.Center.X + Attack4Part1Random - lineSpacing * (lineCount / 2) + i * lineSpacing);
        Vector2 telePos = new Vector2(x, NPC.Center.Y - 550);

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                telePos,
                Vector2.Zero,
                ModContent.ProjectileType<TelegraphLines>(),
                0, 0, Main.myPlayer
            );
        }
    }
}

private void SpawnLaserProjectiles(Player player)
{
    int lineCount   = 16;
    int lineSpacing = 8 * 16;
    int projSpeed   = 12;
    int projDamage  = 10;

    for (int i = 0; i < lineCount; i++)
    {
        int x = (int)(NPC.Center.X + Attack4Part1Random - lineSpacing * (lineCount / 2) + i * lineSpacing);
        Vector2 projPos = new Vector2(x, NPC.Center.Y - 300);

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                projPos,
                new Vector2(0, projSpeed),
                ModContent.ProjectileType<CycloneProjectile>(),
                projDamage, 2f, Main.myPlayer,
                ai0: Main.rand.Next(4)
            );
        }
    }
}






private void EnhancedAttack1()
{   
    Player player = Main.player[NPC.target];
    AITimer++;

    ref float clonePosX = ref NPC.localAI[0];
    ref float clonePosY = ref NPC.localAI[1];
    
    
    // Phase 1: disappear
    if (AITimer < 60f)
    {
        NPC.velocity = Vector2.Zero;
        NPC.alpha    = (int)MathHelper.Lerp(0, 255, AITimer / 60f);
        return;
    }

    // Phase 2: teleport above player
    if (AITimer == 61f)
    {
        NPC.Center = player.Center + new Vector2(300f, -240f);
        NPC.alpha     = 255;
        NPC.velocity  = Vector2.Zero;
        NPC.netUpdate = true;
        SoundEngine.PlaySound(CycloneRoarGor, NPC.Center);

    }

    ExtraTimer++;
    
    if (ExtraTimer < 700)
    {
        if (ExtraTimer % 121 == 61 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            float cloneAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
            float radius = 300f;

            Vector2 clonePos = player.Center + radius * new Vector2(
                (float)Math.Cos(cloneAngle),
                (float)Math.Sin(cloneAngle)
            );

            clonePosX = clonePos.X;
            clonePosY = clonePos.Y;
            CreateTeleportTelegraph(clonePos);
            NPC.netUpdate = true;
        }

        if (ExtraTimer % 121 == 91 && ExtraTimer > 2 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            int cloneIndex = NPC.NewNPC(
                NPC.GetSource_FromAI(),
                (int)clonePosX,
                (int)clonePosY,
                ModContent.NPCType<CycloneClone>()
            );

            Main.npc[cloneIndex].Center = new Vector2(clonePosX, clonePosY); //center the boss 
            Main.npc[cloneIndex].ai[1] = 1f;
            Main.npc[cloneIndex].netUpdate = true;
            SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
        }
    }
    
    // Phase 3: hover and rain projectiles
    if (AITimer >= 61f)
    {
        // Fade in
        NPC.alpha = (int)MathHelper.Lerp(255, 0, Math.Min((AITimer - 61f) / 60f, 1f));

        // Subtle bobbing — stays in place but moves slightly up and down
        float bobOffset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 4f;
        NPC.velocity = new Vector2(0f, bobOffset * 0.1f);

        // Face player
        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;

        AttackTimer++;
        if (AttackTimer % 40 == 0) // every 40 ticks fire a volley
        {

            if (Main.netMode != NetmodeID.MultiplayerClient)
                SpawnRainVolley(player);

            
        }

        if (AttackTimer >= 800)
        {
            AttackTimer = 0;
            NPC.alpha                 = 0;
            AITimer                   = 0f;
            ExtraTimer = 0;


            AIState    = (float)ActionState.Reset;
            
            NPC.netUpdate = true;
        }
    }
}

public static void CreateTeleportTelegraph(Vector2 teleportPosition, float cloudOpacity = 1f)
{
    CloudParticle noxiousCloud = new(teleportPosition, Vector2.Zero, Color.White * cloudOpacity, Color.DarkGray, 120, Main.rand.NextFloat(1.4f, 1.8f));
    ParticleHandler.SpawnParticle(noxiousCloud);

}

private void SpawnRainVolley(Player player)
{
    int   count   = 3;   // projectiles per volley
    int   damage  = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

    for (int i = 0; i < count; i++)
    {
        // Spread evenly across a range centered on the player
        float t      = (float)i / (count - 1);
        float spawnX = player.Center.X;

        // Add a small random offset so it doesn't look too rigid
        spawnX += Main.rand.NextFloat(-200f, 200f);

        Vector2 spawnPos = new Vector2(spawnX, player.Center.Y - 800f); // high above

        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos,
        new Vector2(Main.rand.NextFloat(-1f, 1f), 8f),
        ModContent.ProjectileType<CycloneProjectile>(), damage, 2f, Main.myPlayer, ai0: Main.rand.Next(4));
    }
}






private float EnhancedOrbitRadius4 = 400f;
private int EnhancedAttack4CloneSequence = 0; // 0=topleft, 1=topright, 2=botleft, 3=botright

private void EnhancedAttack4()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    if (AITimer < 60f)
    {
        NPC.velocity = Vector2.Zero;
        NPC.alpha    = (int)MathHelper.Lerp(0, 255, AITimer / 60f);
        return;
    }

    if (AITimer == 61f)
    {
        NPC.Center                 = player.Center + new Vector2(0f, -EnhancedOrbitRadius4); // tp straight to top
        NPC.alpha                  = 0;
        NPC.velocity               = Vector2.Zero;
        NPC.netUpdate              = true;
    }


    if (AITimer >= 61f)
    {   
        NPC.alpha = (int)MathHelper.Lerp(255, 0, Math.Min((AITimer - 61f) / 60f, 1f));

        NPC.alpha = 0;

        float horizontalOffset = 0f;
        float xDiff = NPC.Center.X - player.Center.X;
        if (Math.Abs(xDiff) < 20f)
            horizontalOffset = xDiff == 0f ? (NPC.whoAmI % 2 == 0 ? 140f : -140f) : Math.Sign(xDiff) * 40f;

        Vector2 targetPos     = new Vector2(player.Center.X + horizontalOffset, player.Center.Y - EnhancedOrbitRadius4);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 8f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;

        AttackTimer++;

        if (AttackTimer % 210f == 181 && AttackTimer > 2 && AttackTimer < 850) //&& AttackTimer > 2 prevents insta spawn
            SpawnTelegraphLines(player);

        if (AttackTimer % 210f == 1 && AttackTimer > 2 && AttackTimer < 850) //30 frame gap
        {
            SpawnLaserProjectiles(player);
        }


        if (AttackTimer % 50 == 1 && AttackTimer > 2)
        {

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
                int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    shootDir * 10f,
                    ModContent.ProjectileType<CycloneProjectile>(),
                    damage,
                    2f,
                    Main.myPlayer,
                    ai0: Main.rand.Next(4)
                );
                SoundEngine.PlaySound(ProjectileSound, NPC.Center);
            }
        }


        ExtraTimer++;
        

        // Spawn next clone 
        if (ExtraTimer % 210f == 200 && ExtraTimer < 850 && ExtraTimer > 2) //spawn 4 , and prevent insta spawn
        {

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 clonePos = EnhancedAttack4CloneSequence switch
                {
                    0 => player.Center + new Vector2(-360f, -360f), // top left
                    1 => player.Center + new Vector2( 360f, -360f), // top right
                    2 => player.Center + new Vector2(360f,  360f), // bot right
                    _ => player.Center + new Vector2( -360f,  360f), // bot left
                };

                int cloneIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clonePos.X, (int)clonePos.Y, ModContent.NPCType<CycloneClone>());
                Main.npc[cloneIndex].ai[1] = 4f;
                Main.npc[cloneIndex].ai[2] = (float)NPC.whoAmI; // pass boss whoAmI so clone can signal back
                Main.npc[cloneIndex].netUpdate = true;

                EnhancedAttack4CloneSequence = (EnhancedAttack4CloneSequence + 1) % 4;
            }
        }

        if (AttackTimer >= 1000f)
        {

            AttackTimer   = 0f;
            AttackTimer = 0f;
            AITimer                   = 0f; 
            EnhancedAttack4CloneSequence   = 0;
            ExtraTimer      = 0f;


            AIState    = (float)ActionState.Reset;

            NPC.netUpdate = true;
            }
        }
    }

private void DoBehavior_SpawnMiniCyclones(Player target)
{
    
    AITimer++;
    NPC.velocity = Vector2.Zero;

    if (AITimer == 60)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            for (float offset = -750f; offset < 750f; offset += 150f)
            {
                Vector2 spawnPosition = target.Center + new Vector2(offset, -750f);
                Vector2 pearlShootVelocity = Vector2.UnitY * 8f;
                Projectile.NewProjectile(
                NPC.GetSource_FromAI(),  
                spawnPosition,
                pearlShootVelocity,
                ModContent.ProjectileType<MiniCyclone>(),
                10,
                0f,
                Main.myPlayer
                );
            }
            for (float offset = -675f; offset < 825f; offset += 150f)
            {
                Vector2 spawnPosition = target.Center + new Vector2(offset, 750f);  
                Vector2 pearlShootVelocity = Vector2.UnitY * -8f;
                Projectile.NewProjectile(
                NPC.GetSource_FromAI(),  
                spawnPosition,
                pearlShootVelocity,
                ModContent.ProjectileType<MiniCyclone>(),
                10,
                0f,
                Main.myPlayer
                );
            }
            NPC.netUpdate = true;
        }
    }


    
    if (AITimer >= 240)
    {
        AITimer = 0f;
        AIState = (float)ActionState.Reset;
    }
}

    


#region Networking
public override void SendExtraAI(System.IO.BinaryWriter writer)
{

    // Random picks clients can't reproduce
    writer.Write(OrbitAngle);          // Main.rand.NextFloat on server
    writer.Write(Attack2OrbitAngle);
    writer.Write(Attack4Angle);
    writer.Write(Attack4Part1Random);  // random direction * random int


    writer.Write(EnhancedAttack4CloneSequence);
    // LaunchDirection is calculated from NPC.Center → player.Center
    // but it's only set once and then held, so sync it
    writer.Write(LaunchDirection.X);
    writer.Write(LaunchDirection.Y);
}

public override void ReceiveExtraAI(System.IO.BinaryReader reader)
{

    OrbitAngle = reader.ReadSingle();
    Attack2OrbitAngle = reader.ReadSingle();
    Attack4Angle = reader.ReadSingle();
    Attack4Part1Random = reader.ReadInt32();


    EnhancedAttack4CloneSequence = reader.ReadInt32();

    LaunchDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
}

#endregion Networking


}






















public class CycloneClone : ModNPC
{
    public ref float AITimer => ref NPC.ai[0];
    //npc.ai[1] is the attack we pass through
    //npc.ai[2] is being used to pass info from boss
    public ref float ExtraIncrement => ref NPC.ai[3];
    private Vector2 LaunchTarget = Vector2.Zero;
    
    private const int FadeInDuration = 59;
    private const int TargetAlpha    = 200; // semi transparent, lower = more visible

    private static readonly SoundStyle ProjectileSound = new SoundStyle("TheBattleCats/Assets/Boss/Cyclone/CycloneProjectile")
    {
        PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
    };

    public override void SetDefaults()
    {
        NPC.width         = 110;
        NPC.height        = 110;
        NPC.damage        = 1;
        NPC.defense       = 0;
        NPC.lifeMax       = 1;
        NPC.noGravity     = true;
        NPC.noTileCollide = true;
        NPC.knockBackResist = 0f;
        NPC.alpha         = 255; // start invisible
        NPC.dontTakeDamage = true;
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

    private int   Attack2ShootTimer = 0;
    private float OrbitRadius2 = 400f;
    private const float OrbitSpeed2 = 0.02f;
    private float Attack2OrbitAngle = 0f;

    public override void AI()
    {
        Player player = Main.player[NPC.target];
        NPC.TargetClosest(true);
        

        switch ((int)NPC.ai[1])
        {
            case 1: DoAttack1();   break;
            case 2: DoAttack2();  break;
            case 3: DoAttack3();   break;
            case 4: DoAttack4(); break;
        }
        

    }
    
// private float CloneOrbitAngle     = 0f;
// private float CloneOrbitRadius    = 300f;
// private Vector2 CloneLaunchDirection = Vector2.Zero;
// private const float CloneOrbitSpeed = 0.03f;
// private const float CloneOrbitLaunchDegrees = 270f;

// private const float OrbitRadians = 270f * (MathHelper.Pi / 180f); //270 degrees i think
// private const float OrbitDuration = OrbitRadians / CloneOrbitSpeed;
// private bool CloneLaunched = false;
// private void DoAttack1()
// {
//     Player player = Main.player[NPC.target];
//     AITimer++;

//     // Phase 2: teleport to random position around player
//     if (AITimer == 1f)
//     {
//         NPC.Center        = player.Center + new Vector2(CloneOrbitRadius, 0f).RotatedBy(CloneOrbitAngle);
//         NPC.alpha         = 160;
//         NPC.netUpdate     = true;
//     }

//     // Phase 3: orbit
//     if (AITimer >= 1f && AITimer < OrbitDuration)
//     {   
//         NPC.alpha = (int)MathHelper.Lerp(255, 200, (AITimer - 1f) / 60f);
//         CloneOrbitAngle = AITimer * CloneOrbitSpeed;

//         Vector2 targetPos     = player.Center + new Vector2(CloneOrbitRadius, 0f).RotatedBy(CloneOrbitAngle);
//         Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
//         NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.15f);

//         NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
//         NPC.spriteDirection = NPC.direction;
//         return;
//     }

//     // Phase 4: launch at player
//     if (CloneLaunchDirection == Vector2.Zero && CloneLaunched == false)
//     {
//         CloneLaunchDirection = Vector2.Normalize(player.Center - NPC.Center);
//         CloneLaunched = true;

//     }
//     NPC.velocity = CloneLaunchDirection * 14f;

//     ExtraIncrement++;
//     if (ExtraIncrement >= 60)
//     {
//         NPC.alpha = (int)MathHelper.Lerp(200, 255, Math.Min((ExtraIncrement - 60f) / 60f, 1f));
//         CloneLaunchDirection = Vector2.Zero;
        
//     }

//     if (ExtraIncrement >= 120)
//     {
//         ExtraIncrement     = 0;
//         CloneOrbitAngle      = 0f;
//         NPC.active           = false;
//         CloneLaunched = false;
//     }
// }


private Vector2 DashDirection = Vector2.Zero;
private bool DashLaunched = false;
private float CloneOrbitRadius    = 300f;
private void DoAttack1()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    if (AITimer == 25f)
    {
        DashDirection = Vector2.Normalize(player.Center - NPC.Center);
    }
    // Frames 1–30: wait, fade in
    if (AITimer <= 30f)
    {
        NPC.alpha = (int)MathHelper.Lerp(255, 200, (AITimer - 1f) / 30f);
        NPC.velocity = Vector2.Zero;
        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;
        return;
        
    }

    // Frame 61: lock in dash direction
    if (!DashLaunched)
    {
        DashLaunched  = true;
    }

    // Dash
    NPC.velocity = DashDirection * 14f;
    ExtraIncrement++;

    // Only start fading after 60 frames of dashing
    if (ExtraIncrement >= 60)
    {
        NPC.alpha = (int)MathHelper.Lerp(200, 255, Math.Min((ExtraIncrement - 60f) / 30f, 1f));
    }

    // Kill after 90 frames total (60 dash + 60 fade)
    if (ExtraIncrement >= 90)
    {
        ExtraIncrement = 0;
        DashLaunched   = false;
        DashDirection  = Vector2.Zero;
        NPC.active     = false;
    }
}


private void DoAttack2()
{
    Player player = Main.player[NPC.target];
    AITimer++;  
    if (AITimer == 1f)
    {
        Attack2OrbitAngle = NPC.ai[2]; // use angle passed from boss
        NPC.alpha         = 255;
        NPC.velocity      = Vector2.Zero;
        NPC.netUpdate     = true;
    }

    // Orbit and shoot
    if (AITimer >= 1f && AITimer < 360f)
    {   
        NPC.alpha = (int)MathHelper.Lerp(255, 140, Math.Min(AITimer / 60f, 1f));
        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;

        Attack2OrbitAngle += OrbitSpeed2;

        Vector2 targetPos     = player.Center + new Vector2(OrbitRadius2, 0f).RotatedBy(Attack2OrbitAngle);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.10f);

        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;

        Attack2ShootTimer++;
        if (Attack2ShootTimer >= 50 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Attack2ShootTimer = 0;

            SoundEngine.PlaySound(ProjectileSound, NPC.Center);

            Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
            int damage       = 10;


            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir * 12f,
            ModContent.ProjectileType<CycloneProjectile>(), damage, 2f, Main.myPlayer, ai0: Main.rand.Next(4));
        }

        return;
    }

    // Fade out
    if (AITimer >= 360f && AITimer < 420f)
    {
        NPC.alpha = (int)MathHelper.Lerp(140, 255, Math.Min((AITimer - 360f) / 60f, 1f));
        NPC.velocity *= 0.9f;
        return;
    }

    if (AITimer >= 420f)
        NPC.active = false;
}


private int   CloneAttack3ShootTimer = 0;
private float CloneAttack3StartY     = 0f;
private float CloneAttack3EndY       = 0f;
private const float CloneAttack3Height = 160f;

private void DoAttack3()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    if (AITimer == 1f)
    {
        NPC.Center    = player.Center + new Vector2(NPC.ai[2], 0f);
        NPC.alpha     = 255;
        NPC.velocity  = Vector2.Zero;
        NPC.netUpdate = true;
    }

    if (AITimer >= 1f && AITimer < 360f)
    {
        NPC.alpha = (int)MathHelper.Lerp(255, 140, Math.Min(AITimer / 60f, 1f));

        float verticalOffset = 0f;
        float yDiff = NPC.Center.Y - player.Center.Y;
        if (Math.Abs(yDiff) < 20f)
            verticalOffset = yDiff == 0f ? (NPC.whoAmI % 2 == 0 ? 40f : -40f) : Math.Sign(yDiff) * 40f;

        Vector2 targetPos     = new Vector2(player.Center.X + NPC.ai[2], player.Center.Y + verticalOffset);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        // Face toward player based on which side the clone is on
        int facingDir = NPC.ai[2] > 0f ? -1 : 1; // right side → face left, left side → face right
        NPC.direction       = facingDir;
        NPC.spriteDirection = facingDir;

        CloneAttack3ShootTimer++;
        if (CloneAttack3ShootTimer >= 70)
        {
            CloneAttack3ShootTimer = 0;

            int subAttack = Main.rand.Next(3);
            switch (subAttack)
            {
                case 0:
                    CloneAttack3StartY = player.Center.Y - CloneAttack3Height / 2f;
                    CloneAttack3EndY   = player.Center.Y + CloneAttack3Height / 2f;
                    break;
                case 1:
                    CloneAttack3StartY = player.Center.Y - CloneAttack3Height;
                    CloneAttack3EndY   = player.Center.Y;
                    break;
                case 2:
                    CloneAttack3StartY = player.Center.Y;
                    CloneAttack3EndY   = player.Center.Y + CloneAttack3Height;
                    break;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Fire toward player: if clone is on the right, shoot left (-1); if on the left, shoot right (+1)
                float shootDirX = NPC.ai[2] > 0f ? -4f : 4f;

                for (int i = 0; i < 5; i++)
                {
                    float t      = (float)i / 4;
                    float spawnY = MathHelper.Lerp(CloneAttack3StartY, CloneAttack3EndY, t);

                    SoundEngine.PlaySound(ProjectileSound, NPC.Center);

                    Projectile.NewProjectile(NPC.GetSource_FromAI(),
                        new Vector2(NPC.Center.X, spawnY),
                        new Vector2(shootDirX, 0f),
                        ModContent.ProjectileType<CycloneProjectile>(),
                        10,
                        2f,
                        Main.myPlayer,
                        ai0: Main.rand.Next(4),
                        ai1: 1f);
                }
            }
        }

        return;
    }

    if (AITimer >= 360f && AITimer < 420f)
    {
        NPC.alpha    = (int)MathHelper.Lerp(140, 255, Math.Min((AITimer - 360f) / 60f, 1f));
        NPC.velocity *= 0.9f;
        return;
    }

    if (AITimer >= 420f)
        NPC.active = false;
}

private int CloneAttack4ShotsFired = 0;
private int CloneAttack4ShootTimer =0;


private void DoAttack4()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    if (AITimer == 1f)
    {
        NPC.alpha         = 0;
        NPC.velocity      = Vector2.Zero;
        CloneAttack4ShotsFired = 0;
        NPC.netUpdate     = true;
    }

    if (AITimer >= 1f && AITimer < 120f)
    {
    // Fade in
    NPC.alpha = (int)MathHelper.Lerp(255, 160, Math.Min(AITimer / 30f, 1f));

    NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
    NPC.spriteDirection = NPC.direction;

    CloneAttack4ShootTimer++;
    if (CloneAttack4ShootTimer >= 20 && CloneAttack4ShotsFired < 3
        && Main.netMode != NetmodeID.MultiplayerClient)
    {
        CloneAttack4ShootTimer = 0;
        CloneAttack4ShotsFired++;


        SoundEngine.PlaySound(ProjectileSound, NPC.Center);

        Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
        int damage       = 10;
        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir * 10f,
        ModContent.ProjectileType<CycloneProjectile>(), damage, 2f, Main.myPlayer, ai0: Main.rand.Next(4));
    }
    }

    if (AITimer >= 80f && AITimer < 140f)
    {
        NPC.alpha    = (int)MathHelper.Lerp(160, 255, Math.Min((AITimer - 80f) / 60f, 1f));
        NPC.velocity *= 0.9f;
        return;
    }

    if (AITimer >= 140f)
        NPC.active = false;
}


}


}
