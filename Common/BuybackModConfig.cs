using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using C = Terraria.ModLoader.Config;
using Terraria.ModLoader.Config;

namespace Buyback.Common;

public class BuybackModConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ServerSide;
	[Header("Balance")]
	[DefaultValue(10)]
	[C.Range(1, 100)]
	[Slider]
	public int NetWorthDivider;
}