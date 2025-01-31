using Buyback.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria;
using Terraria.Chat;
using Terraria.ModLoader;

namespace Buyback
{
	public class Buyback : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			base.HandlePacket(reader, whoAmI);
			byte messageType = reader.ReadByte();

			switch (messageType)
			{
				case 0:
				{
					foreach (var p in Main.player)
						if (p.active)
							p.GetModPlayer<BuybackPlayer>().BuybackPlaySound = true;
					break;
				}
				case 1:
					var player = Main.player.ToList().First(p => p.whoAmI == whoAmI);
					ChatHelper.BroadcastChatMessage(FightState.JustBoughtBackMessage.ToNetworkText(player.name),
						Color.Gold);
					break;
			}
		}
	}
}