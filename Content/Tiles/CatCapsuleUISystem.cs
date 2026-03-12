using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace TheBattleCats.Content.Tiles
{
    public class CatCapsuleUISystem : ModSystem
    {
        private static UserInterface _ui;
        private static CatCapsuleUI _state;
        private static bool _visible = false;

        public override void Load()
        {
            _ui = new UserInterface();
            _state = new CatCapsuleUI();
            _state.Activate();
        }

        public override void Unload()
        {
            _state = null;
            _ui = null;
        }

        public static void Open()
        {
            _visible = true;
            _ui.SetState(_state);
            Main.playerInventory = true;
        }

        public static void Close()
        {
            _visible = false;
            _ui.SetState(null);
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (_visible)
                _ui?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(l => l.Name == "Vanilla: Mouse Text");
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "TheBattleCats: CatCapsuleUI",
                    () =>
                    {
                        if (_visible)
                            _ui.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }
    }
}