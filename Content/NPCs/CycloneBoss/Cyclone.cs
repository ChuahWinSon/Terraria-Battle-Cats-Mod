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
            
            Idle,
            TripleDashAttack,
            Dash,
            CircleAndShoot,
            SansWall,
            FallingRocks,
            EnhancedAttack1,
            EnhancedAttack2,
            EnhancedAttack3,
            EnhancedAttack4
        }

        public ref float AIState => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];
        public ref float AttackTimer => ref NPC.ai[2];
        public ref float CloneTimer => ref NPC.ai[3];
        

        

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
            NPC.width = 110;
            NPC.height = 110;
            NPC.boss = true;
        }

        public bool IsPhase2 => NPC.life <= NPC.lifeMax / 2;
        private const int CloneOffset = 100; // pixels left/right, tune this
        private const int CloneAlpha  = 200; // transparency, tune this



        private static readonly SoundStyle ProjectileSound = new SoundStyle("TheBattleCats/Assets/Effects/CycloneProjectile")
        {
            PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
        };

        private static readonly SoundStyle CycloneRoarGor = new SoundStyle("TheBattleCats/Assets/Effects/CycloneRoarGor")
        {
            PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
        };

        private static readonly SoundStyle CycloneRoarDrag = new SoundStyle("TheBattleCats/Assets/Effects/CycloneRoarDrag")
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
                case (float)ActionState.Idle:
                    Idle();
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

        private void Idle()
        {
            // hover in place, transition to Attack1 after a delay
            AITimer++;
            NPC.velocity *= 0.95f;

            if (AITimer >= 120f)
            {
                AITimer = 0f;
                AIState = (float)ActionState.SansWall;
            }
        }

        private ActionState previousAttack = ActionState.Reset;

        

        private void DoBehavior_ResetAI()
        {

            // reset all shared state here so individual attacks don't have to remember
            OrbitAngle = 0f;
            LaunchDirection = Vector2.Zero;
            LaunchTimer = 0f;

            
            NPC.TargetClosest(false);
            NPC.velocity *= 0.95f;

            ActionState nextAttack;
            do
            {
                if (LifeRatio > 0.62f)
                {
                    nextAttack = Main.rand.Next(4) switch
                    {
                        0 => ActionState.Dash,
                        1 => ActionState.CircleAndShoot,
                        2 => ActionState.SansWall,
                        _ => ActionState.FallingRocks,
                    };
                }
                else
                {
                    nextAttack = Main.rand.Next(4) switch
                    {
                        0 => ActionState.EnhancedAttack1,
                        1 => ActionState.EnhancedAttack2,
                        2 => ActionState.EnhancedAttack3,
                        _ => ActionState.EnhancedAttack4,
                    };  
                }
            }

            while (nextAttack == previousAttack); // never repeat the same attack twice


            // ActionState nextAttack = ActionState.TripleDashAttack; //testing


            previousAttack = nextAttack;
            AIState = (float)nextAttack;
            AITimer = 0f;
            NPC.netUpdate = true;
        }


private int dashCount = 0;
private Vector2 dashTarget;

