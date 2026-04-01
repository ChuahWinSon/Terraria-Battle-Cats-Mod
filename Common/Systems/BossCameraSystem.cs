using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TheBattleCats.Content.NPCs.CycloneBoss;

namespace TheBattleCats.Common.Systems
{
    public class BossCameraSystem : ModSystem
    {
        public static CameraPanMagnet PanMagnet = new CameraPanMagnet();
        public static float Shake;

        private static int panTimer;
        private static int _panDuration = 0;
        private static int _holdDuration = 0;

        public static void StartBossPan(Vector2 bossPosition, int panDuration = 60, int holdDuration = 180)
        {
            PanMagnet.TargetPosition = bossPosition;
            panTimer = panDuration + holdDuration;
            _panDuration = panDuration;
            _holdDuration = holdDuration;
        }

        public static void TriggerShake(float strength)
        {
            Shake = strength;
        }

        public override void ModifyScreenPosition()
        {
            Main.instance.CameraModifiers.Add(PanMagnet);

            if (Shake > 0)
            {
                Main.instance.CameraModifiers.Add(new ShakeCamera(Shake, 2, "BattleCats_Shake"));
                Shake = Math.Max(Shake - 1, 0);
            }
        }

        public override void PostUpdateEverything()
        {
            // Find the boss
            NPC boss = null;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.type == ModContent.NPCType<Cyclone>())
                {
                    boss = npc;
                    break;
                }
            }

            if (panTimer > 0)
            {
                if (boss != null)
                    PanMagnet.TargetPosition = boss.Center;

                // Tick PanProgress up toward 1 during pan phase
                if (panTimer > _holdDuration)
                {
                    float panPhaseProgress = 1f - ((panTimer - _holdDuration) / (float)_panDuration);
                    PanMagnet.PanProgress = panPhaseProgress;
                }
                else
                {
                    // Hold phase - stay fully panned
                    PanMagnet.PanProgress = 1f;
                }

                // This is what tells PostUpdateEverything the magnet is active
                PanMagnet.inUse = true;
                panTimer--;
            }

            // Fables-style auto return - just like CameraManager.cs
            // When nothing is calling inUse = true, smoothly tick PanProgress back down
            if (!PanMagnet.inUse && PanMagnet.PanProgress > 0f)
            {
                PanMagnet.PanProgress -= 1f / (60f * 0.5f); // returns over 0.5 seconds
                if (PanMagnet.PanProgress <= 0f)
                    PanMagnet.Reset();
            }

            PanMagnet.inUse = false;
        }
    }
}