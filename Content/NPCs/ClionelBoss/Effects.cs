using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

namespace TheBattleCats.Content.NPCs.ClionelBoss
{
    public class ClionelGlowEffect
    {
        // -------------------------------------------------------
        // Tweak these
        // -------------------------------------------------------
        public static readonly Vector2 EyeOffset = new Vector2(-10f, -72f);
        public const float GlowMaxScale = 0.7f;
        public const float GlowMinScale = 0f;
        public const int GlowScaleUpEndFrame = 6;
        public const int GlowScaleDownStartFrame = 12;
        public const int TotalFrames = 15;
        public const float GlowRotationSpeed = -0.04f;

        private float _rotation = 0f;

        public void Update() => _rotation += GlowRotationSpeed;

        public void Draw(SpriteBatch spriteBatch, NPC npc, Vector2 screenPos, int sharedFrame)
        {
            float scale;
            if (sharedFrame <= GlowScaleUpEndFrame)
            {
                float t = sharedFrame / (float)GlowScaleUpEndFrame;
                scale = MathHelper.Lerp(GlowMinScale, GlowMaxScale, t);
            }
            else if (sharedFrame >= GlowScaleDownStartFrame)
            {
                float t = (sharedFrame - GlowScaleDownStartFrame) / (float)(TotalFrames - GlowScaleDownStartFrame);
                scale = MathHelper.Lerp(GlowMaxScale, GlowMinScale, t);
            }
            else
            {
                scale = GlowMaxScale;
            }

            if (scale <= 0f) return;

            Texture2D tex = ModContent.Request<Texture2D>(
                "TheBattleCats/Content/NPCs/ClionelBoss/ClionelGlow",
                AssetRequestMode.ImmediateLoad).Value;

            Vector2 offset = EyeOffset * new Vector2(npc.spriteDirection, 1f);
            Vector2 drawPos = npc.Center + offset - screenPos;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(tex, drawPos, null, Color.White, _rotation,
                tex.Size() / 2f, scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}