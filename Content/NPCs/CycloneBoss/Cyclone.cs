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

namespace TheBattleCats.Content.NPCs.CycloneBoss
{
    [AutoloadBossHead]
    public class Cyclone : ModNPC
    {
     
        
        private enum ActionState
        {
            Idle,
            Reset,
            Attack1,
            Attack2,
            Attack3,
            Attack4,
            EnhancedAttack1,
            EnhancedAttack2,
            EnhancedAttack3,
            EnhancedAttack4
        }

        public ref float AIState => ref NPC.ai[0];
        public ref float AITimer => ref NPC.ai[1];
        public ref float AttackTimer => ref NPC.ai[2];
        public ref float CloneTimer => ref NPC.ai[3];
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
            NPC.width = 120;
            NPC.height = 120;
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



        private float LifeRatio;

        public override void AI()
        {
        

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
                case (float)ActionState.Idle:
                    Idle();
                    break;
                case (float)ActionState.Reset:
                    DoBehavior_ResetAI();
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

        private void Idle()
        {
            // hover in place, transition to Attack1 after a delay
            AITimer++;
            NPC.velocity *= 0.95f;

            if (AITimer >= 120f)
            {
                AITimer = 0f;
                AIState = (float)ActionState.Attack3;
            }
        }

        private ActionState previousAttack = ActionState.Idle;

        

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
                        0 => ActionState.Attack1,
                        1 => ActionState.Attack2,
                        2 => ActionState.Attack3,
                        _ => ActionState.Attack4,
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

            previousAttack = nextAttack;
            AIState = (float)nextAttack;
            AITimer = 0f;
            NPC.netUpdate = true;
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

private void Attack2()
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
                int projType = Main.rand.Next(4) switch
                {
                    0 => ModContent.ProjectileType<CycloneProjectile1>(),
                    1 => ModContent.ProjectileType<CycloneProjectile2>(),
                    2 => ModContent.ProjectileType<CycloneProjectile3>(),
                    _ => ModContent.ProjectileType<CycloneProjectile4>(),
                };

                Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
                int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    shootDir * 10f,
                    projType,
                    damage,
                    2f,
                    Main.myPlayer
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
        AIState = (float)ActionState.Reset;

    }
}

private float OrbitRadius3    = 700f;
private int   Attack3ShootTimer = 0;
private const float Attack3Height = 160f; // total spread height of the 5 bullets
private float Attack3SubAttack = 0f;
private float Attack3StartY    = 0f;
private float Attack3EndY      = 0f;

private void Attack3()
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
    float speed       = 10f;
    int   damage      = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

    for (int i = 0; i < bulletCount; i++)
    {
        float t      = (float)i / (bulletCount - 1); // 0 to 1
        float spawnY = MathHelper.Lerp(Attack3StartY, Attack3EndY, t);

        Vector2 spawnPos = new Vector2(NPC.Center.X, spawnY);

        int projType = Main.rand.Next(4) switch
        {
            0 => ModContent.ProjectileType<CycloneProjectile1>(),
            1 => ModContent.ProjectileType<CycloneProjectile2>(),
            2 => ModContent.ProjectileType<CycloneProjectile3>(),
            _ => ModContent.ProjectileType<CycloneProjectile4>(),
        };

        Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            spawnPos,
            new Vector2(speed, 0f),
            projType,
            damage,
            2f,
            Main.myPlayer
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

private void Attack4()
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
                int projType = Main.rand.Next(4) switch
                {
                    0 => ModContent.ProjectileType<CycloneProjectile1>(),
                    1 => ModContent.ProjectileType<CycloneProjectile2>(),
                    2 => ModContent.ProjectileType<CycloneProjectile3>(),
                    _ => ModContent.ProjectileType<CycloneProjectile4>(),
                };

                Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
                int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    shootDir * 10f,
                    projType,
                    damage,
                    2f,
                    Main.myPlayer
                );
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
    int projDamage  = 20;

    for (int i = 0; i < lineCount; i++)
    {
        int x = (int)(NPC.Center.X + Attack4Part1Random - lineSpacing * (lineCount / 2) + i * lineSpacing);
        Vector2 projPos = new Vector2(x, NPC.Center.Y - 300);

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            int projType = Main.rand.Next(4) switch
            {
                0 => ModContent.ProjectileType<CycloneProjectile1>(),
                1 => ModContent.ProjectileType<CycloneProjectile2>(),
                2 => ModContent.ProjectileType<CycloneProjectile3>(),
                _ => ModContent.ProjectileType<CycloneProjectile4>(),
            };

            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                projPos,
                new Vector2(0, projSpeed),
                projType,
                projDamage, 2f, Main.myPlayer
            );
        }
    }
}






