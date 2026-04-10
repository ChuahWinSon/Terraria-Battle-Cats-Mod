using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.CameraModifiers;


namespace TheBattleCats.Common.Systems
{
    public class CameraPanMagnet : ICameraModifier
    {
        public string UniqueIdentity => "BattleCats_PanMagnet";
        public bool Finished => false;

        public Vector2 TargetPosition;
        public float PanProgress;
        public bool inUse;

        public void Update(ref CameraInfo cameraInfo)
        {
            
            if (PanProgress <= 0f) return;

            float smooth = SmoothStep(PanProgress);
            Vector2 halfScreen = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
            Vector2 playerCenter = Main.LocalPlayer.Center;

            cameraInfo.CameraPosition = Vector2.Lerp(playerCenter, TargetPosition, smooth) - halfScreen;
        }

        public void Reset()
        {
            TargetPosition = Vector2.Zero;
            PanProgress = 0f;
            inUse = false;
        }

        private float SmoothStep(float t) => t * t * (3f - 2f * t);
    }
}