private void DoBehavior_TripleDashAttack()
{
    AITimer++;

    Player player = Main.player[NPC.target];

    

    // if (AITimer <= 60)
    // {

    //     return;
    // }

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
        dashCount = 0;
    }
    return;
}

    // ─── Phase 2: Triple dash sequence ────────────────────────────────────
    // Dash timing layout (relative to attackTimer):
    //   Frame 31–50  → windup pause before dash 1
    //   Frame 51     → Dash 1 fires (DOWN)
    //   Frame 52–80  → dash 1 travel / decelerate
    //   Frame 81–95  → pause before dash 2
    //   Frame 96     → Dash 2 fires (UP, back to position above player)
    //   Frame 97–125 → dash 2 travel / decelerate
    //   Frame 126–140→ pause before dash 3 + shoot projectiles
    //   Frame 141    → Dash 3 fires (DOWN, with projectiles)
    //   Frame 142–170→ dash 3 travel / decelerate
    //   Frame 171+   → reset / transition to next attack

    int localFrame = (int)AITimer - 120;


    // --- Dash 1: DOWN ---
    if (localFrame == 1) // windup done, launch
    {
        dashCount = 1;
        Vector2 direction = Vector2.Normalize(player.Center - NPC.Center); // aim at player
        NPC.velocity = direction * 20f;

        SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
TriggerRoar(30);
    }

    // --- Dash 2: UP (back above player) ---
    if (localFrame == 60)
    {
        dashCount = 2;
        Vector2 returnPos = player.Center + new Vector2(0f, -400f); // above player
        Vector2 direction = Vector2.Normalize(returnPos - NPC.Center);
        NPC.velocity = direction * 20f;

        SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
TriggerRoar(30);
    }

    // --- Dash 3: DOWN + projectiles ---
    if (localFrame == 120)
    {
        // Shoot 5 projectiles in a spread BEFORE the dash
        ShootSpreadProjectiles(Main.player[NPC.target]);

        SoundEngine.PlaySound(CycloneRoarDrag, NPC.Center);
TriggerRoar(50);
    }

    if (localFrame == 121)
    {
        dashCount = 3;
        Vector2 direction = Vector2.Normalize(player.Center - NPC.Center); // aim at player again
        NPC.velocity = direction * 20f;
    }

    // --- Decelerate after each dash launch ---
    if (dashCount > 0)
    {
        NPC.velocity *= 0.96f; // friction; tweak for feel
    }

    // --- Reset after full sequence ---
    if (localFrame >= 241)
    {
        AITimer = 0f;
        dashCount = 0;
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

    int projDamage  = 40;
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
                projDamage,
                2f,        // knockback
                Main.myPlayer,
                ai0: Main.rand.Next(4)
            );
        }
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
                AIState = (float)ActionState.TripleDashAttack;

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

    // Phase 1: disappear (0-60 ticks)
    if (AITimer < 60f)
    {
        NPC.velocity = Vector2.Zero;
        NPC.alpha    = (int)MathHelper.Lerp(0, 255, AITimer / 60f);
        return;
        
    }

    // Phase 2: teleport to random position around player
    if (AITimer == 61f)
    {
        Attack2OrbitAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
        NPC.Center        = player.Center + new Vector2(OrbitRadius2, 0f).RotatedBy(Attack2OrbitAngle);
        NPC.alpha         = 255;
        NPC.velocity      = Vector2.Zero;
        NPC.netUpdate     = true;
    }

    // Phase 3: orbit continuously while shooting
    if (AITimer >= 61f && AITimer < 360f)
    {
        // Fade in over first 60 ticks of orbit
        NPC.alpha = (int)MathHelper.Lerp(255, 0, Math.Min((AITimer - 61f) / 60f, 1f));

        Attack2OrbitAngle += OrbitSpeed2;

        Vector2 targetPos     = player.Center + new Vector2(OrbitRadius2, 0f).RotatedBy(Attack2OrbitAngle);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.10f);

        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;

        // Shoot every 
        Attack2ShootTimer++;
        if (Attack2ShootTimer >= 50)
        {
            Attack2ShootTimer = 0;

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
            }
        }

        return;
    }

    // Phase 4: fade out and end
    if (AITimer >= 360f && AITimer < 420f)
    {
        NPC.alpha = (int)MathHelper.Lerp(0, 255, (AITimer - 360f) / 60f);
        NPC.velocity *= 0.9f;
        return;
    }

    if (AITimer >= 420f)
    {
        NPC.alpha         = 0;
        AITimer           = 0f;
        Attack2OrbitAngle = 0f;
        Attack2ShootTimer = 0;
        AIState = (float)ActionState.TripleDashAttack;

    }
}

private float OrbitRadius3    = 700f;
private int   Attack3ShootTimer = 0;
private const float Attack3Height = 160f; // total spread height of the 5 bullets
private float Attack3SubAttack = 0f;
private float Attack3StartY    = 0f;
private float Attack3EndY      = 0f;

