using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;

namespace TheBattleCats.Content.Items.Weapons
{
    public class BunnyWeaponLogic : ModPlayer
    {
        private enum MoveState { Inactive, Flipping, Shooting }

        private MoveState State      = MoveState.Inactive;
        private int       Timer      = 0;
        private int       ShootTimer = 0;
        private int       BurstCount = 0;

        private const int   FlipDuration    = 25;
        private const float FlipDegrees     = 140f;
        private const int   ShootInterval   = 6;   // ticks between each bullet
        private const int   BurstSize       = 10;  // bullets per burst
        private const float PlayerKnockback = 2f;
        private int   LockedDirection;
        private float TargetAngle;   // the angle we're flipping toward (cursor direction)

        public override void PostUpdate()
        {
            if (Player.HeldItem.type == ModContent.ItemType<BunnyWeapon>())
                Player.itemRotation = MathHelper.ToRadians(-90f) * Player.direction;
        }

        public void StartFlip()
        {
            // Only start if not already active AND burst is used up (or first time)
            if (State != MoveState.Inactive) return;

            State         = MoveState.Flipping;
            Timer         = 0;
            ShootTimer    = 0;
            BurstCount    = 0;
            LockedDirection = Player.direction;

            // Calculate the target angle toward the cursor
            Vector2 toCursor = Vector2.Normalize(Main.MouseWorld - Player.Center);
            TargetAngle = toCursor.ToRotation() + MathHelper.ToRadians(90f);

            Player.velocity.Y = -14f;
            Player.velocity.X = Main.MouseWorld.X > Player.Center.X ? 8f : -8f;
        }

        public override void PreUpdate()
        {
            if (State == MoveState.Inactive) return;

            Timer++;

            switch (State)
            {
                case MoveState.Flipping:  DoFlip();     break;
                case MoveState.Shooting:  DoShooting(); break;
            }
        }

        private void DoFlip()
        {
            Player.controlJump    = false;
            Player.controlUseItem = false;
            Player.direction      = LockedDirection;

            float progress = (float)Timer / FlipDuration;
            float eased    = 1f - (1f - progress) * (1f - progress);

            // Interpolate from 0 toward TargetAngle
            float currentAngle = MathHelper.Lerp(0f, TargetAngle, eased);

            Player.fullRotation       = currentAngle;
            Player.fullRotationOrigin = new Vector2(Player.width / 2f, Player.height / 2f);

            // Switch to shooting once we've reached the target angle OR timer runs out
            float angleDiff = Math.Abs(MathHelper.WrapAngle(Player.fullRotation - TargetAngle));
            if (angleDiff < MathHelper.ToRadians(5f) || Timer >= FlipDuration)
            {
                Player.fullRotation = TargetAngle;
                State      = MoveState.Shooting;
                Timer      = 0;
                ShootTimer = 0;
                BurstCount = 0;
            }
        }

        private void DoShooting()
        {
            Player.direction = LockedDirection;

            // Lock rotation to the angle we flipped to
            Vector2 toCursor = Vector2.Normalize(Main.MouseWorld - Player.Center);
            Player.fullRotation = toCursor.ToRotation() + MathHelper.ToRadians(90f);
            Player.fullRotationOrigin = new Vector2(Player.width / 2f, Player.height / 2f);

            // Burst is done — wait for next flip to fire again
            if (BurstCount >= BurstSize)
            {
                State               = MoveState.Inactive;
                Timer               = 0;
                Player.fullRotation = 0f;
                return;
            }

            ShootTimer++;
            if (ShootTimer >= ShootInterval)
            {
                ShootTimer = 0;
                BurstCount++;
                FireShot();
            }
        }

        private void FireShot()
        {
            Vector2 shootDir = Vector2.Normalize(Main.MouseWorld - Player.Center);

            int   damage    = Player.HeldItem.damage;
            float knockback = Player.HeldItem.knockBack;
            float speed     = Player.HeldItem.shootSpeed;
            int   projType  = Player.HeldItem.shoot;

            if (projType <= 0) projType = ProjectileID.Bullet;

            Vector2 spawnPos = Player.Center + new Vector2(Player.direction * 20f, -10f);

            Projectile.NewProjectile(
                Player.GetSource_ItemUse(Player.HeldItem),
                spawnPos,
                shootDir * speed,
                projType,
                damage,
                knockback,
                Player.whoAmI
            );

            Player.velocity += -shootDir * PlayerKnockback;

            if (Player.velocity.X >  8f) Player.velocity.X =  8f;
            if (Player.velocity.X < -8f) Player.velocity.X = -8f;
            if (Player.velocity.Y < -6f) Player.velocity.Y = -6f;
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (State == MoveState.Inactive) return;

            drawInfo.drawPlayer.bodyFrame.Y = 5 * drawInfo.drawPlayer.bodyFrame.Height;
            drawInfo.drawPlayer.legFrame.Y  = 5 * drawInfo.drawPlayer.legFrame.Height;
            drawInfo.drawPlayer.headFrame.Y = 5 * drawInfo.drawPlayer.headFrame.Height;
        }
    }
}