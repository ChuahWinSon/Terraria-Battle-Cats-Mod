using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace TheBattleCats.Common.Graphics.Particles
{
    public abstract class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Scale;
        public float Rotation;
        public int Lifetime;
        public int TimeAlive;

        public float LifetimeCompletion => TimeAlive / (float)Lifetime;
        public bool IsDead => TimeAlive >= Lifetime;

        public abstract string Texture { get; }
        public virtual bool UseAdditiveBlend => false;

        public abstract void Update();
        public abstract void CustomDraw(SpriteBatch spriteBatch);

        public void Tick()
        {
            Update();
            TimeAlive++;
        }
    }

    public static class ParticleHandler
    {
        private static readonly List<Particle> _particles = new();

        public static void SpawnParticle(Particle particle) => _particles.Add(particle);

        public static void UpdateParticles()
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Tick();
                if (_particles[i].IsDead)
                    _particles.RemoveAt(i);
            }
        }

        public static void DrawParticles(SpriteBatch spriteBatch)
        {
            // Draw normal blend particles
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, 
                SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var p in _particles)
                if (!p.UseAdditiveBlend)
                    p.CustomDraw(spriteBatch);
            
            spriteBatch.End();

            // Draw additive blend particles separately
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, 
                SamplerState.PointClamp, null, null, null, Main.GameViewMatrix.TransformationMatrix);
            
            foreach (var p in _particles)
                if (p.UseAdditiveBlend)
                    p.CustomDraw(spriteBatch);
            
            spriteBatch.End();
        }
    }
}