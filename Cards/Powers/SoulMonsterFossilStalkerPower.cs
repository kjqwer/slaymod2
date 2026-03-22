using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterFossilStalkerPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>()
    };

    public override async Task AfterAttack(AttackCommand command)
    {
        if (command.Attacker != Owner || command.TargetSide == Owner.Side || !IsPoweredAttack(command.DamageProps))
        {
            return;
        }

        List<DamageResult> results = command.Results.ToList();
        List<DamageResult> petHits = results.Where(r => r.Receiver.IsPet).ToList();
        foreach (DamageResult petHit in petHits)
        {
            results.RemoveAll(r => r.Receiver == petHit.Receiver.PetOwner?.Creature);
        }

        int unblockedHitCount = results.Count(r => r.UnblockedDamage > 0);
        if (unblockedHitCount <= 0)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(Owner, Amount * unblockedHitCount, Owner, null);
    }

    private static bool IsPoweredAttack(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }
}
