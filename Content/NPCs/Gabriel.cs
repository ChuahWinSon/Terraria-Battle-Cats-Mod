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
    public class Gabriel : ModNPC
    {
        // ai[0] = current state (0 = Rush, 1 = Attack, 2 = Cooldown)
        // ai[1] = frame timer / state timer

        private enum ActionState
        {
            Rush = 0,
            Attack = 1,
            Cooldown = 2
        }

        private ActionState State
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        private ref float Timer => ref NPC.ai[1];

        // Tune these
        private const float RushSpeed     = 6f;
        private const float RushAccel     = 0.3f;
        private const float RushDuration  = 180f; // ticks before switching to Attack
        private const float AttackDuration = 60f; // ticks the attack animation plays
        private const float CooldownDuration = 90f; // ticks standing still

        // Spritesheet layout:
        // Frames 0-3  = Rush  (4 frames)
        // Frames 4-7  = Attack (4 frames)
        // Frame  8    = Cooldown (1 frame)

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 9;
        }

        public override void SetDefaults()
        {
            NPC.width        = 36;
            NPC.height       = 36;
            NPC.damage       = 25;
            NPC.defense      = 8;
            NPC.lifeMax      = 200;
            NPC.HitSound     = SoundID.NPCHit1;
            NPC.DeathSound   = SoundID.NPCDeath2;
            NPC.knockBackResist = 0.4f;
            NPC.aiStyle      = -1; // custom AI
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
            NPC.scale = 0.5f;
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            Timer++;

            switch (State)
            {
                case ActionState.Rush:
                    DoRush(player);
                    break;
                case ActionState.Attack:
                    DoAttack(player);
                    break;
                case ActionState.Cooldown:
                    DoCooldown(player);
                    break;
            }

            // Sprite faces movement direction
            if (NPC.velocity.X != 0f)
                NPC.spriteDirection = NPC.velocity.X > 0f ? -1 : 1;
        }

        private void DoRush(Player player)
        {
            // Fast horizontal movement toward player
            if (NPC.velocity.X < -RushSpeed || NPC.velocity.X > RushSpeed)
            {
                if (NPC.velocity.Y == 0f)
                    NPC.velocity.X *= 0.8f;
            }
            else if (NPC.velocity.X < RushSpeed && NPC.direction == 1)
            {
                NPC.velocity.X += RushAccel;
                if (NPC.velocity.X > RushSpeed) NPC.velocity.X = RushSpeed;
            }
            else if (NPC.velocity.X > -RushSpeed && NPC.direction == -1)
            {
                NPC.velocity.X -= RushAccel;
                if (NPC.velocity.X < -RushSpeed) NPC.velocity.X = -RushSpeed;
            }

            // Jump over obstacles
            if (NPC.velocity.Y == 0f && NPC.collideX)
                NPC.velocity.Y = -5f;

            if (Timer >= RushDuration)
            {
                Timer = 0f;
                State = ActionState.Attack;
                NPC.velocity.X = 0f; // stop moving for attack
                NPC.netUpdate = true;
            }
        }

        private void DoAttack(Player player)
        {
            // Stand (mostly) still during attack animation
            NPC.velocity.X *= 0.85f;

            if (Timer >= AttackDuration)
            {
                Timer = 0f;
                State = ActionState.Cooldown;
                NPC.netUpdate = true;
            }
        }

        private void DoCooldown(Player player)
        {
            // Fully stopped
            NPC.velocity.X *= 0.7f;
            if (Math.Abs(NPC.velocity.X) < 0.1f)
                NPC.velocity.X = 0f;

            if (Timer >= CooldownDuration)
            {
                Timer = 0f;
                State = ActionState.Rush;
                NPC.netUpdate = true;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            switch (State)
            {
                case ActionState.Rush:
                    // Frames 0-3, 5 ticks per frame
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= 5)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y >= frameHeight * 4) // past frame 3, loop back to 0
                            NPC.frame.Y = 0;
                    }
                    break;

                case ActionState.Attack:
                    // Frames 4-7, 15 ticks per frame (slow, deliberate)
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= 15)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y >= frameHeight * 8) // past frame 7, clamp at 7
                            NPC.frame.Y = frameHeight * 7;
                    }
                    // Start attack animation at frame 4 when entering this state
                    if (NPC.frame.Y < frameHeight * 4)
                    {
                        NPC.frame.Y = frameHeight * 4;
                        NPC.frameCounter = 0;
                    }
                    break;

                case ActionState.Cooldown:
                    // Frame 8, static
                    NPC.frame.Y = frameHeight * 8;
                    break;
            }
        }
    }
}