private void DoBehavior_SansWall()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    // Phase 1: disappear
    if (AITimer < 60f)
    {
        NPC.velocity = Vector2.Zero;
        NPC.alpha    = (int)MathHelper.Lerp(0, 255, AITimer / 60f);
        return;
    }

    // Phase 2: teleport to left of player
    if (AITimer == 61f)
    {
        NPC.Center    = player.Center + new Vector2(-OrbitRadius3, 0f);
        NPC.alpha     = 255;
        NPC.velocity  = Vector2.Zero;
        NPC.netUpdate = true;
    }

    // Phase 3: track player on left side and fire volleys
    if (AITimer >= 61f && AITimer < 420f)
    {
        NPC.alpha = (int)MathHelper.Lerp(255, 0, Math.Min((AITimer - 61f) / 60f, 1f));

        float verticalOffset = 0f;
        float yDiff = NPC.Center.Y - player.Center.Y;
        if (Math.Abs(yDiff) < 20f)
            verticalOffset = yDiff == 0f ? (NPC.whoAmI % 2 == 0 ? 40f : -40f) : Math.Sign(yDiff) * 40f;

        Vector2 targetPos     = new Vector2(player.Center.X - OrbitRadius3, player.Center.Y + verticalOffset);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        NPC.direction       = 1;
        NPC.spriteDirection = 1;

        Attack3ShootTimer++;
        if (Attack3ShootTimer >= 60)
        {
            Attack3ShootTimer = 0;
            Attack3SubAttack  = Main.rand.Next(3);
            PickAttack3Range(player);
            FireWallVolley(player);
        }

        return;
    }

    // Phase 4: fade out
    if (AITimer >= 420f && AITimer < 480f)
    {
        NPC.alpha    = (int)MathHelper.Lerp(0, 255, Math.Min((AITimer - 420f) / 60f, 1f));
        NPC.velocity *= 0.9f;
        return;
    }

    if (AITimer >= 480f)
    {
        NPC.alpha  = 0;
        AITimer    = 0f;
        AIState = (float)ActionState.TripleDashAttack;

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
    float speed       = 10f;
    int   damage      = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

    for (int i = 0; i < bulletCount; i++)
    {
        float t      = (float)i / (bulletCount - 1); // 0 to 1
        float spawnY = MathHelper.Lerp(Attack3StartY, Attack3EndY, t);

        Vector2 spawnPos = new Vector2(NPC.Center.X, spawnY);

        int variant = Main.rand.Next(4);

        Projectile.NewProjectile(
        NPC.GetSource_FromAI(),
        spawnPos,
        new Vector2(speed, 0f),
        ModContent.ProjectileType<CycloneProjectile>(),
        damage,
        2f,
        Main.myPlayer,
        ai0: variant
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
                int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);
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
                AIState = (float)ActionState.TripleDashAttack;

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
    int projDamage  = 20;

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

    CloneTimer++;
    
    if (CloneTimer < 700)
    {
        if (CloneTimer % 121 == 61 && Main.netMode != NetmodeID.MultiplayerClient)
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

        if (CloneTimer % 121 == 91 && CloneTimer > 2 && Main.netMode != NetmodeID.MultiplayerClient)
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
            CloneTimer = 0;


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


private float EnhancedOrbitRadius2      = 400f;
private const float EnhancedOrbitSpeed2 = 0.02f;

private void EnhancedAttack2()
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
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            OrbitAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
            NPC.netUpdate = true; // This triggers SendExtraAI
        }
        NPC.Center                = player.Center + new Vector2(EnhancedOrbitRadius2, 0f).RotatedBy(OrbitAngle);
        NPC.alpha                 = 255;
        NPC.velocity              = Vector2.Zero;
        NPC.netUpdate             = true;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            float clone1Angle = OrbitAngle + MathHelper.Pi;
            Vector2 clone1Pos = player.Center + new Vector2(EnhancedOrbitRadius2, 0f).RotatedBy(clone1Angle);

            int clone1Index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clone1Pos.X, (int)clone1Pos.Y, ModContent.NPCType<CycloneClone>());
            Main.npc[clone1Index].ai[1] = 2f;
            Main.npc[clone1Index].ai[2] = clone1Angle;
            Main.npc[clone1Index].ai[3] = EnhancedOrbitRadius2;
            Main.npc[clone1Index].netUpdate = true;
        }
    }

    if (AITimer >= 61f && AITimer < 540f)
    {
        NPC.alpha = (int)MathHelper.Lerp(255, 0, Math.Min((AITimer - 61f) / 60f, 1f));

        OrbitAngle += EnhancedOrbitSpeed2;

        Vector2 targetPos     = player.Center + new Vector2(EnhancedOrbitRadius2, 0f).RotatedBy(OrbitAngle);
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

        return;
    }


    if (AITimer >= 540f)
    {
        NPC.alpha                 = 0;
        AITimer                   = 0f;
        OrbitAngle = 0f;
        AttackTimer = 0;


        AIState    = (float)ActionState.Reset;

        NPC.netUpdate = true;
    }
}

