using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterAssassinRubyRaiderAmbushPower : CustomPowerModel
{
    private class Data
    {
        public AttackCommand? CommandToModify;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (command.Attacker != Owner)
        {
            return Task.CompletedTask;
        }

        if (!IsPoweredAttack(command.DamageProps))
        {
            return Task.CompletedTask;
        }

        if (command.ModelSource is CardModel card && card.Type != CardType.Attack)
        {
            return Task.CompletedTask;
        }

        Data data = GetInternalData<Data>();
        data.CommandToModify ??= command;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == null)
        {
            return 1m;
        }

        if (dealer != Owner && !Owner.Pets.Contains(dealer))
        {
            return 1m;
        }

        if (!IsPoweredAttack(props) || cardSource == null || cardSource.Type != CardType.Attack)
        {
            return 1m;
        }

        Data data = GetInternalData<Data>();
        if (data.CommandToModify != null && cardSource != data.CommandToModify.ModelSource)
        {
            return 1m;
        }

        return Amount;
    }

    public override async Task AfterAttack(AttackCommand command)
    {
        Data data = GetInternalData<Data>();
        if (command != data.CommandToModify)
        {
            return;
        }

        data.CommandToModify = null;
        Flash();
        await PowerCmd.Remove(this);
    }

    private static bool IsPoweredAttack(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }
}
