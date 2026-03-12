using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TheBattleCats.Content.Items.Consumables;
using TheBattleCats.Content.Items;

namespace TheBattleCats.Content.Tiles
{
    public class CatCapsuleUI : UIState
    {
        public override void OnInitialize()
        {
            var panel = new UIPanel();
            panel.SetPadding(10);
            panel.Left.Set(-200f, 0.5f);
            panel.Top.Set(-150f, 0.5f);
            panel.Width.Set(400f, 0f);
            panel.Height.Set(300f, 0f);
            Append(panel);

            var title = new UIText("Cat Capsule Machine", 1.2f);
            title.Left.Set(0f, 0f);
            title.Top.Set(0f, 0f);
            panel.Append(title);

            // Ticket A button
            var btnA = new UITextPanel<string>("Exchange Cat Ticket", 0.9f);
            btnA.Left.Set(10f, 0f);
            btnA.Top.Set(60f, 0f);
            btnA.Width.Set(350f, 0f);
            btnA.Height.Set(40f, 0f);
            btnA.OnLeftClick += (evt, el) => UseTicket(0);
            panel.Append(btnA);

            // Ticket B button
            var btnB = new UITextPanel<string>("Exchange Rare Cat Ticket", 0.9f);
            btnB.Left.Set(10f, 0f);
            btnB.Top.Set(120f, 0f);
            btnB.Width.Set(350f, 0f);
            btnB.Height.Set(40f, 0f);
            btnB.OnLeftClick += (evt, el) => UseTicket(1);
            panel.Append(btnB);

            // Ticket C button
            var btnC = new UITextPanel<string>("Exchange Platinum Cat Ticket", 0.9f);
            btnC.Left.Set(10f, 0f);
            btnC.Top.Set(180f, 0f);
            btnC.Width.Set(350f, 0f);
            btnC.Height.Set(40f, 0f);
            btnC.OnLeftClick += (evt, el) => UseTicket(2);
            panel.Append(btnC);

            // Close button
            var btnClose = new UITextPanel<string>("Close", 0.9f);
            btnClose.Left.Set(10f, 0f);
            btnClose.Top.Set(240f, 0f);
            btnClose.Width.Set(350f, 0f);
            btnClose.Height.Set(40f, 0f);
            btnClose.OnLeftClick += (evt, el) => CatCapsuleUISystem.Close();
            panel.Append(btnClose);
        }

        private void UseTicket(int tier)
        {
            Player player = Main.LocalPlayer;

            // Define ticket item types per tier
            int ticketType = tier switch
            {
                0 => ModContent.ItemType<CatTicket>(),
                1 => ModContent.ItemType<RareCatTicket>(),
                2 => ModContent.ItemType<PlatinumCatTicket>(),
                _ => -1
            };

            if (!player.HasItem(ticketType))
            {
                Main.NewText("You don't have that ticket!", 255, 50, 50);
                return;
            }

            player.ConsumeItem(ticketType);
            int reward = RollReward(tier);
            player.QuickSpawnItem(player.GetSource_Misc("CatCapsule"), reward);
            Main.NewText("You got something!", 100, 255, 100);
        }

        private int RollReward(int tier)
        {
            // Each tier has weighted loot tables
            // Weight works like: higher number = more common
            // Total weight per tier should add up to 100 for easy percentages

            if (tier == 0) // Basic Ticket
            {
                var loot = new (int item, int weight)[]
                {
                    (ItemID.IronBar,        40),  // 40% chance
                    (ItemID.GoldBar,        35),  // 35% chance
                    (ItemID.Torch,          15),  // 15% chance
                    (ItemID.HealingPotion,  10),  // 10% chance
                };
                return WeightedRoll(loot);
            }
            else if (tier == 1) // Rare Ticket
            {
                var loot = new (int item, int weight)[]
                {
                    (ItemID.MagicMirror,    40),
                    (ItemID.CloudinaBottle, 30),
                    (ItemID.HermesBoots,    20),
                    (ItemID.ShinyRedBalloon, 10),
                };
                return WeightedRoll(loot);
            }
            else // Platinum Ticket
            {
                var loot = new (int item, int weight)[]
                {
                    (ItemID.Meowmere,       10),
                    (ItemID.Zenith,          5),
                    (ItemID.StarWrath,      20),
                    (ItemID.Terrarian,      25),
                    (ItemID.SDMG,           40),
                };
                return WeightedRoll(loot);
            }
        }

        private int WeightedRoll((int item, int weight)[] table)
        {
            int total = 0;
            foreach (var entry in table)
                total += entry.weight;

            int roll = Main.rand.Next(total);
            int cumulative = 0;

            foreach (var entry in table)
            {
                cumulative += entry.weight;
                if (roll < cumulative)
                    return entry.item;
            }

            return table[0].item; // fallback
        }
    }
}