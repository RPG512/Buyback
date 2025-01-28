using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Buyback.Content.Buffs
{
	public class AegisRegenBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{

		}

		private int _tick;
		private int _baseLifeRegen;
		private int _baseManaRegen;

		public override void Update(Player player, ref int buffIndex)
		{
			if (_tick == 0)
			{
				_baseLifeRegen = player.lifeRegen;
				_baseManaRegen = player.manaRegen;
			}

			if (++_tick <= 300)
			{
				player.lifeRegen = player.statLifeMax2 / 5;
				player.manaRegen = player.statManaMax2 / 5;
			}
			else
			{
				player.lifeRegen = _baseLifeRegen;
				player.manaRegen = _baseManaRegen;
			}
		}

		public override bool RightClick(int buffIndex) => false;
	}
}
