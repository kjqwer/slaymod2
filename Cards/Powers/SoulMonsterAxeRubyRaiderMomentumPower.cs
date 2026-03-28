using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterAxeRubyRaiderMomentumPower : CustomPowerModel
{
    private class Data
    {
        public decimal StoredEnergy;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card.Type != CardType.Attack)
        {
            return Task.CompletedTask;
        }

        Flash();
        GetInternalData<Data>().StoredEnergy += Amount;
        return Task.CompletedTask;
    }

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        if (player != Owner.Player || AmountOnTurnStart == 0)
        {
            return;
        }

        decimal energy = GetInternalData<Data>().StoredEnergy;
        if (energy <= 0m)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainEnergy(energy, player);
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Side && AmountOnTurnStart != 0)
        {
            await PowerCmd.Remove(this);
        }
    }
}
