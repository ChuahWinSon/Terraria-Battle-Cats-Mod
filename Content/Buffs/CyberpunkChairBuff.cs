
using Terraria;
using Terraria.ModLoader;

namespace TheBattleCats.Content.Buffs
{
    public class CyberpunkChairBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type]        = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.mount.SetMount(ModContent.MountType<Mounts.CyberpunkChair>(), player);
            player.buffTime[buffIndex] = 10; // keep refreshing
        }
    }
}