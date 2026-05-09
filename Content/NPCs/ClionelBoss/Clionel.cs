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
        // Animation sheet
        // -------------------------------------------------------
        public enum AnimationSheet
        {
            Idle,
            Attack,
            // Future: Attack2, Death, Spawn, etc.
        }

        public AnimationSheet CurrentSheet { get; private set; } = AnimationSheet.Idle;

        public static int GetFrameCount(AnimationSheet sheet) => sheet switch
        {
            AnimationSheet.Idle   => 15,
            AnimationSheet.Attack => 15,
            _                     => 15,
        };

        public static string GetTexturePath(AnimationSheet sheet) => sheet switch
        {
            AnimationSheet.Idle   => "TheBattleCats/Content/NPCs/ClionelBoss/Clionel_Idle",
            AnimationSheet.Attack => "TheBattleCats/Content/NPCs/ClionelBoss/Clionel_Attack",
            _                     => "TheBattleCats/Content/NPCs/ClionelBoss/Clionel_Idle",
        };

        // -------------------------------------------------------
        // Mini behavior modes
        // -------------------------------------------------------
        public enum MiniBehavior
        {
            Stay,
            AttackSynced,
            AttackIndependent,
            AttackOneByOne,
            AttackOneByOneAttack,
        }

        public MiniBehavior CurrentMiniBehavior { get; private set; } = MiniBehavior.Stay;
        public int ActiveMiniSlot { get; private set; } = 0;
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
            LaserBeamAttack,
            Spawn,
            Reset,
            Death
        }

        public ref float AIState     => ref NPC.ai[0];
        public ref float AITimer     => ref NPC.ai[1];
        public ref float AttackTimer => ref NPC.ai[2];
        public ref float ExtraTimer  => ref NPC.ai[3];

        public static int AllProjectileDamage => 20;

        private int[] miniWhoAmI = new int[3] { -1, -1, -1 };

        public static Vector2[] MiniOffsets = new Vector2[3]
        {
            new Vector2(-100f, -90f),
            new Vector2( 100f, -90f),
            new Vector2( 100f,  90f),
        };

        public int SharedFrame { get; private set; } = 0;

        private int frameTimer = 0;
        private const int FrameSpeed = 8;

        private ActionState PreviousState = ActionState.Reset;
        private float LifeRatio;

        // -------------------------------------------------------
        // Laser state
        // -------------------------------------------------------
        public float LaserAngle { get; private set; } = 0f;
        public bool LaserActive { get; private set; } = false;

        public const float LaserStartOffset = -MathHelper.Pi / 3f;
        public const float LaserSweepSpeed = 0.015f;
        public const float LaserLength = 500f;
        public const int LaserBeamPauseDuration = 300;
        public static int LaserDamage => 20;

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
                case ActionState.LaserBeamAttack:
                    DoBehavior_LaserBeamAttack(player);
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

            if ((ActionState)AIState != ActionState.LaserBeamAttack)
                LaserActive = false;

            AdvanceSharedFrame((ActionState)AIState);
        }

        // -------------------------------------------------------
        // Mini constants
        // -------------------------------------------------------
        public const float MiniCloseDistance = 0.5f;
        public const float MiniOuterDistance = 1.5f;
        public const float MiniLerpSpeed = 0.06f;

        private readonly ClionelGlowEffect _glowEffect = new();
        private readonly ClionelLaserEffect _laserEffect = new();

        // -------------------------------------------------------
        // Behavior: TestAnimation
        // -------------------------------------------------------
        private void DoBehavior_TestAnimation(Player player)
        {
            CurrentSheet = AnimationSheet.Attack;
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
            CurrentSheet = AnimationSheet.Attack;
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
            CurrentSheet = AnimationSheet.Idle;
            CurrentMiniBehavior = MiniBehavior.Stay;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

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
            CurrentSheet = AnimationSheet.Idle;
            CurrentMiniBehavior = MiniBehavior.AttackIndependent;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

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
            CurrentSheet = AnimationSheet.Idle;
            CurrentMiniBehavior = MiniBehavior.AttackOneByOne;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

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
            CurrentSheet = AnimationSheet.Idle;
            CurrentMiniBehavior = MiniBehavior.AttackOneByOneAttack;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

            frameTimer = 0;

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
        // Behavior: LaserBeamAttack
        // -------------------------------------------------------
        private void DoBehavior_LaserBeamAttack(Player player)
        {
            CurrentSheet = AnimationSheet.Attack;
            CurrentMiniBehavior = MiniBehavior.AttackSynced;

            NPC.spriteDirection = player.Center.X > NPC.Center.X ? -1 : 1;
            Vector2 targetPos = player.Center + new Vector2(0f, -250f);
            NPC.velocity = (targetPos - NPC.Center) * 0.06f;

            _glowEffect.Update();

            if (SharedFrame == 8)
            {
                if (AttackTimer == 0)
                {
                    float angleToPlayer = (player.Center - NPC.Center).ToRotation();
                    LaserAngle = angleToPlayer + LaserStartOffset;
                    NPC.netUpdate = true;
                }

                LaserActive = true;
                LaserAngle += LaserSweepSpeed;

                Vector2 eyePos = NPC.Center + ClionelGlowEffect.EyeOffset * new Vector2(NPC.spriteDirection, 1f);
                Vector2 laserEnd = eyePos + LaserAngle.ToRotationVector2() * LaserLength;

                if (Collision.CheckAABBvLineCollision(player.TopLeft, player.Size, eyePos, laserEnd))
                    player.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), LaserDamage, 0);

                AttackTimer++;
                if (AttackTimer >= LaserBeamPauseDuration)
                {
                    AttackTimer   = 0f;
                    LaserActive   = false;
                    SharedFrame   = 9;
                    NPC.netUpdate = true;
                }
            }

            if (SharedFrame == 14 && frameTimer == FrameSpeed - 1)
            {
                AIState     = (float)ActionState.Reset;
                SharedFrame = 0;
                frameTimer  = 0;
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
                //     nextAttack = Main.rand.Next(7) switch
                //     {
                //         0 => ActionState.TestAnimation,
                //         1 => ActionState.TestAnimationShort,
                //         2 => ActionState.Idle,
                //         3 => ActionState.MiniOnlyAttack,
                //         4 => ActionState.OneByOne,
                //         5 => ActionState.OneByOneAttack,
                //         _ => ActionState.LaserBeamAttack,
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
        // Shared frame advancement
        // -------------------------------------------------------
        private void AdvanceSharedFrame(ActionState currentState)
        {
            if (SharedFrame == 8 && (currentState == ActionState.TestAnimation ||
                                     currentState == ActionState.LaserBeamAttack))
                return;

            frameTimer++;
            if (frameTimer < FrameSpeed)
                return;

            frameTimer  = 0;
            SharedFrame = (SharedFrame + 1) % GetFrameCount(CurrentSheet);
        }

        // -------------------------------------------------------
        // PreDraw — draw correct spritesheet, cancel default draw
        // -------------------------------------------------------
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(
                GetTexturePath(CurrentSheet),
                AssetRequestMode.ImmediateLoad).Value;

            int frameCount  = GetFrameCount(CurrentSheet);
            int frameHeight = tex.Height / frameCount;
            Rectangle sourceRect = new Rectangle(0, SharedFrame * frameHeight, tex.Width, frameHeight);

            Vector2 origin  = sourceRect.Size() / 2f;
            Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);

            SpriteEffects effects = NPC.spriteDirection == 1
                ? SpriteEffects.FlipHorizontally
                : SpriteEffects.None;

            spriteBatch.Draw(
                tex,
                drawPos,
                sourceRect,
                drawColor * NPC.Opacity,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );

            return false;
        }

        // -------------------------------------------------------
        // PostDraw
        // -------------------------------------------------------
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (CurrentSheet == AnimationSheet.Attack)
                _glowEffect.Draw(spriteBatch, NPC, screenPos, SharedFrame);

            if (LaserActive)
            {
                Vector2 eyePos = NPC.Center + ClionelGlowEffect.EyeOffset * new Vector2(NPC.spriteDirection, 1f);
                _laserEffect.Draw(spriteBatch, eyePos, screenPos, LaserAngle, LaserLength);
            }
        }

        // -------------------------------------------------------
        // FindFrame
        // -------------------------------------------------------
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