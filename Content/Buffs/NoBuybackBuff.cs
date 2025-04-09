using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.ModLoader;

namespace Buyback.Content.Buffs
{
    public class NoBuybackBuff : ModBuff
    {
        private long _buybackCost = Item.buyPrice(silver: 200);
        private int _buybackCooldown;
        public string Reason 
        { 
            get
            {
                if (_buybackCooldown > 0)
                {
                    var time = TimeSpan.FromSeconds(_buybackCooldown / 60d);
                    return $"{time.Minutes:0}:{time.Seconds:00}";
                }
                return $"{_buybackCost / 10000} Gold";
            }
        }
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            _buybackCost = player.GetModPlayer<BuybackPlayer>().BuybackCost;
            _buybackCooldown = player.GetModPlayer<BuybackPlayer>().BuybackCooldown;
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
