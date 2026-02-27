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

namespace TheBattleCats.Content.NPCs
{
    public class Tackey : ModNPC
    {
        // ai[0] = state (0 = Walking/Positioning, 1 = Attacking, 2 = Cooldown)
        // ai[1] = state timer
        // ai[2] = has fired projectile this attack (0 = no, 1 = yes)

        private enum ActionState { Walking = 0, Attacking = 1, Cooldown = 2 }

        private ActionState State
        {
            get => (ActionState)NPC.ai[0];
            set { NPC.ai[0] = (float)value; NPC.netUpdate = true; }
        }

        private ref float Timer    => ref NPC.ai[1];
        private ref float HasFired => ref NPC.ai[2];

        // Tune these
        private const float TargetDistance   = 16 * 16f;  // 8 blocks in pixels
        private const float WalkSpeed        = 1.5f;
        private const float WalkAccel        = 0.07f;
        private const float PositionTolerance = 12f;      // how close to target X before stopping
        private const float AttackDuration   = 90f;       // ticks for full attack animation
        private const float CooldownDuration = 120f;      // ticks of cooldown repositioning
        private const int   FireFrame        = 4;         // which attack frame fires the projectile
        private const int   TicksPerAttackFrame = 10;     // ticks per frame during attack

        // Frame layout:
        // 0-7   = Walking  (8 frames)
        // 8-16  = Attacking (9 frames)

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 17;
        }

        public override void SetDefaults()
        {
            NPC.width        = 36;
            NPC.height       = 48;
            NPC.damage       = 20;
            NPC.defense      = 4;
            NPC.lifeMax      = 150;
            NPC.HitSound     = SoundID.NPCHit1;
            NPC.DeathSound   = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.5f;
            NPC.aiStyle      = -1;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.ZoneForest &&
                spawnInfo.SpawnTileY <= Main.worldSurface &&
                Main.dayTime)
            {
                return 0.1f;
            }
            return 0f;
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC.scale = 0.7f;
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            Timer++;

            switch (State)
            {
                case ActionState.Walking:   DoWalking(player);   break;
                case ActionState.Attacking: DoAttacking(player); break;
                case ActionState.Cooldown:  DoCooldown(player);  break;
            }
        }

        // ─── Shared positioning logic ─────────────────────────────────────────────

        // Moves NPC to stand TargetDistance away from the player on the X axis.
        // Returns true when in position.
        private bool MoveToPositioningDistance(Player player)
        {
            // Stand to the left or right depending on which side we're already on
            float targetX = player.Center.X + (NPC.Center.X < player.Center.X ? -TargetDistance : TargetDistance);
            float diff    = targetX - NPC.Center.X;

            // Face the player
            NPC.spriteDirection = NPC.direction = (player.Center.X > NPC.Center.X) ? -1 : 1;

            if (Math.Abs(diff) <= PositionTolerance)
            {
                // In position — brake
                NPC.velocity.X *= 0.8f;
                if (Math.Abs(NPC.velocity.X) < 0.1f) NPC.velocity.X = 0f;
                return true;
            }

            // Accelerate toward target X
            int moveDir = diff > 0f ? 1 : -1;

            if (NPC.velocity.X < -WalkSpeed || NPC.velocity.X > WalkSpeed)
            {
                if (NPC.velocity.Y == 0f) NPC.velocity.X *= 0.8f;
            }
            else if (moveDir == 1 && NPC.velocity.X < WalkSpeed)
            {
                NPC.velocity.X += WalkAccel;
                if (NPC.velocity.X > WalkSpeed) NPC.velocity.X = WalkSpeed;
            }
            else if (moveDir == -1 && NPC.velocity.X > -WalkSpeed)
            {
                NPC.velocity.X -= WalkAccel;
                if (NPC.velocity.X < -WalkSpeed) NPC.velocity.X = -WalkSpeed;
            }

            // Jump over obstacles
            if (NPC.velocity.Y == 0f && NPC.collideX)
                NPC.velocity.Y = -5f;

            return false;
        }

        // ─── States ───────────────────────────────────────────────────────────────

        private void DoWalking(Player player)
        {
            bool inPosition = MoveToPositioningDistance(player);

            if (inPosition)
            {
                // Reached target distance — switch to attack
                Timer    = 0f;
                HasFired = 0f;
                State    = ActionState.Attacking;
            }
        }

        private void DoAttacking(Player player)
        {
            // Stay still while attacking
            NPC.velocity.X *= 0.85f;

            // Fire on frame FireFrame
            int currentAttackFrame = (int)(Timer / TicksPerAttackFrame);
            if (HasFired == 0f && currentAttackFrame >= FireFrame)
            {
                HasFired = 1f;
                FireProjectile(player);
            }

            if (Timer >= AttackDuration)
            {
                Timer    = 0f;
                HasFired = 0f;
                State    = ActionState.Cooldown;
            }
        }

        private void DoCooldown(Player player)
        {
            // Reposition during cooldown, same as walking
            MoveToPositioningDistance(player);

            if (Timer >= CooldownDuration)
            {
                Timer    = 0f;
                HasFired = 0f;
                State    = ActionState.Attacking; // skip straight back to attack
            }
        }

        private void FireProjectile(Player player)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            Vector2 spawnPos = NPC.Center + new Vector2(-NPC.spriteDirection * 16f, -20f);
            Vector2 direction = (player.Center - spawnPos).SafeNormalize(Vector2.UnitX * NPC.spriteDirection);
            direction.Y -= 1f;
            direction = Vector2.Normalize(direction);
            float speed = 8f;

            int damage = NPC.GetAttackDamage_ForProjectiles(20f, 15f);
            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                spawnPos,
                direction * speed,
                ModContent.ProjectileType<TackeyProjectile>(),
                damage,
                2f,
                Main.myPlayer
            );

            Vector2 spawnPosSmoke = NPC.Center + new Vector2(NPC.spriteDirection * 16f, -20f);

            Projectile.NewProjectile(
                NPC.GetSource_FromAI(),
                spawnPos,
                direction * speed,
                ModContent.ProjectileType<TackeySmoke>(),
                0,
                2f,
                Main.myPlayer
            );

            SoundEngine.PlaySound(SoundID.Item11, NPC.position);
        }

        // ─── Animation ───────────────────────────────────────────────────────────

        public override void FindFrame(int frameHeight)
        {
            switch (State)
            {
                case ActionState.Walking:
                case ActionState.Cooldown:
                {
                    bool moving = Math.Abs(NPC.velocity.X) > 0.2f;

                    if (moving)
                    {
                        // Cycle frames 0-7
                        NPC.frameCounter++;
                        if (NPC.frameCounter >= 6)
                        {
                            NPC.frameCounter = 0;
                            NPC.frame.Y += frameHeight;
                        }
                        if (NPC.frame.Y >= frameHeight * 8)
                            NPC.frame.Y = 0;
                        // Keep in walking range
                        if (NPC.frame.Y >= frameHeight * 8)
                            NPC.frame.Y = 0;
                    }
                    else
                    {
                        // Idle — hold frame 0
                        NPC.frame.Y = 0;
                        NPC.frameCounter = 0;
                    }
                    break;
                }

                case ActionState.Attacking:
                {
                    // Frames 8-16, advance by TicksPerAttackFrame
                    int attackFrame = Math.Min((int)(Timer / TicksPerAttackFrame), 8);
                    NPC.frame.Y = frameHeight * (8 + attackFrame);
                    break;
                }
            }
        }
    }
}