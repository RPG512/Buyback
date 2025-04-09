using Buyback.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria;
using Terraria.Chat;
using Terraria.ModLoader;
using Buyback.Content.Items;

namespace Buyback
{
	public class Buyback : Mod
	{
		public static readonly SoundStyle BuybackSound = new("Buyback/buyback")
		{
			Type = SoundType.Sound,
			MaxInstances = 100,
			SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
			Volume = 0.8f
		};
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			base.HandlePacket(reader, whoAmI);
			byte messageType = reader.ReadByte();
			Player player;
			if (whoAmI == 256)
				return;
			player = Main.player.ToList().First(p => p.whoAmI == whoAmI);

			switch (messageType)
			{
				case 1:
					ChatHelper.BroadcastChatMessage(FightState.JustBoughtBackMessage.ToNetworkText(player.name), Color.Gold);
					break;
			}
		}
	}
}