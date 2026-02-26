using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace TheBattleCats.Content.Projectiles
{
    public class ChronosAttackDone : ModProjectile
    {
    private const float DetectionRadius = 50*16 ; // Radius to detect enemies
    private bool isHoming = false; // Flag to check if it should home in on an enemy
    private NPC targetNPC; // The enemy the projectile is homing towards
    private float distanceTraveled = 0f;
    private const float DistanceBeforeHoming = 200f; // in pixels (adjust as needed)

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.aiStyle = -1;  // Custom AI
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300; // How long the projectile lasts
    }

public override void AI()
{
    Lighting.AddLight(Projectile.Center, 0.7f, 1f, 0.9f); 
    Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, Projectile.velocity.X * -0.3f, Projectile.velocity.Y * -0.3f, 100, default, 2f).noGravity = true;

    // Track how far the projectile has traveled
    distanceTraveled += Projectile.velocity.Length();

    if (!isHoming)
    {
        if (distanceTraveled >= DistanceBeforeHoming)
        {
            CheckForNearbyEnemies(); // Only start checking for targets after distance is reached
        }

    }
    else
    {
        HomingMovement(); // Homing logic
    }
}



    private void HomingMovement()
    {
        if (targetNPC != null && targetNPC.active && targetNPC.life > 0)
        {
            // Get direction to the target
            Vector2 direction = targetNPC.Center - Projectile.Center;

            // Normalize the direction to avoid speed spikes
            direction.Normalize();

            // Smooth the velocity to curve towards the target
            float turnSpeed = 0.1f; // Adjust to control how fast it turns
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 10f, turnSpeed);

            // Limit the maximum speed
            float maxSpeed = 15f;
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity.Normalize();
                Projectile.velocity *= maxSpeed;
            }
        }
    }

    private void CheckForNearbyEnemies()
    {
        // Only start homing if it's not already homing
        if (!isHoming)
        {
            // Loop through all NPCs to find the closest one
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.life <= 0) continue;

                // Check if the NPC is within the detection radius
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < DetectionRadius)
                {
                    // If an NPC is found, start homing towards it
                    targetNPC = npc;
                    isHoming = true;
                    break; // Exit loop after finding the first target
                }
            }
        }
    }


    }
}
