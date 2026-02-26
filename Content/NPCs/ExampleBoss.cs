using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheBattleCats.Content.NPCs
{
    public class ExampleBoss : ModNPC
    {
        private enum BossState
        {
            Idle,
            Attacking
        }

        private BossState currentState => (BossState)(int)NPC.ai[0];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 40; // Max of idle (20) and attack (40)
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 40;
            NPC.defense = 20;
            NPC.lifeMax = 5000;
            NPC.knockBackResist = 0f;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.boss = true;
            NPC.lavaImmune = true;
        }

        public override void AI()
        {
            NPC.TargetClosest();

            if (NPC.ai[1]++ > 300)
            {
                NPC.ai[1] = 0;
                NPC.ai[0] = NPC.ai[0] == 0 ? 1 : 0; // Toggle between idle and attack
            }

            // Movement example
            Player player = Main.player[NPC.target];
            Vector2 direction = player.Center - NPC.Center;
            direction.Normalize();
            NPC.velocity = direction * 2f;
        }

        public override void FindFrame(int frameHeight)
        {
            if (currentState == BossState.Attacking)
            {
                NPC.frameCounter += 0.4;
                if (NPC.frameCounter >= 40)
                    NPC.frameCounter = 0;
                NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
            }
            else // Idle
            {
                NPC.frameCounter += 0.2;
                if (NPC.frameCounter >= 20)
                    NPC.frameCounter = 0;
                NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture;
            int frameCount;
            Vector2 drawOrigin;

            if (currentState == BossState.Attacking)
            {
                texture = ModContent.Request<Texture2D>("TheBattleCats/Content/NPCs/ExampleBoss_Attack").Value;
                frameCount = 40;
                drawOrigin = new Vector2(texture.Width / 2f, (texture.Height / frameCount) / 2f + 10f); // Adjust offset as needed
            }
            else
            {
                texture = ModContent.Request<Texture2D>("TheBattleCats/Content/NPCs/ExampleBoss_Idle").Value;
                frameCount = 20;
                drawOrigin = new Vector2(texture.Width / 2f, (texture.Height / frameCount) / 2f);
            }

            int frameHeight = texture.Height / frameCount;
            Rectangle frame = new Rectangle(0, (int)NPC.frameCounter * frameHeight, texture.Width, frameHeight);

            Vector2 drawPosition = NPC.Center - screenPos;

            spriteBatch.Draw(texture, drawPosition, frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, SpriteEffects.None, 0f);
            return false; // We've handled drawing
        }
    }
}
