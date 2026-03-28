using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterBowlbugSilkToxinWeaverPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<WeakPower>()
    };

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount <= 0m || power is not PoisonPower || power.Owner != Owner)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<WeakPower>(Owner, Amount, applier ?? Owner, cardSource);
    }
}
