using System.IO;
using System.Linq;
using Buyback.Content.Buffs;
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
		public int BuybackCost = Item.buyPrice(silver: 200);
		public int BuybackCooldown = 1200;
		public bool JustBoughtBack;

		public override void PreUpdate()
		{
			if (BuybackCooldown > 0)
				--BuybackCooldown;
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			base.OnHurt(info);
			if (info.DamageSource.SourcePlayerIndex != -1 ||
			    info.DamageSource.SourcePlayerIndex != Main.player.ToList().IndexOf(Player))
				Player.DelBuff(ModContent.BuffType<AegisRegenBuff>());
		}

		public override void Kill(
			double damage,
			int hitDirection,
			bool pvp,
			PlayerDeathReason damageSource)
		{
			if (Player.ConsumeItem(ModContent.ItemType<AegisOfTheImmortal>()))
				return;

			int netWorth = Player.inventory.Sum(i => i.value) +
			               Player.bank.item.Sum(i => i.value) + Player.bank2.item.Sum(i => i.value) + 
			               Player.bank3.item.Sum(i => i.value) + Player.bank4.item.Sum(i => i.value);

			BuybackCost = Item.buyPrice(silver: 200 + netWorth / 13);

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