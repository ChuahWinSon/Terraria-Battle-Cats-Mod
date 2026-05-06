using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent;

namespace TheBattleCats.Content.NPCs.ClionelBoss
{
    /// <summary>
    /// One of three satellite NPCs that orbit Clionel.
    /// ai[0] = whoAmI of the owner (Clionel)
    /// ai[1] = slot index (0, 1, or 2) — used to pick the correct MiniOffsets entry
    /// </summary>
    public class MiniClionel : ModNPC
    {
        // -------------------------------------------------------
        // Convenience refs
        // -------------------------------------------------------
        private ref float OwnerIndex => ref NPC.ai[0];
        private ref float SlotIndex  => ref NPC.ai[1];

        private Vector2 currentOffset;
        private bool offsetInitialised = false;

        private int miniFrame = 0;
        private int miniFrameTimer = 0;
        private const int MiniFrameSpeed = 8;

        /// <summary>True for one tick when the sequence is fully complete.</summary>
        public bool AnimationFinished { get; private set; } = false;

        // -------------------------------------------------------
        // Separate damage value for the mini launch
        // -------------------------------------------------------
        public static int MiniLaunchDamage => 25;

        // -------------------------------------------------------
        // Per-mini state machine for AttackOneByOneAttack
        // -------------------------------------------------------
        private enum MiniActionState
        {
            Idle,
            ChargeUp,       // animate frames 0-8
            Waiting,        // frozen at frame 8 for WaitDuration
            Launching,      // fly straight at player for LaunchDuration
            FinishAndFade,  // play frames 9-14 while fading out, stopped in place
            FadeIn,         // invisible, teleported to base, fade back in
        }

        private MiniActionState miniState = MiniActionState.Idle;
        private int miniStateTimer = 0;
        private Vector2 launchDirection = Vector2.Zero;
        private float alpha = 1f;

        // -------------------------------------------------------
        // Cone telegraph
        // -------------------------------------------------------
        private readonly MiniConeEffect _coneEffect = new();
        private float coneAlpha = 0f;
        private const int ConeFadeInDuration = 90; // ticks to reach full opacity

        // -------------------------------------------------------
        // Tunable constants
        // -------------------------------------------------------
        private const int   WaitDuration   = 60;  // ticks frozen at frame 8
        private const int   LaunchDuration = 120; // ticks flying at the player
        private const int   FadeInDuration = 30;  // ticks to fade back in
        public  const float LaunchSpeed    = 24f; // pixels per tick

        // Total frames in the finish segment (9 through 14 inclusive = 6 frames)
        private const int FinishFrameStart = 9;
        private const int FinishFrameEnd   = 14;
        private const int FinishFrameCount = FinishFrameEnd - FinishFrameStart + 1; // 6
        private const int FinishDuration   = FinishFrameCount * MiniFrameSpeed;     // 48 ticks

        // -------------------------------------------------------
        // SetDefaults / SetStaticDefaults
        // -------------------------------------------------------
        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.damage = 0;
            NPC.defense = 5;
            NPC.lifeMax = 1;
            NPC.dontTakeDamage = true;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.width = 30;
            NPC.height = 40;
            NPC.HitSound = null;
            NPC.DeathSound = null;
            NPC.boss = false;
            NPC.hide = false;
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
            int ownerIdx = (int)OwnerIndex;
            int slot     = (int)SlotIndex;

            if (ownerIdx < 0 || ownerIdx >= Main.maxNPCs)
            {
                NPC.active = false;
                return;
            }

            NPC owner = Main.npc[ownerIdx];

            if (!owner.active || owner.type != ModContent.NPCType<Clionel>())
            {
                NPC.active = false;
                return;
            }

            Clionel clionel = owner.ModNPC as Clionel;
            Vector2 baseOffset = Clionel.MiniOffsets[slot];

            NPC.spriteDirection = owner.spriteDirection;

            if (!offsetInitialised)
            {
                currentOffset     = baseOffset;
                offsetInitialised = true;
            }

            switch (clionel.CurrentMiniBehavior)
            {
                case Clionel.MiniBehavior.Stay:
                    ResetMiniState();
                    DoMiniBehavior_Stay(baseOffset);
                    break;

                case Clionel.MiniBehavior.AttackSynced:
                    ResetMiniState();
                    DoMiniBehavior_AttackSynced(clionel, baseOffset);
                    break;

                case Clionel.MiniBehavior.AttackIndependent:
                    coneAlpha = 0f;
                    NPC.damage = 0;
                    alpha = 1f;
                    DoMiniBehavior_AttackIndependent(baseOffset);
                    break;

                case Clionel.MiniBehavior.AttackOneByOne:
                    coneAlpha = 0f;
                    NPC.damage = 0;
                    alpha = 1f;
                    if (slot == clionel.ActiveMiniSlot)
                        DoMiniBehavior_AttackIndependent(baseOffset);
                    else
                    {
                        miniFrame = 0;
                        miniFrameTimer = 0;
                        AnimationFinished = false;
                        DoMiniBehavior_Stay(baseOffset);
                    }
                    break;

                case Clionel.MiniBehavior.AttackOneByOneAttack:
                    if (slot == clionel.ActiveMiniSlot)
                        DoMiniBehavior_AttackOneByOneAttack(clionel, baseOffset, owner);
                    else
                    {
                        coneAlpha = 0f;
                        if (slot > clionel.ActiveMiniSlot)
                            ResetMiniState();
                        DoMiniBehavior_Stay(baseOffset);
                    }
                    break;
            }

