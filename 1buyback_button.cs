using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

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
			_parent.Left.Set(Main.screenWidth / 2f + 55f, 0.0f);
			_parent.Top.Set(Main.screenHeight / 2f + 155f, 0.0f);

			if (Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown > 0)
			{
				var time = TimeSpan.FromSeconds(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown / 60d);
				_text.SetText(BuybackCooldown.Format($"{time.Minutes:0}:{time.Seconds:00}"));
				_text.TextColor = Color.Red;
				_coinImage.SetImage(TextureAssets.Item[ItemID.None]);
			}
			else if(!Main.LocalPlayer.CanAfford(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost))
			{
				_text.SetText(BuybackCooldown.Format(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost / 10000));
				_text.TextColor = Color.Red;
				_coinImage.Left.Set(205, 0f);
				_coinImage.SetImage(TextureAssets.Item[ItemID.GoldCoin]);
			}
			else
			{
				_text.SetText(BuybackCost.Format(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost / 10000));
				_text.TextColor = Color.Gold;
				_coinImage.SetImage(TextureAssets.Item[ItemID.GoldCoin]);
			}
			_parent.Recalculate();
		}

		public override void OnInitialize()
		{
			FightElement = new FightElement();
			Append(FightElement);

			_parent.Height.Set(55f, 0f);
			_parent.Width.Set(240f, 0f);
			_parent.Left.Set(Main.screenWidth / 2f + 55f, 0f);
			_parent.Top.Set(Main.screenHeight / 2f + 155f, 0f);
			_parent.BackgroundColor = new Color(41, 49, 51, 200);
			_parent.BorderColor = new Color(0, 0, 0, 0);

			_parent.OnLeftClick += OnButtonClick;

			_text = new UIText("Buyback: ");
			_text.Width.Set(240f, 0f);
			_text.Height.Set(55f, 0f);
			_text.Top.Set(0.0f, 0.2f);
			_text.TextColor = Color.Gold;

			_parent.Append(_text);

			_coinImage = new(ModContent.Request<Texture2D>("Terraria/Images/Item_73")); // Images\\Item_73 - GoldCoin
			_coinImage.Height.Set(55f, 0f);
			_coinImage.Left.Set(180, 0f);
			_coinImage.Top.Set(0, 0.2f);
			
			_parent.Append(_coinImage);

			Append(_parent);
		}

		private void OnButtonClick(UIMouseEvent evt, UIElement listeningElement)
		{
			if (Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown > 0 ||
			    !Main.LocalPlayer.CanAfford(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost) ||
			    !Main.LocalPlayer.dead)
			{
				//Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown = 1;// Скип для тестов
				return;
			}

			Main.LocalPlayer.BuyItem(Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCost);
			Main.LocalPlayer.respawnTimer = 1;
			Main.LocalPlayer.statMana = Main.LocalPlayer.statManaMax2;

			Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown = 18005;
			Main.LocalPlayer.GetModPlayer<BuybackPlayer>().JustBoughtBack = true;

			if(Main.netMode == NetmodeID.SinglePlayer)
			{
				SoundStyle soundStyle = new("Buyback/buyback")
				{
					Type = SoundType.Sound,
					MaxInstances = 100,
					SoundLimitBehavior = SoundLimitBehavior.IgnoreNew,
					Volume = 1.4f
				};
				SoundEngine.PlaySound(in soundStyle, Main.LocalPlayer.GetModPlayer<BuybackPlayer>().RespawnPosition);
				Main.NewText(JustBoughtBackMessage.ToNetworkText(Main.LocalPlayer.name), Color.Gold);
			}
			else
			{
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)0);
				packet.Send();
				foreach (var p in Main.player)
					if (p.active)
						p.GetModPlayer<BuybackPlayer>().BuybackPlaySound = true;
				packet = Mod.GetPacket();
				packet.Write((byte)1);
				packet.Send();
			}
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
