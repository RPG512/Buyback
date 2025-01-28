using System;
using System.Collections.Generic;
using System.IO;
using Buyback.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;


namespace Buyback.Content.Items;

public class AegisOfTheImmortal : ModItem
{
	public int Timer = 18000;
	public static LocalizedText LifeTime { get; private set; }

	public override void SetStaticDefaults()
	{
		Item.ResearchUnlockCount = 100;
		LifeTime = this.GetLocalization(nameof(LifeTime));
	}

	public override void SetDefaults()
	{
		Item.width = 20;
		Item.height = 20;

		Item.maxStack = 1;
		Item.value = 0;
		Item.rare = ItemRarityID.Purple;
		Item.consumable = true;
	}

	public override void OnCreated(ItemCreationContext context)
	{
		Timer = 18000;
	}

	public override bool ConsumeItem(Player player)
	{
		return player.dead || Timer <= 0;
	}

	public override void OnConsumeItem(Player player)
	{
		if (player.dead)
		{
			player.dead = false;
			player.statLife = player.statLifeMax2;

			player.AddBuff(ModContent.BuffType<ReincarnationBuff>(), 300);
			SoundStyle soundStyle = new("Buyback/Content/Items/AegisOfTheImmortalReturn")
			{
				Type = SoundType.Sound,
				MaxInstances = 100,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				Volume = 0.6f
			};
			SoundEngine.PlaySound(in soundStyle, player.Center);
			//WorldGen.KillTile((int)(player.position.X), (int)(player.position.Y));
		}
		else
		{
			player.AddBuff(ModContent.BuffType<AegisRegenBuff>(), 300);
			SoundStyle soundStyle = new("Buyback/Content/Items/AegisOfTheImmortalExpire")
			{
				Type = SoundType.Sound,
				MaxInstances = 100,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				Volume = 0.6f
			};
			SoundEngine.PlaySound(in soundStyle, player.Center);
		}
	}

	public override void UpdateInventory(Player player)
	{
		bool turnToAir = false;
		for (var i = 0; i < player.inventory.Length; i++)
		{
			if(turnToAir && player.inventory[i].type == Item.type)
			{
				player.inventory[i].TurnToAir();
				player.inventory[i] = new Item(ItemID.MilkCarton, 9);
			}
			if (player.inventory[i].type == Item.type)
				turnToAir = true;
		}
		for (var i = 0; i < player.bank.item.Length; i++)
		{
			if(turnToAir && player.bank.item[i].type == Item.type)
			{
				player.bank.item[i].TurnToAir();
				player.bank.item[i] = new Item(ItemID.MilkCarton, 9);
			}
			if (player.bank.item[i].type == Item.type)
				turnToAir = true;
		}
		for (var i = 0; i < player.bank2.item.Length; i++)
		{
			if(turnToAir && player.bank2.item[i].type == Item.type)
			{
				player.bank2.item[i].TurnToAir();
				player.bank2.item[i] = new Item(ItemID.MilkCarton, 9);
			}
			if (player.bank2.item[i].type == Item.type)
				turnToAir = true;
		}
		for (var i = 0; i < player.bank3.item.Length; i++)
		{
			if(turnToAir && player.bank3.item[i].type == Item.type)
			{
				player.bank3.item[i].TurnToAir();
				player.bank3.item[i] = new Item(ItemID.MilkCarton, 9);
			}
			if (player.bank3.item[i].type == Item.type)
				turnToAir = true;
		}
		for (var i = 0; i < player.bank4.item.Length; i++)
		{
			if(turnToAir && player.bank4.item[i].type == Item.type)
			{
				player.bank4.item[i].TurnToAir();
				player.bank4.item[i] = new Item(ItemID.MilkCarton, 9);
			}
			if (player.bank4.item[i].type == Item.type)
				turnToAir = true;
		}
		if (turnToAir && player.trashItem.type == Item.type)
		{
			player.trashItem.TurnToAir();
			player.trashItem = new Item(ItemID.MilkCarton, 9);
		}
		if (--Timer <= 0)
			player.ConsumeItem(Item.type);
	}

	public override void Update(ref float gravity, ref float maxFallSpeed)
	{
		if (--Timer <= 0)
		{
			Item.TurnToAir();
			SoundStyle soundStyle = new("Buyback/Content/Items/AegisOfTheImmortalTimer")
			{
				Type = SoundType.Sound,
				MaxInstances = 100,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				Volume = 0.8f
			};
			SoundEngine.PlaySound(in soundStyle, Item.Center);
		}
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		var time = TimeSpan.FromSeconds(Timer / 60d);
		var tooltip = new TooltipLine(Mod, "Buyback: AegisOfTheImmortal.LifeTime", LifeTime.Format($"{time.Minutes:0}:{time.Seconds:00}"))
			{ OverrideColor = Color.Red };
		tooltips.Add(tooltip);
	}

	public override void AddRecipes()
	{
		var recipe = CreateRecipe()
			.AddIngredient(ItemID.MilkCarton, 9)
			.AddTile(TileID.CookingPots)
			.AddCondition(Condition.NearShimmer);
		((AegisOfTheImmortal)recipe.createItem.ModItem).Timer = 18000;
		recipe.Register();
	} //Хе-хе сыр!

	public override void OnResearched(bool fullyResearched)
	{
		if (fullyResearched)
			CreativeUI.ResearchItem(ModContent.ItemType<AegisOfTheImmortal>());
	}

	public override void SaveData(TagCompound tag)
	{
		tag["Timer"] = Timer;
	}

	public override void LoadData(TagCompound tag)
	{
		if (tag.ContainsKey("Timer"))
		{
			Timer = tag.GetInt("Timer");
		}
	}

	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(Timer);
	}

	public override void NetReceive(BinaryReader reader)
	{
		Timer = reader.ReadInt32();
	}
}