            // Apply alpha and position
            NPC.Opacity = alpha;

            bool isLaunching = clionel.CurrentMiniBehavior == Clionel.MiniBehavior.AttackOneByOneAttack
                               && miniState == MiniActionState.Launching;

            bool isFinishing = clionel.CurrentMiniBehavior == Clionel.MiniBehavior.AttackOneByOneAttack
                               && miniState == MiniActionState.FinishAndFade;

            if (!isLaunching && !isFinishing)
            {
                NPC.Center   = owner.Center + currentOffset;
                NPC.velocity = Vector2.Zero;
            }
        }

        // -------------------------------------------------------
        // Helper — reset all mini state back to neutral
        // -------------------------------------------------------
        private void ResetMiniState()
        {
            miniFrame         = 0;
            miniFrameTimer    = 0;
            miniState         = MiniActionState.Idle;
            miniStateTimer    = 0;
            AnimationFinished = false;
            alpha             = 1f;
            coneAlpha         = 0f;
            NPC.damage        = 0;
        }

        // -------------------------------------------------------
        // Stay
        // -------------------------------------------------------
        private void DoMiniBehavior_Stay(Vector2 baseOffset)
        {
            currentOffset = Vector2.Lerp(currentOffset, baseOffset, Clionel.MiniLerpSpeed);
        }

        // -------------------------------------------------------
        // AttackSynced
        // -------------------------------------------------------
        private void DoMiniBehavior_AttackSynced(Clionel clionel, Vector2 baseOffset)
        {
            int frame = clionel.SharedFrame;

            float targetScale;
            if (frame <= 2)
                targetScale = Clionel.MiniCloseDistance;
            else if (frame <= 8)
                targetScale = Clionel.MiniOuterDistance;
            else if (frame <= 12)
                targetScale = Clionel.MiniOuterDistance;
            else
                targetScale = 1.0f;

            currentOffset = Vector2.Lerp(currentOffset, baseOffset * targetScale, Clionel.MiniLerpSpeed);
        }

        // -------------------------------------------------------
        // AttackIndependent — free animation, stay at base offset
        // -------------------------------------------------------
        private void DoMiniBehavior_AttackIndependent(Vector2 baseOffset)
        {
            AnimationFinished = false;

            miniFrameTimer++;
            if (miniFrameTimer >= MiniFrameSpeed)
            {
                miniFrameTimer = 0;
                miniFrame = (miniFrame + 1) % 15;

                if (miniFrame == 0)
                    AnimationFinished = true;
            }

            currentOffset = Vector2.Lerp(currentOffset, baseOffset, Clionel.MiniLerpSpeed);
        }