private void EnhancedAttack1()
{   
    Player player = Main.player[NPC.target];
    AITimer++;
    // Let clone timer run first so its staggered forward
    CloneTimer++;
    
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
        NPC.Center = player.Center + new Vector2(Main.rand.NextFloat(-300f, 300f), -240f);
        NPC.alpha     = 255;
        NPC.velocity  = Vector2.Zero;
        NPC.netUpdate = true;
        SoundEngine.PlaySound(CycloneRoarGor, NPC.Center);

    }

    

    if (CloneTimer % 241f == 1 && CloneTimer < 500) //spawns only 2 clones
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float cloneAngle = Main.rand.NextFloat(0, MathHelper.TwoPi);
                Vector2 clonePos = player.Center + new Vector2(300f, 0f).RotatedBy(cloneAngle);
                int cloneIndex   = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clonePos.X, (int)clonePos.Y, ModContent.NPCType<CycloneClone>());
                Main.npc[cloneIndex].ai[1] = 1f;
                Main.npc[cloneIndex].netUpdate = true;
                SoundEngine.PlaySound(CycloneRoarGor, NPC.Center);
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

        int projType = Main.rand.Next(4) switch
        {
            0 => ModContent.ProjectileType<CycloneProjectile1>(),
            1 => ModContent.ProjectileType<CycloneProjectile2>(),
            2 => ModContent.ProjectileType<CycloneProjectile3>(),
            _ => ModContent.ProjectileType<CycloneProjectile4>(),
        };

        Projectile.NewProjectile(
            NPC.GetSource_FromAI(),
            spawnPos,
            new Vector2(Main.rand.NextFloat(-1f, 1f), 8f), // slight random X drift, straight down
            projType,
            damage,
            2f,
            Main.myPlayer
        );
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
                int projType = Main.rand.Next(4) switch
                {
                    0 => ModContent.ProjectileType<CycloneProjectile1>(),
                    1 => ModContent.ProjectileType<CycloneProjectile2>(),
                    2 => ModContent.ProjectileType<CycloneProjectile3>(),
                    _ => ModContent.ProjectileType<CycloneProjectile4>(),
                };

                Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
                int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    shootDir * 10f,
                    projType,
                    damage,
                    2f,
                    Main.myPlayer
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
private int   EnhancedAttack3ShootTimer = 0;
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

        EnhancedAttack3ShootTimer++;
        if (EnhancedAttack3ShootTimer >= 70)
        {
            EnhancedAttack3ShootTimer = 0;
            EnhancedAttack3SubAttack  = Main.rand.Next(3);
            PickEnhancedAttack3Range(player);
            FireEnhancedWallVolley(player);
        }

        return;
    }

    if (AITimer >= 600f)
    {
        NPC.alpha  = 0;
        AITimer    = 0f;
        EnhancedAttack3ShootTimer   = 0;
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

        int projType = Main.rand.Next(4) switch
        {
            0 => ModContent.ProjectileType<CycloneProjectile1>(),
            1 => ModContent.ProjectileType<CycloneProjectile2>(),
            2 => ModContent.ProjectileType<CycloneProjectile3>(),
            _ => ModContent.ProjectileType<CycloneProjectile4>(),
        };

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                spawnPos,
                new Vector2(speed, 0f),
                projType,
                damage,
                2f,
                Main.myPlayer
            );
            SoundEngine.PlaySound(ProjectileSound, NPC.Center);
        }
    }
}

private float EnhancedAttack4LoopCount = 0f;
private bool  EnhancedAttack4LinesSpawned = false;
private int   EnhancedAttack4Part1Random = 0;
private float EnhancedAttack4Angle     = 0f;
private float EnhancedAttack4HoverTimer = 0f;
private float EnhancedAttack4ShootTimer = 0f;
private float EnhancedOrbitRadius4 = 400f;
private int EnhancedAttack4CloneSequence = 0; // 0=topleft, 1=topright, 2=botleft, 3=botright
private float EnhancedAttack4CloneTimer = 0f;

