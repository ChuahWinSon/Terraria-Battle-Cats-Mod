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
            Attack2,
            Attack3,
            Attack4,
            Transform,
            EnhancedAttack1,
            EnhancedAttack2,
            EnhancedAttack3,
            EnhancedAttack4
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
            NPC.width = 120;
            NPC.height = 120;
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
                case (float)ActionState.Attack2:
                    Attack2();
                    break;
                case (float)ActionState.Attack3:
                    Attack3();
                    break;
                case (float)ActionState.Attack4:
                    Attack4();
                    break;
                case (float)ActionState.Transform:
                    Idle();
                    break;
                case (float)ActionState.EnhancedAttack1:
                    Attack1();
                    break;
                case (float)ActionState.EnhancedAttack2:
                    Attack2();
                    break;
                case (float)ActionState.EnhancedAttack3:
                    Attack3();
                    break;
                case (float)ActionState.EnhancedAttack4:
                    Attack4();
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
                AIState = (float)ActionState.Attack2;
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
                AIState = (float)ActionState.Attack1;
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

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            float clone1Angle = Attack2OrbitAngle + MathHelper.TwoPi / 3f;         // 120 degrees
            float clone2Angle = Attack2OrbitAngle + MathHelper.TwoPi / 3f * 2f;    // 240 degrees

            Vector2 clone1Pos = player.Center + new Vector2(OrbitRadius2, 0f).RotatedBy(clone1Angle);
            Vector2 clone2Pos = player.Center + new Vector2(OrbitRadius2, 0f).RotatedBy(clone2Angle);

            int clone1Index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clone1Pos.X, (int)clone1Pos.Y, ModContent.NPCType<CycloneClone>());
            Main.npc[clone1Index].ai[1] = clone1Angle;
            Main.npc[clone1Index].ai[2] = OrbitRadius2;
            Main.npc[clone1Index].netUpdate = true;

            int clone2Index = NPC.NewNPC(NPC.GetSource_FromAI(), (int)clone2Pos.X, (int)clone2Pos.Y, ModContent.NPCType<CycloneClone>());
            Main.npc[clone2Index].ai[1] = clone2Angle;
            Main.npc[clone2Index].ai[2] = OrbitRadius2;
            Main.npc[clone2Index].netUpdate = true;
        }

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

        // Shoot every 30 ticks
        Attack2ShootTimer++;
        if (Attack2ShootTimer >= 30)
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
                    shootDir * 12f,
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
        AIState           = (float)ActionState.Idle;
    }
}

private int ShootTimer = 0;

private void Attack3()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    // Slowly move toward player
    Vector2 toPlayer = Vector2.Normalize(player.Center - NPC.Center);
    NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer * 3f, 0.05f);

    // Face player
    NPC.direction = NPC.Center.X < player.Center.X ? 1 : -1;
    NPC.spriteDirection = NPC.direction;

    // Shoot every 30 ticks (0.5 seconds)
    ShootTimer++;
    if (ShootTimer >= 30)
    {
        ShootTimer = 0;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            // Pick random projectile from 4 types
            int projType = Main.rand.Next(4) switch
            {
                0 => ModContent.ProjectileType<CycloneProjectile1>(),
                1 => ModContent.ProjectileType<CycloneProjectile2>(),
                2 => ModContent.ProjectileType<CycloneProjectile3>(),
                _ => ModContent.ProjectileType<CycloneProjectile4>(),
            };

            Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
            int damage = NPC.GetAttackDamage_ForProjectiles(30f, 20f);

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

    // End after 5 seconds
    if (AITimer >= 300f)
    {
        ShootTimer = 0;
        AITimer    = 0f;
        AIState    = (float)ActionState.Attack1;
    }
}


private float Attack4LoopCount = 0f;
private bool  Attack4LinesSpawned = false;
private int   Attack4Part1Random = 0;
private float Attack4Angle     = 0f;
private float Attack4HoverTimer = 0f;

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
        NPC.Center            = player.Center + new Vector2(0f, OrbitRadius);
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

        Vector2 idealPos      = player.Center + new Vector2(OrbitRadius, 0f).RotatedBy(Attack4Angle);
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

        Vector2 targetPos     = new Vector2(player.Center.X + horizontalOffset, player.Center.Y - OrbitRadius);
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
        if (Attack4HoverTimer == 120f && !Attack4LinesSpawned)
        {
            SpawnLaserProjectiles(player);
            Attack4LinesSpawned = true;
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
                AIState           = (float)ActionState.Attack3;
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

}

public class CycloneClone : ModNPC
{
    public ref float AITimer => ref NPC.ai[0];
    private Vector2 LaunchTarget = Vector2.Zero;
    
    private const int FadeInDuration = 59;
    private const int TargetAlpha    = 200; // semi transparent, lower = more visible

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
    AITimer++;

    if (AITimer == 1f)
    {
        Attack2OrbitAngle = NPC.ai[1]; // use angle passed from boss
        NPC.alpha         = 255;
        NPC.velocity      = Vector2.Zero;
        NPC.netUpdate     = true;
    }

    // Orbit and shoot
    if (AITimer >= 1f && AITimer < 300f)
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
        if (Attack2ShootTimer >= 30 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Attack2ShootTimer = 0;

            int projType = Main.rand.Next(4) switch
            {
                0 => ModContent.ProjectileType<CycloneProjectile1>(),
                1 => ModContent.ProjectileType<CycloneProjectile2>(),
                2 => ModContent.ProjectileType<CycloneProjectile3>(),
                _ => ModContent.ProjectileType<CycloneProjectile4>(),
            };

            Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center);
            int damage       = NPC.GetAttackDamage_ForProjectiles(30f, 20f);
            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir * 12f, projType, damage, 2f, Main.myPlayer);
        }

        return;
    }

    // Fade out
    if (AITimer >= 300f && AITimer < 360f)
    {
        NPC.alpha = (int)MathHelper.Lerp(200, 255, Math.Min((AITimer - 300f) / 60f, 1f));
        NPC.velocity *= 0.9f;
        return;
    }

    if (AITimer >= 360f)
        NPC.active = false;
}
}

}
