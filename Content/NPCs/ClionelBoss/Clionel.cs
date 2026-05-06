using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;

namespace TheBattleCats.Content.NPCs.ClionelBoss
{
    [AutoloadBossHead]
    public class Clionel : ModNPC
    {
        // -------------------------------------------------------
        // Mini behavior modes — add new entries here for new attacks
        // -------------------------------------------------------
        public enum MiniBehavior
        {
            Stay,               // minis locked to base offset, frozen on frame 0
            AttackSynced,       // minis follow SharedFrame for position AND sprite
            AttackIndependent,  // minis play animation freely, stay at base offset
            AttackOneByOne,     // minis animate one at a time in slot order
        }

        /// <summary>The current behavior mode the minis should execute.</summary>
        public MiniBehavior CurrentMiniBehavior { get; private set; } = MiniBehavior.Stay;

        /// <summary>Which mini slot is currently animating during AttackOneByOne.</summary>
        public int ActiveMiniSlot { get; private set; } = 0;

        private enum ActionState
        {
            TestAnimation,
            TestAnimationShort,
            Idle,
            MiniOnlyAttack,
            OneByOne,
            Spawn,
            Reset,
            Death
        }

        public ref float AIState     => ref NPC.ai[0];
        public ref float AITimer     => ref NPC.ai[1];
        public ref float AttackTimer => ref NPC.ai[2];
        public ref float ExtraTimer  => ref NPC.ai[3];

        public static int AllProjectileDamage => 20;

        // -------------------------------------------------------
        // Mini Clionel NPC slot tracking
        // -------------------------------------------------------
        private int[] miniWhoAmI = new int[3] { -1, -1, -1 };

        // -------------------------------------------------------
        // Offsets from the boss centre for each mini.
        // Edit these Vector2 values to reposition the minis.
        // -------------------------------------------------------
        public static Vector2[] MiniOffsets = new Vector2[3]
        {
            new Vector2(-100f, -90f),   // [0] Top-left
            new Vector2( 100f, -90f),   // [1] Top-right
            new Vector2( 100f,  90f),   // [2] Bottom-right
        };

        // -------------------------------------------------------
        // Shared animation frame (written here, read by minis)
        // -------------------------------------------------------
        /// <summary>The animation frame that the boss AND all minis should display.</summary>
        public int SharedFrame { get; private set; } = 0;

        private int frameTimer = 0;
        private const int FrameSpeed = 8; // ticks per frame

        private ActionState PreviousState = ActionState.TestAnimation;
        private float LifeRatio;

        // -------------------------------------------------------
        // SetDefaults / SetStaticDefaults
        // -------------------------------------------------------
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
            NPC.width = 60;
            NPC.height = 150;
            NPC.boss = true;
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/CycloneBossMusic");
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 15;
        }

        // -------------------------------------------------------
        // Main AI
        // -------------------------------------------------------
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
                PreviousState = CurrentState;

            // Always keep minis alive and in position
            ManageMinis();

            switch ((ActionState)AIState)
            {
                case ActionState.TestAnimation:
                    DoBehavior_TestAnimation(player);
                    break;
                case ActionState.TestAnimationShort:
                    DoBehavior_TestAnimationShort(player);
                    break;
                case ActionState.Idle:
                    DoBehavior_Idle(player);
                    break;
                case ActionState.MiniOnlyAttack:
                    DoBehavior_MiniOnlyAttack(player);
                    break;
                case ActionState.OneByOne:
                    DoBehavior_OneByOne(player);
                    break;
                case ActionState.Reset:
                    DoBehavior_ResetAI();
                    break;
                case ActionState.Spawn:
                    DoBehavior_SpawnAnimation();
                    break;
                case ActionState.Death:
                    DoBehavior_DeathAnimation();
                    break;
            }

