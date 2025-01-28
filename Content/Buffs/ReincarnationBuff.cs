using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Buyback.Content.Buffs
{
	public class ReincarnationBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			
		}

		public override string Texture => "Buyback/Content/Buffs/AegisRegenBuff";

		private int _tick;

		public override void Update(Player player, ref int buffIndex)
		{
			base.Update(player, ref buffIndex);
			player.immune = true;
			player.AddBuff(BuffID.Invisibility, 2);
			player.AddBuff(BuffID.Frozen, 2);
			player.immuneTime = 2;
			player.statLife = player.statLifeMax2;

			if (++_tick <= 300)
				return;
			player.immune = false;
			player.immuneTime = 0;
		}

		public override bool RightClick(int buffIndex) => false;
	}
}
