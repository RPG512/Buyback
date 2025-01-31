using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

#nullable disable
namespace Buyback
{
	[Autoload(Side = ModSide.Client)]
	public class FightSystem : ModSystem
	{
		internal FightState FightState;
		private UserInterface _fightState;

		public override void Load()
		{
			FightState = new FightState{Mod = Mod};
			FightState.Activate();
			_fightState = new UserInterface();
			_fightState.SetState(FightState);
		}

		public override void UpdateUI(GameTime gameTime) => _fightState?.Update(gameTime);

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int index = layers.FindIndex(
				(layer => layer.Name.Equals("Vanilla: Mouse Text")));
			if (!Main.LocalPlayer.dead || index == -1)
				return;
			layers.Insert(index,
				new LegacyGameInterfaceLayer("DOTA2Mod: BuybackButton",
					() =>
					{
						_fightState.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI));
		}
	}
}