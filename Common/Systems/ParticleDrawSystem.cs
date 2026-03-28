using Terraria.ModLoader;
using TheBattleCats.Common.Graphics.Particles;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace TheBattleCats.Common.Systems
{
    public class ParticleDrawSystem : ModSystem
    {
        public override void PostUpdateEverything()
        {
            ParticleHandler.UpdateParticles();
        }

        public override void PostDrawTiles()
        {
            Main.spriteBatch.GraphicsDevice.Textures[0] = null;
            ParticleHandler.DrawParticles(Main.spriteBatch);
        }
    }
}