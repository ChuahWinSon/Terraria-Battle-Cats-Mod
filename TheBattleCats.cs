using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace TheBattleCats
{
    public class TheBattleCats : Mod
    {
        public static int CatFoodCurrencyId;

        public override void Load()
        {
            CatFoodCurrencyId = CustomCurrencyManager.RegisterCurrency(
                new Content.Currencies.CatFoodCurrency(
                    ModContent.ItemType<Content.Items.CatFood>(),
                    9999L,
                    "Mods.TheBattleCats.Currencies.CatFood"
                )
            );
        }

        public override void Unload()
        {
        }
    }
}