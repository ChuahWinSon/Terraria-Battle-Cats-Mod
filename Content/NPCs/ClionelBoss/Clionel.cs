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
            Stay,                  // minis locked to base offset, frozen on frame 0
            AttackSynced,          // minis follow SharedFrame for position AND sprite
            AttackIndependent,     // minis play animation freely, stay at base offset
            AttackOneByOne,        // minis animate one at a time in slot order
            AttackOneByOneAttack,  // minis charge up, launch, fade out, fade in one at a time
        }

        /// <summary>The current behavior mode the minis should execute.</summary>
        public MiniBehavior CurrentMiniBehavior { get; private set; } = MiniBehavior.Stay;

        /// <summary>Which mini slot is currently animating during AttackOneByOne / AttackOneByOneAttack.</summary>
        public int ActiveMiniSlot { get; private set; } = 0;

        /// <summary>Delay in ticks between each mini's turn during AttackOneByOneAttack.</summary>
        public const int OneByOneAttackDelay = 60;

        private int oneByOneDelayTimer = 0;
        private bool waitingForNextMini = false;

        private enum ActionState
        {
            TestAnimation,
            TestAnimationShort,
            Idle,
            MiniOnlyAttack,
            OneByOne,
            OneByOneAttack,
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
                case ActionState.OneByOneAttack:
                    DoBehavior_OneByOneAttack(player);
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

            AdvanceSharedFrame((ActionState)AIState);
        }

        // -------------------------------------------------------
        // Mini orbit distance / lerp constants
        // -------------------------------------------------------
        public const float MiniCloseDistance = 0.5f;
        public const float MiniOuterDistance = 1.5f;
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

            if (ActiveMiniSlot >= 3)
            {
                ActiveMiniSlot = 0;
                AIState = (float)ActionState.Reset;
                NPC.netUpdate = true;
            }
        }

        // -------------------------------------------------------
        // Behavior: OneByOneAttack
        // -------------------------------------------------------
        private void DoBehavior_OneByOneAttack(Player player)
        {
            CurrentMiniBehavior = MiniBehavior.AttackOneByOneAttack;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

            SharedFrame = 0;
            frameTimer = 0;

            // Count down delay between minis
            if (waitingForNextMini)
            {
                oneByOneDelayTimer++;
                if (oneByOneDelayTimer >= OneByOneAttackDelay)
                {
                    oneByOneDelayTimer = 0;
                    waitingForNextMini = false;
                    NPC.netUpdate = true;
                }
                return;
            }

            // Check if the active mini finished its full sequence
            if (ActiveMiniSlot < 3 &&
                miniWhoAmI[ActiveMiniSlot] >= 0 &&
                miniWhoAmI[ActiveMiniSlot] < Main.maxNPCs)
            {
                NPC mini = Main.npc[miniWhoAmI[ActiveMiniSlot]];
                if (mini.active && mini.ModNPC is MiniClionel mc && mc.AnimationFinished)
                {
                    ActiveMiniSlot++;
                    NPC.netUpdate = true;

                    if (ActiveMiniSlot < 3)
                    {
                        waitingForNextMini = true;
                        oneByOneDelayTimer = 0;
                    }
                }
            }

            if (ActiveMiniSlot >= 3)
            {
                ActiveMiniSlot = 0;
                waitingForNextMini = false;
                oneByOneDelayTimer = 0;
                AIState = (float)ActionState.Reset;
                NPC.netUpdate = true;
            }
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
                // ActionState nextAttack;
                // do
                // {
                //     nextAttack = Main.rand.Next(6) switch
                //     {
                //         0 => ActionState.TestAnimation,
                //         1 => ActionState.TestAnimationShort,
                //         2 => ActionState.Idle,
                //         3 => ActionState.MiniOnlyAttack,
                //         4 => ActionState.OneByOne,
                //         _ => ActionState.OneByOneAttack,
                //     };
                // }
                // while (nextAttack == previousAttack);

                ActionState nextAttack = ActionState.OneByOneAttack; //testing

                previousAttack = nextAttack;
                AIState        = (float)nextAttack;
                AITimer        = 0f;
                NPC.netUpdate  = true;
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

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = SharedFrame * frameHeight;
        }

        // -------------------------------------------------------
        // Mini management
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