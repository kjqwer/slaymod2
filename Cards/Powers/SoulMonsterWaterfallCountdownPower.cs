using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterWaterfallCountdownPower : CustomPowerModel
{
    private class Data
    {
        public int TurnsRemaining = 2;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.TurnsRemaining--;
        if (data.TurnsRemaining > 0 || CombatState == null)
        {
            return;
        }

        Flash();
        foreach (Creature enemy in CombatState.HittableEnemies)
        {
            await CreatureCmd.Damage(choiceContext, enemy, Amount, ValueProp.Unpowered, Owner, null);
        }
        await PowerCmd.Remove(this);
    }

    public static decimal CalculateDamage(decimal waterfallPoints)
    {
        return waterfallPoints + decimal.Floor(waterfallPoints * waterfallPoints / 20m);
    }
}
