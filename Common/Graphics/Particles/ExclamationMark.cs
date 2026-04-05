using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static System.MathF;
using System;

namespace TheBattleCats.Common.Graphics.Particles
{

    public class ExclamationParticle : Particle
    {
        public override string Texture => "TheBattleCats/Common/Graphics/Particles/ExclamationMark";
        public override bool UseAdditiveBlend => false;

        public ExclamationParticle(Vector2 position)
        {
            Position = position;
            Lifetime = 240;
            Scale = 1f;
        }

        public override void Update() { }

        public override void CustomDraw(SpriteBatch spriteBatch)
        {
            // Flash by using a sine wave on alpha
            float flash = Math.Abs((float)Math.Sin(LifetimeCompletion * Math.PI * 6)); // 3 flashes
            float alpha = flash * (1f - LifetimeCompletion); // fade out overall

            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            spriteBatch.Draw(tex, Position - Main.screenPosition, null,
        new Color(255, 0, 0) * alpha, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
        }
    }

}