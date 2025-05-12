using Microsoft.Xna.Framework;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.ModLoader;

namespace Buyback.Content.Buffs
{
    public class NoBuybackBuff : ModBuff
    {
        static string Reason => Main.LocalPlayer.GetModPlayer<BuybackPlayer>().Reason + (Main.LocalPlayer.GetModPlayer<BuybackPlayer>().BuybackCooldown > 0 ? "" : " " + new Item(ItemID.GoldCoin).Name);
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            rare = ItemRarityID.Red;
            tip = Description.Format(Reason);
        }

        public override bool RightClick(int buffIndex)
        {
            ChatHelper.SendChatMessageFromClient(new(Description.Format(Reason)));
            return false;
        }
    }
}