private float EnhancedOrbitRadius3    = 600f;
private const float EnhancedAttack3Height = 160f;
private float EnhancedAttack3SubAttack = 0f;
private float EnhancedAttack3StartY    = 0f;
private float EnhancedAttack3EndY      = 0f;

private void EnhancedAttack3()
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
        NPC.Center    = player.Center + new Vector2(-EnhancedOrbitRadius3, 0f);
        NPC.alpha     = 255;
        NPC.velocity  = Vector2.Zero;
        NPC.netUpdate = true;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 clonePos = player.Center + new Vector2(EnhancedOrbitRadius3, 0f); // right side
            int cloneIndex   = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clonePos.X, (int)clonePos.Y, ModContent.NPCType<CycloneClone>());
            Main.npc[cloneIndex].ai[1] = 3f; // mode 3 = mirrored attack3
            Main.npc[cloneIndex].ai[2] = EnhancedOrbitRadius3;
            Main.npc[cloneIndex].netUpdate = true;
        }

    }

    if (AITimer >= 61f && AITimer < 600f)
    {

        
        NPC.alpha = (int)MathHelper.Lerp(255, 0, Math.Min((AITimer - 61f) / 60f, 1f));

        float verticalOffset = 0f;
        float yDiff = NPC.Center.Y - player.Center.Y;
        if (Math.Abs(yDiff) < 20f)
            verticalOffset = yDiff == 0f ? (NPC.whoAmI % 2 == 0 ? 40f : -40f) : Math.Sign(yDiff) * 40f;

        Vector2 targetPos     = new Vector2(player.Center.X - EnhancedOrbitRadius3, player.Center.Y + verticalOffset);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        NPC.direction       = 1;
        NPC.spriteDirection = 1;

        AttackTimer++;
        if (AttackTimer >= 70)
        {
            AttackTimer = 0;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                EnhancedAttack3SubAttack = Main.rand.Next(3);
                NPC.netUpdate = true;
            }
            PickEnhancedAttack3Range(player);
            FireEnhancedWallVolley(player);
        }

        return;
    }

    if (AITimer >= 600f)
    {
        NPC.alpha  = 0;
        AITimer    = 0f;
        AttackTimer   = 0;
        AIState    = (float)ActionState.Reset;

        NPC.netUpdate = true;
    }
}

private void PickEnhancedAttack3Range(Player player)
{
    switch ((int)EnhancedAttack3SubAttack)
    {
        case 0:
            EnhancedAttack3StartY = player.Center.Y - EnhancedAttack3Height / 2f;
            EnhancedAttack3EndY   = player.Center.Y + EnhancedAttack3Height / 2f;
            break;
        case 1:
            EnhancedAttack3StartY = player.Center.Y - EnhancedAttack3Height;
            EnhancedAttack3EndY   = player.Center.Y;
            break;
        case 2:
            EnhancedAttack3StartY = player.Center.Y;
            EnhancedAttack3EndY   = player.Center.Y + EnhancedAttack3Height;
            break;
    }
}

