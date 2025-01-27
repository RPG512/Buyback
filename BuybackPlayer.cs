using System.IO;
using Buyback.Content.Items;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

#nullable disable
namespace Buyback
{
	public class BuybackPlayer : ModPlayer
	{
		public int BuybackCost = 30000;
		public int BuybackCooldown = 1200;
		public bool JustBoughtBack;

		public override void PreUpdate()
		{
			if (BuybackCooldown > 0)
				--BuybackCooldown;
			if (!Main.hardMode)
				return;
			BuybackCost = 100000;
		}

		public override void Kill(
			double damage,
			int hitDirection,
			bool pvp,
			PlayerDeathReason damageSource)
		{
			if (Player.ConsumeItem(ModContent.ItemType<AegisOfTheImmortal>()))
				return;

			if (JustBoughtBack)
			{
				Player.respawnTimer = 3600;
				JustBoughtBack = false;
			}
			else
				Player.respawnTimer = 1800;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			var packet = Mod.GetPacket();
			packet.Write((byte)Player.whoAmI);
			packet.Write((byte)BuybackCooldown);
			packet.Write((byte)Player.respawnTimer);
			packet.Write(JustBoughtBack);
		}

		public void ReceivePlayerSync(BinaryReader reader)
		{
			BuybackCooldown = reader.ReadByte();
			Player.respawnTimer = reader.ReadByte();
			JustBoughtBack = reader.ReadBoolean();
		}

		public override void CopyClientState(ModPlayer targetCopy)
		{
			var buybackPlayer = (BuybackPlayer)targetCopy;
			buybackPlayer.BuybackCooldown = BuybackCooldown;
			buybackPlayer.JustBoughtBack = JustBoughtBack;
			buybackPlayer.Player.respawnTimer = Player.respawnTimer;
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			var buybackPlayer = (BuybackPlayer)clientPlayer;
			if (BuybackCooldown != buybackPlayer.BuybackCooldown)
				base.SyncPlayer(-1, Main.myPlayer, false);
			if (JustBoughtBack != buybackPlayer.JustBoughtBack)
				base.SyncPlayer(-1, Main.myPlayer, false);
			if (Player.respawnTimer != buybackPlayer.Player.respawnTimer)
				base.SyncPlayer(-1, Main.myPlayer, false);
		}

		public override void SaveData(TagCompound tag)
		{
			tag["buyback-cooldown"] = BuybackCooldown;
			tag["just-bought-back"] = JustBoughtBack;
		}

		public override void LoadData(TagCompound tag)
		{
			BuybackCooldown = tag.GetInt("buyback-cooldown");
			JustBoughtBack = tag.GetBool("just-bought-back");
		}
	}
}