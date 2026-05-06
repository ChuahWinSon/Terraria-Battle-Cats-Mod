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
                currentOffset    = baseOffset;
                offsetInitialised = true;
            }

            // --------------------------------------------------
            // Dispatch to the correct movement mode
            // --------------------------------------------------
            switch (clionel.CurrentMiniBehavior)
            {
                case Clionel.MiniBehavior.Stay:
                    DoMiniBehavior_Stay(baseOffset);
                    break;

                case Clionel.MiniBehavior.Orbit:
                    DoMiniBehavior_Orbit(clionel, baseOffset);
                    break;

                // Future behaviors get their own cases here:
                // case Clionel.MiniBehavior.Scatter:
                //     DoMiniBehavior_Scatter(clionel, baseOffset, slot);
                //     break;
            }

            // Apply position
            NPC.Center   = owner.Center + currentOffset;
            NPC.velocity = Vector2.Zero;
        }

        // -------------------------------------------------------
        // Stay — lerp smoothly back to the base offset and hold
        // -------------------------------------------------------
        private void DoMiniBehavior_Stay(Vector2 baseOffset)
        {
            currentOffset = Vector2.Lerp(currentOffset, baseOffset, Clionel.MiniLerpSpeed);
        }

        // -------------------------------------------------------
        // Orbit — frame-driven push/pull around the boss
        //
        //  Frames  0- 2  → pull in  (MiniCloseDistance)
        //  Frames  3- 8  → push out (MiniOuterDistance)
        //  Frames  9-12  → stay out (MiniOuterDistance)
        //  Frames 13-14  → return   (1.0f, original offset)
        // -------------------------------------------------------
        private void DoMiniBehavior_Orbit(Clionel clionel, Vector2 baseOffset)
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
        // FindFrame — mirror the boss's SharedFrame exactly
        // -------------------------------------------------------
        public override void FindFrame(int frameHeight)
        {
            int ownerIdx = (int)OwnerIndex;

            if (ownerIdx >= 0 && ownerIdx < Main.maxNPCs)
            {
                NPC owner = Main.npc[ownerIdx];
                if (owner.active && owner.ModNPC is Clionel clionel)
                {
                    NPC.frame.Y = clionel.SharedFrame * frameHeight;
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