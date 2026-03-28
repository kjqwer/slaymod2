using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterAxeRubyRaiderMarkPower : CustomPowerModel
{
    private const decimal TriggerThresholdRatio = 0.5m;
    private const decimal DamagePerStack = 20m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side || Owner.CurrentHp >= Owner.MaxHp * TriggerThresholdRatio)
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(choiceContext, Owner, Amount * DamagePerStack, ValueProp.Unpowered | ValueProp.Unblockable, (Creature)Owner);
        await PowerCmd.Remove(this);
    }
}
