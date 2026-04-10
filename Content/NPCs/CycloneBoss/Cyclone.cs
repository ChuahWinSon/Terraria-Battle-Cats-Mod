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
            CircleAndShoot,
            SansWall,
            LingeringRocks,
            MiniCyclones,
            GroundSmash,

            FinalTurn
        }

        public ref float AIState => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];
        public ref float AttackTimer => ref NPC.ai[2];
        public ref float ExtraTimer => ref NPC.ai[3];


        public static int DashSpreadDamage => 20;

        public static int AllProjectileDamage => 20;


        public override void SetDefaults()
        {
            NPC.aiStyle = -1;

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
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/CycloneBossMusic");
        }

        public bool IsPhase2 => NPC.life <= NPC.lifeMax / 2;
        private const int CloneOffset = 100; // pixels left/right, tune this
        private const int CloneAlpha = 200; // transparency, tune this

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
            SpinTimer = 0;
            ActiveFrameCount = 29; 
        }

        public override void OnSpawn(IEntitySource source)
        {
            SpinTimer = 0;
            NPC.frame.Y = 0;
            ActiveFrameCount = 29;
        }

        public override void FindFrame(int frameHeight)
        {
            Texture2D activeTex = IsRoaring ? RoarTexture.Value : TextureAssets.Npc[NPC.type].Value;
            int correctHeight = activeTex.Height / ActiveFrameCount;

            NPC.frameCounter++;
            if (NPC.frameCounter >= 4)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += correctHeight;
                if (NPC.frame.Y >= correctHeight * ActiveFrameCount)
                    NPC.frame.Y = 0;
            }
        }



        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = IsRoaring
                ? RoarTexture.Value
                : TextureAssets.Npc[NPC.type].Value;

            int correctFrameHeight = tex.Height / ActiveFrameCount;
            Rectangle frame = new Rectangle(0, NPC.frame.Y, tex.Width, correctFrameHeight);
            Vector2 origin = frame.Size() / 2f;
            Vector2 pos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
            SpriteEffects flip = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(tex, pos, frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale, flip, 0f);

            return false;

        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // If the NPC dies, spawn gore and play a sound
            if (Main.netMode == NetmodeID.Server)
            {
                // We don't want Mod.Find<ModGore> to run on servers as it will crash because gores are not loaded on servers
                return;
            }

            if (NPC.life <= 0)
            {
                // These gores work by simply existing as a texture inside any folder which path contains "Gores/"
                int backGoreType = Mod.Find<ModGore>("CycloneBossBody_Bottom").Type;
                int frontGoreType = Mod.Find<ModGore>("CycloneBossBody_Top").Type;
                int eyeGoreType = Mod.Find<ModGore>("CycloneBossEye").Type;

                var entitySource = NPC.GetSource_Death();

                for (int i = 0; i < 2; i++)
                {
                    Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), backGoreType);
                    Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), frontGoreType);

                }
                Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-3, 3), Main.rand.Next(-3, 3)), eyeGoreType);

                SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);

                if (Main.netMode != NetmodeID.Server)
                    BossCameraSystem.TriggerShake(10f); // increase for stronger shake
            }
        }

        public static Asset<Texture2D> RoarTexture;
        private int SpinTimer = 0;
        private bool IsRoaring => SpinTimer > 0;
        public override void Load()
        {
            RoarTexture = ModContent.Request<Texture2D>("TheBattleCats/Content/NPCs/CycloneBoss/Cyclone_Roar");
        }
        private int ActiveFrameCount = 29; // replace all Main.npcFrameCount[NPC.type] mutations

        private void TriggerSpin(int duration = 60)
        {
            if (Main.netMode == NetmodeID.Server) return;
            SpinTimer = duration;
            NPC.frame.Y = 0;
            ActiveFrameCount = 6;
        }

        

        public override void Unload() => RoarTexture = null;


        private float LifeRatio;

        private ActionState PreviousState = ActionState.Reset;
        public override void AI()
        {

            // makes movement look more fluid
            NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.04f, -MathHelper.Pi / 6f, MathHelper.Pi / 6f);

            if (SpinTimer > 0)
            {
                SpinTimer--;
                if (SpinTimer == 0)
                {
                    NPC.frame.Y = 0;
                    ActiveFrameCount = 29; 
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
                case (float)ActionState.CircleAndShoot:
                    DoBehavior_CircleAndShoot();
                    break;
                case (float)ActionState.SansWall:
                    DoBehavior_SansWall();
                    break;
                case (float)ActionState.LingeringRocks:
                    DoBehavior_LingeringRocks();
                    break;
                case (float)ActionState.MiniCyclones:
                    DoBehavior_SpawnMiniCyclones(player);
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
                Vector2 velocity = new Vector2(0f, 5f);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,                          // position (boss center)
                        velocity,                        // velocity (adjust as needed)
                        ModContent.ProjectileType<SplittingRock>(),
                        AllProjectileDamage,                              // your damage value
                        2f,                           // your knockback value
                        Main.myPlayer
                    );
                }

            }

            //spliting rocks
            if (LifeRatio < 0.7 && AITimer % 150 == 149)
            {
                Vector2 velocity = new Vector2(0f, 5f);
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {

                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        velocity,
                        ModContent.ProjectileType<ClusteredRock>(),
                        AllProjectileDamage,
                        2f,
                        Main.myPlayer
                    );
                }

            }



        }


        private void DoBehavior_SpawnAnimation()
        {
            // Make boss invulnerable and non-contact during entire spawn animation
            NPC.dontTakeDamage = true;

            // Disable contact damage with players
            NPC.damage = 0;

            if (AITimer == 0)
            {
                // Only trigger camera on client
                if (Main.netMode != NetmodeID.Server)
                    BossCameraSystem.StartBossPan(NPC.Center, panDuration: 90, holdDuration: 210);
            }
            AITimer++;

            if (AITimer < 120)
            {
                NPC.alpha = (int)MathHelper.Lerp(255, 0, (AITimer - 60) / 60f);
                return;
            }

            if (AITimer == 180)
            {
                SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
                TriggerSpin(60);

                if (Main.netMode != NetmodeID.Server)
                    BossCameraSystem.TriggerShake(60f); // increase for stronger shake
            }


            if (AITimer >= 300f)
            {
                AITimer = 0f;
                AIState = (float)ActionState.TripleDashAttack;
                NPC.dontTakeDamage = false;
                NPC.damage = 40;
            }
        }

        private ActionState previousAttack = ActionState.Reset;

        private void DoBehavior_ResetAI()
        {

            // reset all shared state here so individual attacks don't have to remember
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


            int localFrame = (int)AITimer - 1;


            // --- Dash 1: DOWN ---
            if (localFrame == 1) // windup done, launch
            {
                Vector2 direction = Vector2.Normalize(player.Center - NPC.Center); // aim at player
                NPC.velocity = direction * 24f;

                SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
                TriggerSpin(30);
            }

            // --- Dash 2: UP (back above player) ---
            if (localFrame == 45)
            {
                Vector2 direction = Vector2.Normalize(player.Center - NPC.Center); // aim at player
                NPC.velocity = direction * 24f;

                SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
                TriggerSpin(30);
            }

            // --- Dash 3: DOWN + projectiles ---
            if (localFrame == 105)
            {
                // Shoot 5 projectiles in a spread BEFORE the dash
                ShootSpreadProjectiles(Main.player[NPC.target]);

                SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
                TriggerSpin(50);
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
                float lerpT = spreadCount == 1 ? 0.5f : i / (float)(spreadCount - 1);
                float rotation = MathHelper.Lerp(-spreadAngle / 2f, spreadAngle / 2f, lerpT);

                Vector2 velocity = baseDirection.RotatedBy(rotation) * projSpeed;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(
                        NPC.GetSource_FromAI(),
                        NPC.Center,
                        velocity,
                        ModContent.ProjectileType<CycloneProjectile>(),
                        AllProjectileDamage,
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
                TriggerSpin(30);

                if (Main.netMode != NetmodeID.Server)
                    BossCameraSystem.TriggerShake(10f); // increase for stronger shake
                ShootLingeringRockProjectiles();
            }

            if (AITimer >= 151f)
            {
                AITimer = 0f;
                AIState = (float)ActionState.Reset;
                NPC.netUpdate = true;
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
                    AllProjectileDamage,
                    2f,
                    Main.myPlayer
                );
            }
        }


        // orbit state variables
        private float CircleAndShootOrbitRadius = 400f;
        private const float CircleAndShootOrbitSpeed = 0.02f;

        private void DoBehavior_CircleAndShoot()
        {
            Player player = Main.player[NPC.target];
            AITimer++;

            // Phase 1: move to nearest orbit point (0-60 ticks)
            if (AITimer == 1f)
            {
                // Calculate the angle from player to NPC, snap to that orbit point
                ExtraTimer = (NPC.Center - player.Center).ToRotation(); // Orbit Angle
                NPC.netUpdate = true;
            }

            if (AITimer <= 60f)
            {
                Vector2 targetPos = player.Center + new Vector2(CircleAndShootOrbitRadius, 0f).RotatedBy(ExtraTimer);
                float dist = Vector2.Distance(NPC.Center, targetPos);
                NPC.velocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(dist * 0.1f, 2f, 20f);
                return;
            }

            if (AITimer == 61f && LifeRatio < 0.7f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float clone1Angle = ExtraTimer + MathHelper.Pi;
                    Vector2 clone1Pos = player.Center + new Vector2(CircleAndShootOrbitRadius, 0f).RotatedBy(clone1Angle);

                    int clone1Index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clone1Pos.X, (int)clone1Pos.Y, ModContent.NPCType<CycloneClone>());
                    Main.npc[clone1Index].ai[1] = 2f;
                    Main.npc[clone1Index].ai[2] = clone1Angle;
                    Main.npc[clone1Index].ai[3] = CircleAndShootOrbitRadius;
                    Main.npc[clone1Index].netUpdate = true;
                }
            }

            // Phase 2: orbit continuously while shooting

            if (AITimer >= 61f && AITimer < 420f)
            {

                ExtraTimer += CircleAndShootOrbitSpeed;

                Vector2 targetPos = player.Center + new Vector2(CircleAndShootOrbitRadius, 0f).RotatedBy(ExtraTimer);
                Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.10f);

                NPC.direction = NPC.Center.X < player.Center.X ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                AttackTimer++;
                if (AttackTimer >= 50)
                {
                    AttackTimer = 0;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);

                        Projectile.NewProjectile(
                            NPC.GetSource_FromAI(),
                            NPC.Center,
                            shootDir * 12f,
                            ModContent.ProjectileType<CycloneProjectile>(),
                            AllProjectileDamage,
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
                NPC.alpha = 0;
                AITimer = 0f;
                ExtraTimer = 0f;
                AttackTimer = 0;


                AIState = (float)ActionState.Reset;

                NPC.netUpdate = true;
            }
        }

        private float SansWallOrbitRadius = 400f;
        private const float SansWallHeight = 160f; // total spread height of the 5 bullets

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
                    float cloneOffset = bossSide * SansWallOrbitRadius;

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

                float xOffset = NPC.Center.X < player.Center.X ? -SansWallOrbitRadius : SansWallOrbitRadius;
                Vector2 targetPos = new Vector2(player.Center.X + xOffset, player.Center.Y + verticalOffset);
                Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

                AttackTimer++;
                if (AttackTimer >= 70)
                {
                    AttackTimer = 0;
                    AttackTimer = Main.rand.Next(3);
                    FireWallVolley(player);
                }

                return;
            }


            if (AITimer >= 420f)
            {
                NPC.alpha = 0;
                AttackTimer = 0;
                AITimer = 0f;
                AIState = (float)ActionState.Reset;

                NPC.netUpdate = true;
            }
        }



        private void FireWallVolley(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            int bulletCount = 5;
            float speed = 4f;
            float direction = NPC.Center.X < player.Center.X ? 1f : -1f;

            for (int i = 0; i < bulletCount; i++)
            {
                float t = (float)i / (bulletCount - 1);
                float spawnY = MathHelper.Lerp(player.Center.Y - SansWallHeight / 2f, player.Center.Y + SansWallHeight / 2f, t);

                Vector2 spawnPos = new Vector2(NPC.Center.X, spawnY);
                int variant = Main.rand.Next(4);

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    spawnPos,
                    new Vector2(speed * direction, 0f),
                    ModContent.ProjectileType<CycloneProjectile>(),
                    AllProjectileDamage,
                    2f,
                    Main.myPlayer,
                    ai0: variant,
                    ai1: 1f
                );
            }
        }


        private void DoBehavior_GroundSmash()
        {
            Player player = Main.player[NPC.target];


            // ── Phase 1: reposition above player (negative timer ramp-up, same pattern as TripleDash) ──
            if (AITimer <= 0)
            {
                Vector2 targetPos = player.Center + new Vector2(0f, -280f);
                Vector2 toTarget = targetPos - NPC.Center;
                float dist = toTarget.Length();
                float rampFrames = -AITimer;
                float maxSpeed = MathHelper.Clamp(rampFrames * 0.3f, 20f, 40f);
                Vector2 ideal = Vector2.Normalize(toTarget) * Math.Min(dist * 0.08f, maxSpeed);
                NPC.velocity = Vector2.Lerp(NPC.velocity, ideal, 0.07f);
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
                NPC.velocity = Vector2.Zero;
                NPC.spriteDirection = NPC.Center.X < player.Center.X ? 1 : -1;

                if (AITimer == 10f)
                {
                    SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
                    TriggerSpin(40);
                    if (Main.netMode != NetmodeID.Server)
                        BossCameraSystem.TriggerShake(6f);
                }
                return;
            }

            float AllowContactTimer = AITimer - 40;
            // ── Phase 3: dash downward & tile-check every frame ──
            if (ExtraTimer == 0f) //extratimer is being used as a check here for hit/not hit
            {
                NPC.velocity = new Vector2(0f, 30f);

                if (AllowContactTimer <= 6f)
                    return;

                // Check the tile directly under the boss centre
                int tileX = (int)((NPC.Center.X) / 16f);
                int tileY = (int)((NPC.Bottom.Y) / 16f);      // bottom edge
                int tileYSafeGuard = ((int)((NPC.Bottom.Y) / 16f)) - 1;      // prevent phase through blocks

                bool hitTile = IsSolidOrPlatform(tileX, tileY) || IsSolidOrPlatform(tileX, tileYSafeGuard);

                bool timedOut = AITimer >= 160f;

                if (hitTile || timedOut)
                {
                    ExtraTimer = 1f;
                    NPC.velocity = Vector2.Zero;

                    if (Main.netMode != NetmodeID.Server)
                        BossCameraSystem.TriggerShake(14f);

                    SoundEngine.PlaySound(CycloneSlam, NPC.Center);

                    // Determine which tileY actually detected the block
                    int effectiveTileY = IsSolidOrPlatform(tileX, tileY) ? tileY : tileYSafeGuard;


                    // ── Rock spawn: check ±25, ±50, ±75 tile offsets ──
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {

                        int[] offsets = { -75, -50, -25, 25, 50, 75 };

                        foreach (int tileOffsetX in offsets)
                        {
                            int checkX = tileX + tileOffsetX;

                            // Walk down from the boss Y to find the surface at this X
                            int surfaceY = effectiveTileY;
                            for (int scanY = effectiveTileY; scanY < effectiveTileY + 40; scanY++)
                            {
                                if (IsSolidOrPlatform(checkX, scanY))
                                {
                                    surfaceY = scanY;
                                    break;
                                }
                            }

                            if (IsSolidOrPlatform(checkX, surfaceY))
                            {
                                // Spawn rock just above the solid tile
                                Vector2 rockPos = new Vector2(
                                    checkX * 16f + 8f,
                                    surfaceY * 16f - 8f
                                );

                                // Velocity: arc upward, slight outward spread from centre
                                float horizontalDir = tileOffsetX > 0 ? 1f : -1f;
                                float horizontalSpeed = Math.Abs(tileOffsetX) / 25f * 2.5f; // further = more spread
                                Vector2 rockVel = new Vector2(horizontalDir * horizontalSpeed, -8f);

                                Projectile.NewProjectile(
                                    NPC.GetSource_FromAI(),
                                    rockPos,
                                    rockVel,
                                    ModContent.ProjectileType<ClusteredRock>(),
                                    AllProjectileDamage,
                                    2f,
                                    Main.myPlayer
                                );
                            }
                        }
                    }
                }
                return;
            }


            if (AITimer >= 120f)
            {
                ExtraTimer = 0f;
                AITimer = 0f;
                AIState = (float)ActionState.Reset;
                NPC.netUpdate = true;
            }
        }

        private static bool IsSolidOrPlatform(int x, int y)
        {
            Tile tile = Framing.GetTileSafely(x, y);
            return tile.HasTile && (Main.tileSolid[tile.TileType] || TileID.Sets.Platforms[tile.TileType]);
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
                float angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                float radius = 400f;

                Vector2 targetPos = player.Center + radius * new Vector2(
                    (float)Math.Cos(angle),
                    (float)Math.Sin(angle)
                );

                CreateTeleportTelegraph(targetPos);

                NPC.Center = targetPos;
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
                NPC.direction = NPC.Center.X < player.Center.X ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                // Lock in dash direction once near the launch frame
                if (AITimer == 55f)
                {
                    DashDirection = Vector2.Normalize(player.Center - NPC.Center);
                    SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
                    NPC.netUpdate = true;
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
                DashDirection = Vector2.Zero;
                AIState = (float)ActionState.FinalTurn;
                NPC.alpha = 0; // fully visible again
                // transition to next AI state here
            }
        }




        public static void CreateTeleportTelegraph(Vector2 teleportPosition, float cloudOpacity = 1f)
        {
            CloudParticle noxiousCloud = new(teleportPosition, Vector2.Zero, Color.White * cloudOpacity, Color.DarkGray, 120, Main.rand.NextFloat(1.4f, 1.8f));
            ParticleHandler.SpawnParticle(noxiousCloud);

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
                        AllProjectileDamage,
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
                        AllProjectileDamage,
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

                NPC.netUpdate = true;
            }
        }




        #region Networking
        public override void SendExtraAI(System.IO.BinaryWriter writer)
        {

            writer.Write(DashDirection.X);
            writer.Write(DashDirection.Y);
            writer.Write((int)previousAttack); // cast enum to int
        }

        public override void ReceiveExtraAI(System.IO.BinaryReader reader)
        {

            DashDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            previousAttack = (ActionState)reader.ReadInt32(); // cast int back to enum

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
        private const int TargetAlpha = 200; // semi transparent, lower = more visible

        private static readonly SoundStyle ProjectileSound = new SoundStyle("TheBattleCats/Assets/Boss/Cyclone/CycloneProjectile")
        {
            PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
        };

        public override void SetDefaults()
        {
            NPC.width = 110;
            NPC.height = 110;
            NPC.damage = 1;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.alpha = 255; // start invisible
            NPC.dontTakeDamage = true;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 29;

        }

        public static int AllProjectileDamage => 20;


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

        private int Attack2ShootTimer = 0;
        private float OrbitRadius2 = 400f;
        private const float OrbitSpeed2 = 0.02f;
        private float Attack2OrbitAngle = 0f;

        public override void AI()
        {
            Player player = Main.player[NPC.target];
            NPC.TargetClosest(true);


            switch ((int)NPC.ai[1])
            {
                case 2: DoAttack2(); break;
                case 3: DoAttack3(); break;
            }


        }






        private void DoAttack2()
        {
            Player player = Main.player[NPC.target];
            AITimer++;
            if (AITimer == 1f)
            {
                Attack2OrbitAngle = NPC.ai[2]; // use angle passed from boss
                NPC.alpha = 255;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
            }

            // Orbit and shoot
            if (AITimer >= 1f && AITimer < 360f)
            {
                NPC.alpha = (int)MathHelper.Lerp(255, 140, Math.Min(AITimer / 60f, 1f));
                NPC.direction = NPC.Center.X < player.Center.X ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                Attack2OrbitAngle += OrbitSpeed2;

                Vector2 targetPos = player.Center + new Vector2(OrbitRadius2, 0f).RotatedBy(Attack2OrbitAngle);
                Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.10f);

                NPC.direction = NPC.Center.X < player.Center.X ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                Attack2ShootTimer++;
                if (Attack2ShootTimer >= 50 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Attack2ShootTimer = 0;

                    SoundEngine.PlaySound(ProjectileSound, NPC.Center);

                    Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);


                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir * 12f,
                    ModContent.ProjectileType<CycloneProjectile>(), AllProjectileDamage, 2f, Main.myPlayer, ai0: Main.rand.Next(4));
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


        private int CloneAttack3ShootTimer = 0;
        private float CloneAttack3StartY = 0f;
        private float CloneAttack3EndY = 0f;
        private const float CloneAttack3Height = 160f;

        private void DoAttack3()
        {
            Player player = Main.player[NPC.target];
            AITimer++;

            if (AITimer == 1f)
            {
                NPC.Center = player.Center + new Vector2(NPC.ai[2], 0f);
                NPC.alpha = 255;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
            }

            if (AITimer >= 1f && AITimer < 360f)
            {
                NPC.alpha = (int)MathHelper.Lerp(255, 140, Math.Min(AITimer / 60f, 1f));

                float verticalOffset = 0f;
                float yDiff = NPC.Center.Y - player.Center.Y;
                if (Math.Abs(yDiff) < 20f)
                    verticalOffset = yDiff == 0f ? (NPC.whoAmI % 2 == 0 ? 40f : -40f) : Math.Sign(yDiff) * 40f;

                Vector2 targetPos = new Vector2(player.Center.X + NPC.ai[2], player.Center.Y + verticalOffset);
                Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

                // Face toward player based on which side the clone is on
                int facingDir = NPC.ai[2] > 0f ? -1 : 1; // right side → face left, left side → face right
                NPC.direction = facingDir;
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
                            CloneAttack3EndY = player.Center.Y + CloneAttack3Height / 2f;
                            break;
                        case 1:
                            CloneAttack3StartY = player.Center.Y - CloneAttack3Height;
                            CloneAttack3EndY = player.Center.Y;
                            break;
                        case 2:
                            CloneAttack3StartY = player.Center.Y;
                            CloneAttack3EndY = player.Center.Y + CloneAttack3Height;
                            break;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Fire toward player: if clone is on the right, shoot left (-1); if on the left, shoot right (+1)
                        float shootDirX = NPC.ai[2] > 0f ? -4f : 4f;

                        for (int i = 0; i < 5; i++)
                        {
                            float t = (float)i / 4;
                            float spawnY = MathHelper.Lerp(CloneAttack3StartY, CloneAttack3EndY, t);

                            SoundEngine.PlaySound(ProjectileSound, NPC.Center);

                            Projectile.NewProjectile(NPC.GetSource_FromAI(),
                                new Vector2(NPC.Center.X, spawnY),
                                new Vector2(shootDirX, 0f),
                                ModContent.ProjectileType<CycloneProjectile>(),
                                AllProjectileDamage,
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
                NPC.alpha = (int)MathHelper.Lerp(140, 255, Math.Min((AITimer - 360f) / 60f, 1f));
                NPC.velocity *= 0.9f;
                return;
            }

            if (AITimer >= 420f)
                NPC.active = false;
        }






    }


}