        // -------------------------------------------------------
        // AttackOneByOneAttack — charge → wait → launch → finish+fade → fadein
        // -------------------------------------------------------
        private void DoMiniBehavior_AttackOneByOneAttack(Clionel clionel, Vector2 baseOffset, NPC owner)
        {
            AnimationFinished = false;
            miniStateTimer++;

            switch (miniState)
            {
                // --------------------------------------------------
                // Idle: first tick — initialise and fall into ChargeUp
                // --------------------------------------------------
                case MiniActionState.Idle:
                    miniFrame      = 0;
                    miniFrameTimer = 0;
                    miniStateTimer = 0;
                    alpha          = 1f;
                    coneAlpha      = 0f;
                    NPC.damage     = 0;
                    miniState      = MiniActionState.ChargeUp;
                    goto case MiniActionState.ChargeUp;

                // --------------------------------------------------
                // ChargeUp: advance frames 0-8, cone fades in
                // --------------------------------------------------
                case MiniActionState.ChargeUp:
                    coneAlpha = MathHelper.Clamp((float)miniStateTimer / ConeFadeInDuration, 0f, 1f);
                    currentOffset = Vector2.Lerp(currentOffset, baseOffset, Clionel.MiniLerpSpeed);

                    if (miniFrame < 8)
                    {
                        miniFrameTimer++;
                        if (miniFrameTimer >= MiniFrameSpeed)
                        {
                            miniFrameTimer = 0;
                            miniFrame++;
                        }
                    }
                    else
                    {
                        miniFrame      = 8;
                        miniState      = MiniActionState.Waiting;
                        miniStateTimer = 0;
                    }
                    break;

                // --------------------------------------------------
                // Waiting: frozen at frame 8, cone fully visible, tracks player
                // --------------------------------------------------
                case MiniActionState.Waiting:
                    coneAlpha     = 1f;
                    miniFrame     = 8;
                    currentOffset = Vector2.Lerp(currentOffset, baseOffset, Clionel.MiniLerpSpeed);

                    if (miniStateTimer >= WaitDuration)
                    {
                        coneAlpha      = 0f;
                        miniState      = MiniActionState.Launching;
                        miniStateTimer = 0;
                        NPC.damage     = MiniLaunchDamage;
                    }
                    break;

                // --------------------------------------------------
                // Launching: lock direction on tick 1, fly straight
                // --------------------------------------------------
                case MiniActionState.Launching:
                    coneAlpha = 0f;

                    if (miniStateTimer == 1)
                    {
                        Player player = Main.player[owner.target];
                        launchDirection = player.Center - NPC.Center;
                        if (launchDirection != Vector2.Zero)
                            launchDirection.Normalize();
                    }

                    NPC.velocity = launchDirection * LaunchSpeed;
                    miniFrame    = 8;

                    if (miniStateTimer >= LaunchDuration)
                    {
                        NPC.velocity   = Vector2.Zero;
                        NPC.damage     = 0;
                        miniState      = MiniActionState.FinishAndFade;
                        miniStateTimer = 0;
                        miniFrame      = FinishFrameStart;
                        miniFrameTimer = 0;
                    }
                    break;

                // --------------------------------------------------
                // FinishAndFade: play frames 9-14 in place while fading out
                // --------------------------------------------------
                case MiniActionState.FinishAndFade:
                    coneAlpha = 0f;

                    miniFrameTimer++;
                    if (miniFrameTimer >= MiniFrameSpeed)
                    {
                        miniFrameTimer = 0;
                        if (miniFrame < FinishFrameEnd)
                            miniFrame++;
                    }

                    alpha = 1f - (float)miniStateTimer / FinishDuration;
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);

                    NPC.velocity = Vector2.Zero;

                    if (miniStateTimer >= FinishDuration)
                    {
                        alpha         = 0f;
                        currentOffset = baseOffset;
                        NPC.Center    = owner.Center + baseOffset;
                        miniState     = MiniActionState.FadeIn;
                        miniStateTimer = 0;
                        miniFrame     = 0;
                        miniFrameTimer = 0;
                    }
                    break;

                // --------------------------------------------------
                // FadeIn: appear back at base offset
                // --------------------------------------------------
                case MiniActionState.FadeIn:
                    coneAlpha = 0f;

                    alpha = (float)miniStateTimer / FadeInDuration;
                    alpha = MathHelper.Clamp(alpha, 0f, 1f);
                    currentOffset = Vector2.Lerp(currentOffset, baseOffset, Clionel.MiniLerpSpeed);

                    if (miniStateTimer >= FadeInDuration)
                    {
                        alpha             = 1f;
                        miniFrame         = 0;
                        miniFrameTimer    = 0;
                        miniState         = MiniActionState.Idle;
                        miniStateTimer    = 0;
                        AnimationFinished = true;
                    }
                    break;
            }
        }

        // -------------------------------------------------------
        // PostDraw — draw the cone telegraph
        // -------------------------------------------------------
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (coneAlpha <= 0f)
                return;

            int ownerIdx = (int)OwnerIndex;
            if (ownerIdx < 0 || ownerIdx >= Main.maxNPCs)
                return;

            NPC owner = Main.npc[ownerIdx];
            if (!owner.active)
                return;

            Player player = Main.player[owner.target];
            _coneEffect.Draw(spriteBatch, NPC, screenPos, coneAlpha, player);
        }

        // -------------------------------------------------------
        // FindFrame
        // -------------------------------------------------------
        public override void FindFrame(int frameHeight)
        {
            int ownerIdx = (int)OwnerIndex;

            if (ownerIdx >= 0 && ownerIdx < Main.maxNPCs)
            {
                NPC owner = Main.npc[ownerIdx];
                if (owner.active && owner.ModNPC is Clionel clionel)
                {
                    bool usesMiniFrame =
                        clionel.CurrentMiniBehavior == Clionel.MiniBehavior.AttackIndependent         ||
                        clionel.CurrentMiniBehavior == Clionel.MiniBehavior.AttackOneByOneAttack      ||
                        (clionel.CurrentMiniBehavior == Clionel.MiniBehavior.AttackOneByOne &&
                         (int)SlotIndex == clionel.ActiveMiniSlot);

                    NPC.frame.Y = usesMiniFrame
                        ? miniFrame * frameHeight
                        : clionel.SharedFrame * frameHeight;
                    return;
                }
            }

            NPC.frame.Y = 0;
        }

        // -------------------------------------------------------
        // Don't let minis appear in the bestiary / boss bar
        // -------------------------------------------------------
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
            => false;
    }
}