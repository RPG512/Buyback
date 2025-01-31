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
		public bool BuybackPlaySound;

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
				--BuybackCooldown;
			if (!BuybackPlaySound)
				return;
			SoundStyle soundStyle = new("Buyback/buyback")
			{
				Type = SoundType.Sound,
				MaxInstances = 100,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				Volume = 0.8f
			};
			Player.GetModPlayer<BuybackPlayer>().PlaySound(in soundStyle);
			BuybackPlaySound = false;
		}

		public SlotId PlaySound(in SoundStyle style, Vector2? position = null) =>
			SoundEngine.PlaySound(in style, position);

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

			long netWorth = Player.inventory.Sum(i => i.value * i.stack) +
							 Player.bank.item.Sum(i => i.value * i.stack) +
							 Player.bank2.item.Sum(i => i.value * i.stack) +
							 Player.bank3.item.Sum(i => i.value * i.stack) +
							 Player.bank4.item.Sum(i => i.value * i.stack);

			long dividedNetWorth = netWorth / 13 / ModContent.GetInstance<BuybackModConfig>().NetWorthDivider;

			BuybackCost = 20000 + dividedNetWorth - dividedNetWorth % 100;

			if (JustBoughtBack)
			{
				Player.respawnTimer = 3600;
				JustBoughtBack = false;
			}
			//else
			//	Player.respawnTimer = 1800;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			var packet = Mod.GetPacket();
			packet.Write((byte)Player.whoAmI);
			packet.Write((byte)BuybackCooldown);
			packet.Write((byte)Player.respawnTimer);
			packet.Write(JustBoughtBack);
			packet.Write(BuybackPlaySound);
		}

		public void ReceivePlayerSync(BinaryReader reader)
		{
			BuybackCooldown = reader.ReadByte();
			Player.respawnTimer = reader.ReadByte();
			JustBoughtBack = reader.ReadBoolean();
			BuybackPlaySound = reader.ReadBoolean();
		}

		public override void CopyClientState(ModPlayer targetCopy)
		{
			var buybackPlayer = (BuybackPlayer)targetCopy;
			buybackPlayer.BuybackCooldown = BuybackCooldown;
			buybackPlayer.JustBoughtBack = JustBoughtBack;
			buybackPlayer.BuybackPlaySound = BuybackPlaySound;
			buybackPlayer.Player.respawnTimer = Player.respawnTimer;
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			var buybackPlayer = (BuybackPlayer)clientPlayer;
			if (BuybackCooldown != buybackPlayer.BuybackCooldown)
				base.SyncPlayer(-1, Main.myPlayer, false);
			if (JustBoughtBack != buybackPlayer.JustBoughtBack)
				base.SyncPlayer(-1, Main.myPlayer, false);
			if (BuybackPlaySound != buybackPlayer.BuybackPlaySound)
				base.SyncPlayer(-1, Main.myPlayer, false);
			if (Player.respawnTimer != buybackPlayer.Player.respawnTimer)
				base.SyncPlayer(-1, Main.myPlayer, false);
		}

		public override void SaveData(TagCompound tag)
		{
			tag["buyback-cooldown"] = BuybackCooldown;
			tag["just-bought-back"] = JustBoughtBack;
			tag["buyback-play-sound"] = BuybackPlaySound;
		}

		public override void LoadData(TagCompound tag)
		{
			BuybackCooldown = tag.GetInt("buyback-cooldown");
			JustBoughtBack = tag.GetBool("just-bought-back");
			BuybackPlaySound = tag.GetBool("buyback-play-sound");
		}
	}
}