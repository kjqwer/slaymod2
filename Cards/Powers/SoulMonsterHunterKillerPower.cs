using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterHunterKillerPower : CustomPowerModel
{
    private class Data
    {
        public decimal GainedThisTurn;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>()
    };

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, cardPlay.Card);
        await PowerCmd.Apply<DexterityPower>(Owner, Amount, Owner, cardPlay.Card);
        GetInternalData<Data>().GainedThisTurn += Amount;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext context, CombatSide side)
    {
        if (side != Owner.Side)
        {
            return;
        }

        decimal gained = GetInternalData<Data>().GainedThisTurn;
        if (gained <= 0m)
        {
            return;
        }

        await PowerCmd.Apply<StrengthPower>(Owner, -gained, Owner, null);
        await PowerCmd.Apply<DexterityPower>(Owner, -gained, Owner, null);
        GetInternalData<Data>().GainedThisTurn = 0m;
    }
}
