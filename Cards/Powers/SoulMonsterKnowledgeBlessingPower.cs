using System.Collections.Generic;
using System.Threading.Tasks;
using ABStS2Mod.Cards.MonsterSouls;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterKnowledgeBlessingPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.ForEnergy(this)
    };

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner.Player || Amount < 1m)
        {
            return count;
        }

        return count + 1m;
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner.Player || Amount < 2m)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainEnergy(1m, player);
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (Amount < 3m || card.Owner.Creature != Owner)
        {
            return false;
        }

        bool isInHandOrPlay = card.Pile?.Type == PileType.Hand || card.Pile?.Type == PileType.Play;
        if (!isInHandOrPlay)
        {
            return false;
        }

        if (card is SoulMonsterKnowledgeDemon)
        {
            modifiedCost = 0m;
            return true;
        }

        modifiedCost = decimal.Max(0m, originalCost - 1m);
        return modifiedCost != originalCost;
    }
}
