using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterShrinkerBeetleShrinkPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType
    {
        get
        {
            if (Amount < 0)
            {
                return PowerStackType.Single;
            }

            return PowerStackType.Counter;
        }
    }

    public override bool AllowNegative => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("DamageDecrease", 30m)
    };

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        NCombatRoom.Instance?.GetCreatureNode(Owner)?.ScaleTo(0.5f, 0.75f);
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        NCombatRoom.Instance?.GetCreatureNode(oldOwner)?.ScaleTo(1f, 0.75f);
        return Task.CompletedTask;
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Amount >= 0 && side == Owner.Side)
        {
            await PowerCmd.Decrement(this);
        }
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!wasRemovalPrevented && creature == Applier)
        {
            await PowerCmd.Remove(this);
        }
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner || !props.HasFlag(ValueProp.Move) || props.HasFlag(ValueProp.Unpowered))
        {
            return 1m;
        }

        return 0.7m;
    }
}
