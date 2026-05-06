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

        // Current interpolated position, updated each tick
        private Vector2 currentOffset;
        private bool offsetInitialised = false;

        // Independent frame counter — used during AttackIndependent and AttackOneByOne
        private int miniFrame = 0;
        private int miniFrameTimer = 0;
        private const int MiniFrameSpeed = 8;

        /// <summary>True for one tick when the independent animation completes a full cycle.</summary>
        public bool AnimationFinished { get; private set; } = false;

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
            NPC.width = 60;
            NPC.height = 60;
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
        // Main AI — execute whatever behavior the boss declares
        // -------------------------------------------------------
        public override void AI()
        {
            int ownerIdx = (int)OwnerIndex;
            int slot     = (int)SlotIndex;

            // Validate owner
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

            // Snap to base offset on first tick so there's no slide-in from Vector2.Zero
            if (!offsetInitialised)
            {
                currentOffset     = baseOffset;
                offsetInitialised = true;
            }

            // --------------------------------------------------
            // Dispatch to the correct movement mode
            // --------------------------------------------------
            switch (clionel.CurrentMiniBehavior)
            {
                case Clionel.MiniBehavior.Stay:
                    miniFrame = 0;
                    miniFrameTimer = 0;
                    AnimationFinished = false;
                    DoMiniBehavior_Stay(baseOffset);
                    break;

                case Clionel.MiniBehavior.AttackSynced:
                    miniFrame = 0;
                    miniFrameTimer = 0;
                    AnimationFinished = false;
                    DoMiniBehavior_AttackSynced(clionel, baseOffset);
                    break;

                case Clionel.MiniBehavior.AttackIndependent:
                    DoMiniBehavior_AttackIndependent(baseOffset);
                    break;

                case Clionel.MiniBehavior.AttackOneByOne:
                    // Only the active slot animates, others stay
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
            }

            // Apply position
            NPC.Center   = owner.Center + currentOffset;
            NPC.velocity = Vector2.Zero;
        }

        // -------------------------------------------------------
        // Stay — lerp smoothly back to base offset, frozen on frame 0
        // -------------------------------------------------------
        private void DoMiniBehavior_Stay(Vector2 baseOffset)
        {
            currentOffset = Vector2.Lerp(currentOffset, baseOffset, Clionel.MiniLerpSpeed);
        }

        // -------------------------------------------------------
        // AttackSynced — position AND sprite follow SharedFrame
        //
        //  Frames  0- 2  → pull in  (MiniCloseDistance)
        //  Frames  3- 8  → push out (MiniOuterDistance)
        //  Frames  9-12  → stay out (MiniOuterDistance)
        //  Frames 13-14  → return   (1.0f, original offset)
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
        // AttackIndependent — minis animate freely, stay at base offset
        // Also reused by AttackOneByOne for the active slot
        // -------------------------------------------------------
        private void DoMiniBehavior_AttackIndependent(Vector2 baseOffset)
        {
            // Reset the flag — it is only true for one tick when the cycle wraps
            AnimationFinished = false;

            miniFrameTimer++;
            if (miniFrameTimer >= MiniFrameSpeed)
            {
                miniFrameTimer = 0;
                miniFrame = (miniFrame + 1) % 15;

                // Just wrapped back to 0 — one full cycle complete
                if (miniFrame == 0)
                    AnimationFinished = true;
            }

            // Position stays at base offset, no push/pull
            currentOffset = Vector2.Lerp(currentOffset, baseOffset, Clionel.MiniLerpSpeed);
        }

        // -------------------------------------------------------
        // FindFrame — pick the right frame source per behavior
        //   Stay                → frame 0 (SharedFrame is always 0 here)
        //   AttackSynced        → follow SharedFrame
        //   AttackIndependent   → follow miniFrame
        //   AttackOneByOne      → active slot follows miniFrame, others frame 0
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
                        clionel.CurrentMiniBehavior == Clionel.MiniBehavior.AttackIndependent ||
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