private int EnhancedAttack4CloneCount = 0;
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
        EnhancedAttack4Angle          = MathHelper.PiOver2;
        NPC.Center                    = player.Center + new Vector2(0f, EnhancedOrbitRadius4);
        NPC.alpha                     = 255;
        NPC.velocity                  = Vector2.Zero;
        EnhancedAttack4LinesSpawned   = false;
        int dir                       = Main.rand.NextBool() ? 1 : -1;
        EnhancedAttack4Part1Random    = dir * Main.rand.Next(3, 6) * 16;
        NPC.netUpdate                 = true;

    }

    float targetAngle = -MathHelper.PiOver2;
    if (AITimer >= 61f && EnhancedAttack4Angle > targetAngle)
    {
        NPC.alpha             = (int)MathHelper.Lerp(255, 0, Math.Min((AITimer - 61f) / 60f, 1f));
        EnhancedAttack4Angle -= OrbitSpeed;

        Vector2 idealPos      = player.Center + new Vector2(EnhancedOrbitRadius4, 0f).RotatedBy(EnhancedAttack4Angle);
        Vector2 idealVelocity = Vector2.Normalize(idealPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, idealPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;
        return;
    }

    if (AITimer >= 61f && EnhancedAttack4Angle <= targetAngle)
    {   

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

        EnhancedAttack4HoverTimer++;

        if (EnhancedAttack4HoverTimer == 60f && !EnhancedAttack4LinesSpawned)
            SpawnTelegraphLines(player);

        if (EnhancedAttack4HoverTimer == 90f && !EnhancedAttack4LinesSpawned)
        {
            SpawnLaserProjectiles(player);
            EnhancedAttack4LinesSpawned = true;
        }

        EnhancedAttack4ShootTimer++;
        if (EnhancedAttack4ShootTimer >= 50)
        {
            EnhancedAttack4ShootTimer = 0;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projType = Main.rand.Next(4) switch
                {
                    0 => ModContent.ProjectileType<CycloneProjectile1>(),
                    1 => ModContent.ProjectileType<CycloneProjectile2>(),
                    2 => ModContent.ProjectileType<CycloneProjectile3>(),
                    _ => ModContent.ProjectileType<CycloneProjectile4>(),
                };

                Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
                int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    shootDir * 10f,
                    projType,
                    damage,
                    2f,
                    Main.myPlayer
                );
                SoundEngine.PlaySound(ProjectileSound, NPC.Center);
            }
        }

        EnhancedAttack4CloneTimer++;

        // Spawn next clone 
        if (EnhancedAttack4CloneTimer >= 180f && EnhancedAttack4CloneCount < 4)
        {
            EnhancedAttack4CloneTimer  = 0f;

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
                EnhancedAttack4CloneCount ++;
            }
        }

        if (EnhancedAttack4HoverTimer >= 360f)
        {
            EnhancedAttack4LoopCount++;
            EnhancedAttack4HoverTimer   = 0f;
            EnhancedAttack4LinesSpawned = false;
            int dir                     = Main.rand.NextBool() ? 1 : -1;
            EnhancedAttack4Part1Random  = dir * Main.rand.Next(3, 6) * 16;

            if (EnhancedAttack4LoopCount >= 3)
            {
                EnhancedAttack4LoopCount  = 0f;
                EnhancedAttack4HoverTimer = 0f;
                EnhancedAttack4Angle      = 0f;
                AITimer                   = 0f; 
                EnhancedAttack4CloneSequence   = 0;
                EnhancedAttack4CloneTimer      = 0f;
                EnhancedAttack4CloneCount = 0;


                AIState    = (float)ActionState.Reset;

                NPC.netUpdate = true;
            }
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

    writer.Write(EnhancedAttack4Angle);
    writer.Write(EnhancedAttack4Part1Random);

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

    EnhancedAttack4Angle = reader.ReadSingle();
    EnhancedAttack4Part1Random = reader.ReadInt32();

    LaunchDirection = new Vector2(reader.ReadSingle(), reader.ReadSingle());
}

#endregion Networking


}






















public class CycloneClone : ModNPC
{
    public ref float AITimer => ref NPC.ai[0];
    private Vector2 LaunchTarget = Vector2.Zero;
    
    private const int FadeInDuration = 59;
    private const int TargetAlpha    = 200; // semi transparent, lower = more visible

