﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

#nullable disable
namespace Buyback
{
	internal class FightState : UIState, ILocalizedModType, ILoadable
	{
		private UIText _text;
		private readonly UIPanel _parent = new();
		public FightElement FightElement;
		private UIImage _coinImage;
		public static LocalizedText JustBoughtBackMessage { get; private set; }
		public static LocalizedText BuybackCost { get; private set; }
		public static LocalizedText BuybackCooldown { get; private set; }

		public string LocalizationCategory => "UIStates";

		public Mod Mod { get; internal set; }

		public string Name => GetType().Name;

		public string FullName => $"{Mod?.Name ?? "Terraria"}/{Name}";

		public override void Update(GameTime gameTime)
		{
			if (Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown > 0)
			{
				var time = TimeSpan.FromSeconds(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown / 60d);
				_text.SetText(BuybackCooldown.Format($"{time.Minutes:0}:{time.Seconds:00}"));
				_text.TextColor = Color.Red;
				_coinImage.SetImage(Terraria.GameContent.TextureAssets.Item[ItemID.GoldWatch]);
				_coinImage.Left.Set(_text.Left.Pixels + _text.Width.Pixels + 5f, 0f);
				_coinImage.Top.Set(_text.Top.Pixels, 0f);
				_coinImage.Recalculate();
			}
			else if(!Main.LocalPlayer.CanAfford(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost))
			{
				_text.SetText(BuybackCooldown.Format(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost / 10000));
				_text.TextColor = Color.Red;
				_coinImage.SetImage(Terraria.GameContent.TextureAssets.Item[ItemID.GoldCoin]);
				_coinImage.Left.Set(_text.Left.Pixels + _text.Width.Pixels + 5f, 0f);
				_coinImage.Top.Set(_text.Top.Pixels, 0f);
				_coinImage.Recalculate();
			}
			else
			{
				_text.SetText(BuybackCost.Format(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost / 10000));
				_text.TextColor = Color.Gold;
				_coinImage.SetImage(Terraria.GameContent.TextureAssets.Item[ItemID.GoldCoin]);
				_coinImage.Left.Set(_text.Left.Pixels + _text.Width.Pixels + 5f, 0f);
				_coinImage.Top.Set(_text.Top.Pixels, 0f);
				_coinImage.Recalculate();
			}
		}

		public override void OnInitialize()
		{
			FightElement = new FightElement();
			Append(FightElement);

			_parent.Height.Set(55f, 0.0f);
			_parent.Width.Set(240f, 0.0f);
			_parent.Left.Set(Main.screenWidth / 2f + 55f, 0.0f);
			_parent.Top.Set(Main.screenHeight / 2f + 155f, 0.0f);
			_parent.BackgroundColor = new Color(41, 49, 51, 200);
			_parent.BorderColor = new Color(0, 0, 0, 0);

			_parent.OnLeftClick += OnButtonClick;

			_text = new UIText("Buyback: ");
			_text.Width.Set(190f, 0.0f);
			_text.Height.Set(55f, 0.0f);
			_text.Top.Set(0.0f, 0.2f);
			_text.TextColor = Color.Gold;

			_parent.Append(_text);

			_coinImage = new(Terraria.GameContent.TextureAssets.Item[ItemID.GoldCoin].Value);
			_coinImage.Left.Set(_text.Left.Pixels + _text.Width.Pixels + 5f, 0f);
			_coinImage.Top.Set(_text.Top.Pixels, 0f);

			_parent.Append(_coinImage);

			Append(_parent);
		}

		private void OnButtonClick(UIMouseEvent evt, UIElement listeningElement)
		{
			if (Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown > 0 ||
			    !Main.LocalPlayer.CanAfford(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost) ||
			    !Main.LocalPlayer.dead)
				return;

			Main.LocalPlayer.BuyItem(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost);
			Main.LocalPlayer.respawnTimer = 1;
			Main.LocalPlayer.statMana = Main.LocalPlayer.statManaMax2;

			Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown = 18005;
			Main.LocalPlayer.GetModPlayer<BuybackPlayer>().JustBoughtBack = true;

			SoundStyle soundStyle = new("Buyback/buyback")
			{
				Type = SoundType.Sound,
				MaxInstances = 100,
				SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
				Volume = 0.6f,
				
			};
			SoundEngine.PlaySound(in soundStyle, Main.LocalPlayer.PotionOfReturnHomePosition);
			ChatHelper.SendChatMessageFromClient(new(JustBoughtBackMessage.Value));
		}

		public void Load(Mod mod)
		{
			Mod = mod;
			JustBoughtBackMessage = this.GetLocalization(nameof(JustBoughtBackMessage));
			BuybackCost = this.GetLocalization(nameof(BuybackCost));
			BuybackCooldown = this.GetLocalization(nameof(BuybackCooldown));
		}

		public void Unload()
		{
		}
	}
}
