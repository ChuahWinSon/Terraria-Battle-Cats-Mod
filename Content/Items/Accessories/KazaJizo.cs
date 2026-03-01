using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TheBattleCats.Content.Items.Accessories
{
    [AutoloadEquip(EquipType.Back)]
    public class KazaJizo : ModItem
    {
        public override void SetDefaults()
        {
            Item.width    = 24;
            Item.height   = 34;
            Item.accessory = true;
            Item.value    = Item.buyPrice(0, 5, 0, 0);
            Item.rare     = ItemRarityID.Green;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // apply effects here while equipped
            player.GetModPlayer<KazaJizoPlayer>().HasJizo = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IronBar, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }

    public class KazaJizoPlayer : ModPlayer
    {
        public bool HasJizo = false;

        public override void ResetEffects()
        {
            // resets every tick so UpdateAccessory has to keep setting it true
            HasJizo = false;
        }

        public override void PostUpdate()
        {
            if (!HasJizo) return;

            // animation and shoot logic goes here
        }
    }
}