            // Advance the shared animation frame every tick
            AdvanceSharedFrame((ActionState)AIState);
        }


                // -------------------------------------------------------
        // Behavior: Reset
        // -------------------------------------------------------
        private ActionState previousAttack = ActionState.Reset;

        private void DoBehavior_ResetAI()
        {
            CurrentMiniBehavior = MiniBehavior.Stay;

            AttackTimer = 0f;
            AITimer     = 0f;
            ExtraTimer  = 0f;

            NPC.TargetClosest(false);
            NPC.velocity *= 0.95f;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {

                ActionState nextAttack;
                do
                {

                    nextAttack = Main.rand.Next(5) switch
                    {
                        0 => ActionState.TestAnimation,
                        1 => ActionState.TestAnimationShort,
                        2 => ActionState.Idle,
                        3 => ActionState.MiniOnlyAttack,
                        _ => ActionState.OneByOne,
                    };
                }

                while (nextAttack == previousAttack);

                // ActionState nextAttack = ActionState.TestAnimation; // testing

                previousAttack = nextAttack;
                AIState        = (float)nextAttack;
                AITimer        = 0f;
                NPC.netUpdate  = true;
            }
        }

        // -------------------------------------------------------
        // Mini orbit distance / lerp constants
        // -------------------------------------------------------
        /// <summary>How close the minis pull in during frames 0-2 (fraction of original offset).</summary>
        public const float MiniCloseDistance = 0.5f;

        /// <summary>How far the minis push out during frames 3-8 (fraction of original offset).</summary>
        public const float MiniOuterDistance = 1.5f;

        /// <summary>Lerp speed for mini position interpolation. Higher = snappier (0.0-1.0).</summary>
        public const float MiniLerpSpeed = 0.06f;

        private readonly ClionelGlowEffect _glowEffect = new();

        // -------------------------------------------------------
        // Behavior: TestAnimation
        // -------------------------------------------------------
        private void DoBehavior_TestAnimation(Player player)
        {
            CurrentMiniBehavior = MiniBehavior.AttackSynced;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

            _glowEffect.Update();

            if (SharedFrame == 8)
            {
                AttackTimer++;
                if (AttackTimer >= 120)
                {
                    AttackTimer = 0f;
                    SharedFrame = 9;
                    NPC.netUpdate = true;
                }
            }

            if (SharedFrame == 14 && frameTimer == FrameSpeed - 1)
            {
                AIState = (float)ActionState.Reset;
                SharedFrame = 0;
                frameTimer = 0;
                NPC.netUpdate = true;
            }
        }

        // -------------------------------------------------------
        // Behavior: TestAnimationShort
        // -------------------------------------------------------
        private void DoBehavior_TestAnimationShort(Player player)
        {
            CurrentMiniBehavior = MiniBehavior.AttackSynced;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

            _glowEffect.Update();

            if (SharedFrame == 14 && frameTimer == FrameSpeed - 1)
            {
                AIState = (float)ActionState.Reset;
                SharedFrame = 0;
                frameTimer = 0;
                NPC.netUpdate = true;
            }
        }

        // -------------------------------------------------------
        // Behavior: Idle
        // -------------------------------------------------------
        private void DoBehavior_Idle(Player player)
        {
            CurrentMiniBehavior = MiniBehavior.Stay;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

            SharedFrame = 0;
            frameTimer = 0;

            AITimer++;
            if (AITimer >= 300)
            {
                AITimer = 0f;
                AIState = (float)ActionState.Reset;
                NPC.netUpdate = true;
            }
        }

        // -------------------------------------------------------
        // Behavior: MiniOnlyAttack
        // -------------------------------------------------------
        private void DoBehavior_MiniOnlyAttack(Player player)
        {
            CurrentMiniBehavior = MiniBehavior.AttackIndependent;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

            SharedFrame = 0;
            frameTimer = 0;

            // Wait for all three minis to finish their animation cycle
            bool allDone = true;
            for (int i = 0; i < 3; i++)
            {
                if (miniWhoAmI[i] >= 0 && miniWhoAmI[i] < Main.maxNPCs)
                {
                    NPC mini = Main.npc[miniWhoAmI[i]];
                    if (mini.active && mini.ModNPC is MiniClionel mc && !mc.AnimationFinished)
                        allDone = false;
                }
            }

            if (allDone)
            {
                AIState = (float)ActionState.Reset;
                NPC.netUpdate = true;
            }
        }

        // -------------------------------------------------------
        // Behavior: OneByOne
        // -------------------------------------------------------
        private void DoBehavior_OneByOne(Player player)
        {
            CurrentMiniBehavior = MiniBehavior.AttackOneByOne;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

            SharedFrame = 0;
            frameTimer = 0;

            // Check if the currently active mini has finished its animation
            if (ActiveMiniSlot < 3 &&
                miniWhoAmI[ActiveMiniSlot] >= 0 &&
                miniWhoAmI[ActiveMiniSlot] < Main.maxNPCs)
            {
                NPC mini = Main.npc[miniWhoAmI[ActiveMiniSlot]];
                if (mini.active && mini.ModNPC is MiniClionel mc && mc.AnimationFinished)
                {
                    ActiveMiniSlot++;
                    NPC.netUpdate = true;
                }
            }

            // All three done — reset and transition to Idle
            if (ActiveMiniSlot >= 3)
            {
                ActiveMiniSlot = 0;
                AIState = (float)ActionState.Reset;
                NPC.netUpdate = true;
            }
        }



        // -------------------------------------------------------
        // Placeholder stubs
        // -------------------------------------------------------
        private void DoBehavior_SpawnAnimation() { }
        private void DoBehavior_DeathAnimation() { }

        // -------------------------------------------------------
        // Shared frame advancement (15 frames, looping)
        // -------------------------------------------------------
        private void AdvanceSharedFrame(ActionState currentState)
        {
            if (SharedFrame == 8 && currentState == ActionState.TestAnimation)
                return;

            frameTimer++;
            if (frameTimer < FrameSpeed)
                return;

            frameTimer = 0;
            SharedFrame = (SharedFrame + 1) % 15;
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            _glowEffect.Draw(spriteBatch, NPC, screenPos, SharedFrame);
        }

        // -------------------------------------------------------
        // FindFrame — sync boss sprite to SharedFrame
        // -------------------------------------------------------
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = SharedFrame * frameHeight;
        }

        // -------------------------------------------------------
        // Mini management — spawn missing minis, move existing ones
        // -------------------------------------------------------
        private void ManageMinis()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            for (int i = 0; i < 3; i++)
            {
                bool needsSpawn = true;

                if (miniWhoAmI[i] >= 0 && miniWhoAmI[i] < Main.maxNPCs)
                {
                    NPC mini = Main.npc[miniWhoAmI[i]];
                    if (mini.active && mini.type == ModContent.NPCType<MiniClionel>())
                        needsSpawn = false;
                }

                if (needsSpawn)
                {
                    Vector2 spawnPos = NPC.Center + MiniOffsets[i];
                    int idx = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X, (int)spawnPos.Y,
                                        ModContent.NPCType<MiniClionel>(), 0,
                                        NPC.whoAmI,
                                        i);
                    miniWhoAmI[i] = idx;

                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, idx);
                }
            }
        }

        // -------------------------------------------------------
        // Kill minis when boss dies / despawns
        // -------------------------------------------------------
        public override void OnKill()
        {
            KillMinis();
        }

        private void KillMinis()
        {
            for (int i = 0; i < 3; i++)
            {
                if (miniWhoAmI[i] >= 0 && miniWhoAmI[i] < Main.maxNPCs)
                {
                    NPC mini = Main.npc[miniWhoAmI[i]];
                    if (mini.active && mini.type == ModContent.NPCType<MiniClionel>())
                        mini.active = false;
                }
                miniWhoAmI[i] = -1;
            }
        }
    }
}