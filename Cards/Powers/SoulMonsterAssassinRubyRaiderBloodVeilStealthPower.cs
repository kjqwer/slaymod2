using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterAssassinRubyRaiderBloodVeilStealthPower : CustomPowerModel
{
    private class Data
    {
        public decimal HealAmount = 3m;
    }

    private bool _consumed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public void SetHealAmount(decimal healAmount)
    {
        GetInternalData<Data>().HealAmount = healAmount;
    }

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
        await CreatureCmd.Heal(Owner, GetInternalData<Data>().HealAmount);
        await PowerCmd.Decrement(this);
    }

    private static bool IsPoweredAttack(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }
}
