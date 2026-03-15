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

    private float Attack2Angle = 0f;
private Vector2 CycloneLaunchTarget = Vector2.Zero;

private void Attack2()
{
    Player player = Main.player[NPC.target];
    AITimer++;

    if (AITimer == 1f)
    {
        // Pick random angle, place all 3 (2 copies + boss) evenly around circle
        Attack2Angle = Main.rand.NextFloat(0, MathHelper.TwoPi);
        CycloneLaunchTarget = player.Center; // store player pos once

        float angle1 = Attack2Angle;
        float angle2 = Attack2Angle + MathHelper.TwoPi / 3f;
        float angle3 = Attack2Angle + MathHelper.TwoPi / 3f * 2f;

        // Teleport main boss to its position
        NPC.Center = player.Center + new Vector2(OrbitRadius, 0f).RotatedBy(angle1);
        NPC.alpha  = 255;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            NPC.NewNPC(NPC.GetSource_FromAI(), 
                (int)(player.Center + new Vector2(OrbitRadius, 0f).RotatedBy(angle2)).X,
                (int)(player.Center + new Vector2(OrbitRadius, 0f).RotatedBy(angle2)).Y,
                ModContent.NPCType<CycloneClone>());

            NPC.NewNPC(NPC.GetSource_FromAI(),
                (int)(player.Center + new Vector2(OrbitRadius, 0f).RotatedBy(angle3)).X,
                (int)(player.Center + new Vector2(OrbitRadius, 0f).RotatedBy(angle3)).Y,
                ModContent.NPCType<CycloneClone>());
        }
        NPC.netUpdate = true;
    }

    if (AITimer >= 1f)
    {
        NPC.alpha = (int)MathHelper.Lerp(255, 0, (AITimer - 60f) / 60f);
    }

    if (AITimer <= 120f)
    {
        NPC.velocity = Vector2.Zero;
        NPC.direction = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;
        return;
    }


    // Dash toward stored player position
    Vector2 launchDir = Vector2.Normalize(CycloneLaunchTarget - NPC.Center);
    NPC.velocity = launchDir * 14f;

    if (AITimer >= 180f)
    {
        NPC.alpha  = 0;
        AITimer    = 0f;
        CycloneLaunchTarget = Vector2.Zero;
        AIState    = (float)ActionState.Idle;
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


private float Attack4Timer = 0f;
private float Attack4Angle = 0f;

private void Attack4()
{
    Player player = Main.player[NPC.target];
    Attack4Timer++;

    // Phase 1: disappear (0-60 ticks)
    if (Attack4Timer < 60f)
    {
        NPC.velocity = Vector2.Zero;
        NPC.alpha = (int)MathHelper.Lerp(0, 255, Attack4Timer / 60f);
        return;
    }

    // Phase 2: teleport to bottom of player
    if (Attack4Timer == 61f)
    {
        // Start from directly below the player
        Attack4Angle = MathHelper.PiOver2; // 90 degrees = bottom
        NPC.Center   = player.Center + new Vector2(0f, OrbitRadius);
        NPC.alpha    = 255;
        NPC.velocity = Vector2.Zero;
        NPC.netUpdate = true;
    }

    // Phase 3: arc from bottom to top (90 degrees to -90 degrees = half circle)
    float targetAngle = -MathHelper.PiOver2; // top of player
    if (Attack4Timer >= 61f && Attack4Angle > targetAngle)
    {
        // Fade in
        NPC.alpha = (int)MathHelper.Lerp(255, 0, Math.Min((Attack4Timer - 61f) / 60f, 1f));

        // Rotate from bottom toward top
        Attack4Angle -= OrbitSpeed;

        Vector2 idealPos     = player.Center + new Vector2(OrbitRadius, 0f).RotatedBy(Attack4Angle);
        float   dist         = Vector2.Distance(NPC.Center, idealPos);
        float   catchUpSpeed = MathHelper.Clamp(dist / 10f, 3f, 30f);

        NPC.velocity        = Vector2.Normalize(idealPos - NPC.Center) * catchUpSpeed;
        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;
        return;
    }

    // Phase 4: hover above player mimicking their movement
    if (Attack4Timer >= 61f && Attack4Angle <= targetAngle)
    {
        NPC.alpha = 0;

        float horizontalOffset = 0f;
        float xDiff = NPC.Center.X - player.Center.X;
        if (Math.Abs(xDiff) < 20f)
        {
            horizontalOffset = xDiff == 0f ? (NPC.whoAmI % 2 == 0 ? 140f : -140f) : Math.Sign(xDiff) * 40f;
        }

        Vector2 targetPos    = new Vector2(player.Center.X + horizontalOffset, player.Center.Y - OrbitRadius);
        Vector2 idealVelocity = Vector2.Normalize(targetPos - NPC.Center) * MathHelper.Clamp(Vector2.Distance(NPC.Center, targetPos) / 8f, 2f, 20f);
        NPC.velocity         = Vector2.Lerp(NPC.velocity, idealVelocity, 0.04f);

        NPC.direction       = NPC.Center.X < player.Center.X ? 1 : -1;
        NPC.spriteDirection = NPC.direction;
    }
  
}

}

public class CycloneClone : ModNPC
{
    public ref float AITimer => ref NPC.ai[0];
    private Vector2 LaunchTarget = Vector2.Zero;
    
    private const int FadeInDuration = 120;
    private const int TargetAlpha    = 160; // semi transparent, lower = more visible

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
        AITimer++;
        NPC.TargetClosest(true);
        Player player = Main.player[NPC.target];

        // Store player position once on first tick
        if (AITimer == 1f)
        {
            LaunchTarget = player.Center;
            NPC.netUpdate = true;
        }

        // Fade in to semi transparent
        if (AITimer <= FadeInDuration)
        {
            NPC.alpha = (int)MathHelper.Lerp(255, TargetAlpha, AITimer / FadeInDuration);
            NPC.velocity = Vector2.Zero;
            NPC.direction = NPC.Center.X < player.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
            return;
        }

        // Dash toward stored player position
        Vector2 launchDir = Vector2.Normalize(LaunchTarget - NPC.Center);
        NPC.velocity = launchDir * 14f;

        if (AITimer >= 180f)
            NPC.active = false;

            
    }
}

}
