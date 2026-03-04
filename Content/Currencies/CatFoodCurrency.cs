using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;

namespace TheBattleCats.Content.Currencies
{
    public class CatFoodCurrency : CustomCurrencySingleCoin
    {
        public CatFoodCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap)
        {
            this.CurrencyTextKey = CurrencyTextKey;
            CurrencyTextColor = Color.Orange;
        }
    }
}