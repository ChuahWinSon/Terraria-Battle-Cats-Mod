using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TheBattleCats.Content.Items
{
    public class CatFood : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 20;
			Item.height = 20; 
            Item.maxStack = 9999;
            Item.value = 0;
        }

        
        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float newScale = 0.2f;
            Vector2 drawPos = new Vector2(
                Item.position.X - Main.screenPosition.X + Item.width / 2,
                Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * newScale / 2
            );

            spriteBatch.Draw(
                texture,
                drawPos,
                null,
                lightColor,
                rotation,
                texture.Size() / 2,
                newScale,
                SpriteEffects.None,
                0f
            );

            return false; // false = skip default drawing
        }
    }

    
}