private void FireEnhancedWallVolley(Player player)
{
    if (Main.netMode == NetmodeID.MultiplayerClient) return;

    int   bulletCount = 5;
    float speed       = 8f;
    int   damage      = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

    for (int i = 0; i < bulletCount; i++)
    {
        float t      = (float)i / (bulletCount - 1);
        float spawnY = MathHelper.Lerp(EnhancedAttack3StartY, EnhancedAttack3EndY, t);

        Vector2 spawnPos = new Vector2(NPC.Center.X, spawnY);

        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, new Vector2(speed, 0f),
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


        CloneTimer++;
        

        // Spawn next clone 
        if (CloneTimer % 210f == 200 && CloneTimer < 850 && CloneTimer > 2) //spawn 4 , and prevent insta spawn
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
            CloneTimer      = 0f;


            AIState    = (float)ActionState.Reset;

            NPC.netUpdate = true;
            }
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


    writer.Write(EnhancedAttack3SubAttack);

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


    EnhancedAttack3SubAttack = reader.ReadSingle();

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

    private static readonly SoundStyle ProjectileSound = new SoundStyle("TheBattleCats/Assets/Effects/CycloneProjectile")
    {
        PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
    };

    public override void SetDefaults()
    {
        NPC.width         = 110;
        NPC.height        = 110;
        NPC.damage        = 30;
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
    if (AITimer >= 1f && AITimer < 480f)
    {   
        NPC.alpha = (int)MathHelper.Lerp(255, 200, Math.Min(AITimer / 60f, 1f));
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
            int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);


            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir * 10f,
            ModContent.ProjectileType<CycloneProjectile>(), damage, 2f, Main.myPlayer, ai0: Main.rand.Next(4));
        }

        return;
    }

    // Fade out
    if (AITimer >= 480f && AITimer < 540f)
    {
        NPC.alpha = (int)MathHelper.Lerp(200, 255, Math.Min((AITimer - 480f) / 60f, 1f));
        NPC.velocity *= 0.9f;
        return;
    }

    if (AITimer >= 540f)
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
        NPC.Center    = player.Center + new Vector2(NPC.ai[2], 0f); // right side
        NPC.alpha     = 255;
        NPC.velocity  = Vector2.Zero;
        NPC.netUpdate = true;
    }

    if (AITimer >= 1f && AITimer < 540f)
    {
        NPC.alpha = (int)MathHelper.Lerp(255, 200, Math.Min(AITimer / 60f, 1f));

        float verticalOffset = 0f;
        float yDiff = NPC.Center.Y - player.Center.Y;
        if (Math.Abs(yDiff) < 20f)
            verticalOffset = yDiff == 0f ? (NPC.whoAmI % 2 == 0 ? 40f : -40f) : Math.Sign(yDiff) * 40f;

        // Right side — positive X offset
        Vector2 targetPos     = new Vector2(player.Center.X + NPC.ai[2], player.Center.Y + verticalOffset);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        NPC.direction       = -1; // face left toward player
        NPC.spriteDirection = -1;

        CloneAttack3ShootTimer++;
        if (CloneAttack3ShootTimer >= 70)
        {
            CloneAttack3ShootTimer = 0;

            // Pick random range same as boss
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

            // Fire left toward player
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 5; i++)
                {
                    float t      = (float)i / 4;
                    float spawnY = MathHelper.Lerp(CloneAttack3StartY, CloneAttack3EndY, t);

                    SoundEngine.PlaySound(ProjectileSound, NPC.Center);

                    Projectile.NewProjectile(NPC.GetSource_FromAI(),
                        new Vector2(NPC.Center.X, spawnY),
                        new Vector2(-8f, 0f),
                        ModContent.ProjectileType<CycloneProjectile>(),
                        NPC.GetAttackDamage_ForProjectiles(30f, 20f),
                        2f, Main.myPlayer, ai0: Main.rand.Next(4));
                }
            }
        }

        return;
    }

    if (AITimer >= 540f && AITimer < 600f)
    {
        NPC.alpha    = (int)MathHelper.Lerp(200, 255, Math.Min((AITimer - 540f) / 60f, 1f));
        NPC.velocity *= 0.9f;
        return;
    }

    if (AITimer >= 600f)
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
        int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);
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
