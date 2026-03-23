using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterDecimillipedeSegmentRevivePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldDie(Creature creature)
    {
        if (creature != Owner)
        {
            return true;
        }
        return false;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        await CreatureCmd.Heal(creature, creature.MaxHp * 0.25m);
        if (Amount <= 1m)
        {
            await PowerCmd.Remove(this);
            return;
        }
        await PowerCmd.Apply<SoulMonsterDecimillipedeSegmentRevivePower>(Owner, -1m, Owner, null);
    }
}
