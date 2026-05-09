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
        public static readonly Vector2 EyeOffset = new Vector2(-10f, -40f);
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

    // -------------------------------------------------------
    // Cone telegraph drawn from a mini toward the player
    // -------------------------------------------------------
    public class MiniConeEffect
    {
        public const int ConeFadeInDuration = 90; // ticks to reach full opacity

        public void Draw(SpriteBatch spriteBatch, NPC mini, Vector2 screenPos, float coneAlpha, Player target)
        {
            if (coneAlpha <= 0f)
                return;

            Texture2D tex = ModContent.Request<Texture2D>(
                "TheBattleCats/Content/NPCs/ClionelBoss/ClionelCone",
                AssetRequestMode.ImmediateLoad).Value;

            // Angle from mini center toward the player
            Vector2 toPlayer = target.Center - mini.Center;
            float angle = toPlayer.ToRotation() + MathHelper.Pi;

            float drawOffset = 20f; // adjust this to push the cone further out
            Vector2 drawPos = mini.Center - screenPos + Vector2.Normalize(toPlayer) * drawOffset;

            spriteBatch.Draw(
                tex,
                drawPos,
                null,
                Color.White * coneAlpha,
                angle,
                new Vector2(tex.Width, tex.Height / 2f), // origin at right center, tip points toward player
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }

    // -------------------------------------------------------
    // Laser beam drawn from the boss eye
    // -------------------------------------------------------
    public class ClionelLaserEffect
    {
        private const string HeadPath = "TheBattleCats/Content/NPCs/ClionelBoss/LaserBeam_Head";
        private const string BodyPath = "TheBattleCats/Content/NPCs/ClionelBoss/LaserBeam_Body";
        private const string TailPath = "TheBattleCats/Content/NPCs/ClionelBoss/LaserBeam_Tail";

        public void Draw(SpriteBatch spriteBatch, Vector2 eyePos, Vector2 screenPos, float angle, float length)
        {
            Texture2D headTex = ModContent.Request<Texture2D>(HeadPath, AssetRequestMode.ImmediateLoad).Value;
            Texture2D bodyTex = ModContent.Request<Texture2D>(BodyPath, AssetRequestMode.ImmediateLoad).Value;
            Texture2D tailTex = ModContent.Request<Texture2D>(TailPath, AssetRequestMode.ImmediateLoad).Value;

            Vector2 dir = angle.ToRotationVector2();
            Vector2 origin = new Vector2(0f, headTex.Height / 2f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
                SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw head at eye position
            Vector2 drawPos = eyePos - screenPos;
            spriteBatch.Draw(headTex, drawPos, null, Color.White, angle, origin, 1f, SpriteEffects.None, 0f);

            // Draw body segments between head and tail
            float coveredLength = headTex.Width;
            float bodyLength = length - headTex.Width - tailTex.Width;

            while (coveredLength < headTex.Width + bodyLength)
            {
                drawPos = eyePos - screenPos + dir * coveredLength;
                spriteBatch.Draw(bodyTex, drawPos, null, Color.White, angle,
                    new Vector2(0f, bodyTex.Height / 2f), 1f, SpriteEffects.None, 0f);
                coveredLength += bodyTex.Width;
            }

            // Draw tail at the end
            drawPos = eyePos - screenPos + dir * (length - tailTex.Width);
            spriteBatch.Draw(tailTex, drawPos, null, Color.White, angle,
                new Vector2(0f, tailTex.Height / 2f), 1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}