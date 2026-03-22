using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterFogmogPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>()
    };

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Skill)
        {
            return;
        }

        Flash();
        foreach (Creature enemy in CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(enemy, Amount, Owner, cardPlay.Card);
            await PowerCmd.Apply<WeakPower>(enemy, Amount, Owner, cardPlay.Card);
        }
    }
}
