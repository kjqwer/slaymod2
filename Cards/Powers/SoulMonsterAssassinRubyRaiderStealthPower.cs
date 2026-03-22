using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterAssassinRubyRaiderStealthPower : CustomPowerModel
{
    private bool _consumed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || dealer.Side == Owner.Side)
        {
            return amount;
        }

        if (!IsPoweredAttack(props))
        {
            return amount;
        }

        _consumed = true;
        return 0m;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        if (!_consumed)
        {
            return;
        }

        _consumed = false;
        Flash();
        await PowerCmd.Decrement(this);
    }

    private static bool IsPoweredAttack(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }
}