    private static readonly SoundStyle ProjectileSound = new SoundStyle("TheBattleCats/Assets/Effects/CycloneProjectile")
    {
        PitchVariance = 0.2f, // adds slight random pitch variation each play, stops it sounding repetitive
    };

    public override void SetDefaults()
    {
        NPC.width         = 140;
        NPC.height        = 140;
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
    
private float CloneOrbitAngle     = 0f;
private float CloneOrbitRadius    = 300f;
private Vector2 CloneLaunchDirection = Vector2.Zero;
private float CloneLaunchTimer    = 0f;
private const float CloneOrbitSpeed = 0.03f;
private const float CloneOrbitLaunchDegrees = 270f;

private bool CloneLaunched = false;
private void DoAttack1()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    // Phase 2: teleport to random position around player
    if (AITimer == 1f)
    {
        CloneOrbitAngle   = Main.rand.NextFloat(0, MathHelper.TwoPi);
        NPC.Center        = player.Center + new Vector2(CloneOrbitRadius, 0f).RotatedBy(CloneOrbitAngle);
        NPC.alpha         = 160;
        NPC.netUpdate     = true;
    }

    // Phase 3: orbit
    if (AITimer >= 1f && AITimer < (CloneOrbitLaunchDegrees / MathHelper.ToDegrees(CloneOrbitSpeed)))
    {   
        NPC.alpha = (int)MathHelper.Lerp(255, 200, (AITimer - 1f) / 60f);
        CloneOrbitAngle += CloneOrbitSpeed;

        Vector2 targetPos     = player.Center + new Vector2(CloneOrbitRadius, 0f).RotatedBy(CloneOrbitAngle);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity          = Vector2.Lerp(NPC.velocity, idealVelocity, 0.15f);

        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;
        return;
    }

    // Phase 4: launch at player
    if (CloneLaunchDirection == Vector2.Zero && CloneLaunched == false)
    {
        CloneLaunchDirection = Vector2.Normalize(player.Center - NPC.Center);
        CloneLaunched = true;

    }
    NPC.velocity = CloneLaunchDirection * 14f;

    CloneLaunchTimer++;
    if (CloneLaunchTimer >= 60)
    {
        NPC.alpha = (int)MathHelper.Lerp(200, 255, Math.Min((CloneLaunchTimer - 60f) / 60f, 1f));
        CloneLaunchDirection = Vector2.Zero;
        
    }

    if (CloneLaunchTimer >= 120)
    {
        CloneLaunchTimer     = 0;
        CloneOrbitAngle      = 0f;
        NPC.active           = false;
        CloneLaunched = false;
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

            int projType = Main.rand.Next(4) switch
            {
                0 => ModContent.ProjectileType<CycloneProjectile1>(),
                1 => ModContent.ProjectileType<CycloneProjectile2>(),
                2 => ModContent.ProjectileType<CycloneProjectile3>(),
                _ => ModContent.ProjectileType<CycloneProjectile4>(),
            };
            SoundEngine.PlaySound(ProjectileSound, NPC.Center);

            Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
            int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir * 10f, projType, damage, 2f, Main.myPlayer);
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

                    int projType = Main.rand.Next(4) switch
                    {
                        0 => ModContent.ProjectileType<CycloneProjectile1>(),
                        1 => ModContent.ProjectileType<CycloneProjectile2>(),
                        2 => ModContent.ProjectileType<CycloneProjectile3>(),
                        _ => ModContent.ProjectileType<CycloneProjectile4>(),
                    };
                    SoundEngine.PlaySound(ProjectileSound, NPC.Center);

                    Projectile.NewProjectile(NPC.GetSource_FromAI(),
                        new Vector2(NPC.Center.X, spawnY),
                        new Vector2(-8f, 0f), // fire left
                        projType,
                        NPC.GetAttackDamage_ForProjectiles(30f, 20f),
                        2f, Main.myPlayer);
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

        int projType = Main.rand.Next(4) switch
        {
            0 => ModContent.ProjectileType<CycloneProjectile1>(),
            1 => ModContent.ProjectileType<CycloneProjectile2>(),
            2 => ModContent.ProjectileType<CycloneProjectile3>(),
            _ => ModContent.ProjectileType<CycloneProjectile4>(),
        };
        SoundEngine.PlaySound(ProjectileSound, NPC.Center);

        Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
        int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);
        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir * 10f, projType, damage, 2f, Main.myPlayer);
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
