using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterTestSubjectSiphonPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<IntangiblePower>()
    };

    public override bool ShouldDie(Creature creature)
    {
        return creature != Owner;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        Flash();
        await CreatureCmd.Heal(creature, creature.MaxHp);
        if (Amount <= 1m)
        {
            await PowerCmd.Remove(this);
            return;
        }

        decimal nextAmount = Amount - 1m;
        await PowerCmd.Apply<SoulMonsterTestSubjectSiphonPower>(Owner, -1m, Owner, null);
        if (nextAmount == 1m)
        {
            await PowerCmd.Apply<IntangiblePower>(Owner, 3m, Owner, null);
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Skill)
        {
            return;
        }

        if (Amount != 3m && Amount != 1m)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(Owner, 1m, Owner, cardPlay.Card);
    }

    public override async Task AfterAttack(AttackCommand command)
    {
        if (command.Attacker != Owner || command.TargetSide == Owner.Side || Amount > 2m || !IsPoweredAttack(command.DamageProps))
        {
            return;
        }

        List<DamageResult> results = command.Results.ToList();
        List<DamageResult> petHits = results.Where(r => r.Receiver.IsPet).ToList();
        foreach (DamageResult petHit in petHits)
        {
            results.RemoveAll(r => r.Receiver == petHit.Receiver.PetOwner?.Creature);
        }

        List<DamageResult> unblockedHits = results.Where(r => r.UnblockedDamage > 0).ToList();
        if (unblockedHits.Count == 0)
        {
            return;
        }

        Flash();
        foreach (DamageResult hit in unblockedHits)
        {
            await PowerCmd.Apply<StrengthPower>(hit.Receiver, -1m, Owner, null);
            await PowerCmd.Apply<VulnerablePower>(hit.Receiver, 1m, Owner, null);
        }
    }

    private static bool IsPoweredAttack(ValueProp props)
    {
        return props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered);
    }
}
