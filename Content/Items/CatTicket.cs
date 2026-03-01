using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace TheBattleCats.Content.Items
{
	public class CatTicket : ModItem
	{
		public override void SetStaticDefaults()
        {
			// The text shown below some item names is called a tooltip. Tooltips are defined in the localization files. See en-US.hjson.

			// How many items are needed in order to research duplication of this item in Journey mode. See https://terraria.wiki.gg/wiki/Journey_Mode#Research for a list of commonly used research amounts depending on item type. This defaults to 1, which is what most items will use, so you can omit this for most ModItems.
			Item.ResearchUnlockCount = 100;

        }

        public override void SetDefaults() {
			Item.width = 20; // The item texture's width
			Item.height = 20; // The item texture's height

			Item.maxStack = 99; // The item's max stack value
			Item.value = Item.buyPrice(silver: 1); // The value of the item in copper coins. Item.buyPrice & Item.sellPrice are helper methods that returns costs in copper coins based on platinum/gold/silver/copper arguments provided to it.
		}

        public override void PostUpdate()
{
    Item.scale = 0.5f;
}

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
{
    Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
    float newScale = 0.4f;
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