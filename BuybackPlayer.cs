using System;
using System.IO;
using System.Linq;
using Buyback.Common;
using Buyback.Content.Buffs;
using Buyback.Content.Items;
using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Buyback
{
	public class BuybackPlayer : ModPlayer
	{
		public long BuybackCost = Item.buyPrice(silver: 200);
		public int BuybackCooldown = 1200;
		public bool JustBoughtBack;
        public string Reason
        {
            get
            {
                if (BuybackCooldown > 0)
                {
                    var time = TimeSpan.FromSeconds(BuybackCooldown / 60d);
                    return $"{time.Minutes:0}:{time.Seconds:00}";
                }
				ProccessBuybackCost();
                return $"{BuybackCost / 10000}";
            }
        }

        public Vector2 RespawnPosition
		{
			get
			{
				Player.FindSpawn();
				return Player.SpawnX < 0
					? new(Main.spawnTileX * 15.99912203687f, Main.spawnTileY * 15.99912203687f)
					: new(Player.SpawnX * 15.99912203687f, Player.SpawnY * 15.99912203687f);
			}
		}

		public override void PreUpdate()
		{
			if (BuybackCooldown > 0)
			{
				BuybackCooldown--;
				Player.AddBuff(ModContent.BuffType<NoBuybackBuff>(), 6);
				return;
			}
			if (!Player.CanAfford(BuybackCost))
				Player.AddBuff(ModContent.BuffType<NoBuybackBuff>(), 6);
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			base.OnHurt(info);
			if (info.DamageSource.SourcePlayerIndex != -1 &&
				info.DamageSource.SourcePlayerIndex != Player.whoAmI)
				Player.ClearBuff(ModContent.BuffType<AegisRegenBuff>());
		}

		public override void Kill(
			double damage,
			int hitDirection,
			bool pvp,
			PlayerDeathReason damageSource)
        {
            if (Player.ConsumeItem(ModContent.ItemType<AegisOfTheImmortal>()))
                return;

            ProccessBuybackCost();

            if (JustBoughtBack)
            {
                Player.respawnTimer = 3600;
                JustBoughtBack = false;
            }
            //else
            //	Player.respawnTimer = 1800;
        }

		private void ProccessBuybackCost()
		{
			long netWorth = Player.inventory.Sum(i => i.value * i.stack) +
										 Player.bank.item.Sum(i => i.value * i.stack) +
										 Player.bank2.item.Sum(i => i.value * i.stack) +
										 Player.bank3.item.Sum(i => i.value * i.stack) +
										 Player.bank4.item.Sum(i => i.value * i.stack);

			long dividedNetWorth = netWorth / 13 / ModContent.GetInstance<BuybackModConfig>().NetWorthDivider;

			BuybackCost = 20000 + dividedNetWorth - dividedNetWorth % 100;

			if (BuybackCost < 20000)
				BuybackCost = long.MaxValue - 7;
		}

        public override void OnRespawn()
		{
			if (JustBoughtBack)
				//SoundEngine.PlaySound(in Buyback.BuybackSound);
				Player.QuickSpawnItem(Terraria.Entity.GetSource_None(), ModContent.ItemType<AegisOfTheImmortal>());
		}
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			var packet = Mod.GetPacket();
			packet.Write((byte)Player.whoAmI);
			packet.Write(BuybackCost);
			packet.Write(BuybackCooldown);
			packet.Write(Player.respawnTimer);
			packet.Write(JustBoughtBack);
		}

		public void ReceivePlayerSync(BinaryReader reader)
		{
			BuybackCost = reader.ReadInt64();
			BuybackCooldown = reader.ReadInt32();
			Player.respawnTimer = reader.ReadInt32();
			JustBoughtBack = reader.ReadBoolean();
		}

		public override void CopyClientState(ModPlayer targetCopy)
		{
			var buybackPlayer = (BuybackPlayer)targetCopy;
			buybackPlayer.BuybackCost = BuybackCost;
			buybackPlayer.BuybackCooldown = BuybackCooldown;
			buybackPlayer.JustBoughtBack = JustBoughtBack;
			buybackPlayer.Player.respawnTimer = Player.respawnTimer;
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			var buybackPlayer = (BuybackPlayer)clientPlayer;
			if (BuybackCost != buybackPlayer.BuybackCost)
				base.SyncPlayer(-1, Main.myPlayer, false);
			if (BuybackCooldown != buybackPlayer.BuybackCooldown)
				base.SyncPlayer(-1, Main.myPlayer, false);
			if (JustBoughtBack != buybackPlayer.JustBoughtBack)
				base.SyncPlayer(-1, Main.myPlayer, false);
			if (Player.respawnTimer != buybackPlayer.Player.respawnTimer)
				base.SyncPlayer(-1, Main.myPlayer, false);
		}

		public override void SaveData(TagCompound tag)
		{
			tag["buyback-cost"] = BuybackCost;
			tag["buyback-cooldown"] = BuybackCooldown;
			tag["just-bought-back"] = JustBoughtBack;
		}

		public override void LoadData(TagCompound tag)
		{
			BuybackCost = tag.GetLong("buyback-cost");
			BuybackCooldown = tag.GetInt("buyback-cooldown");
			JustBoughtBack = tag.GetBool("just-bought-back");
		}
	}
}