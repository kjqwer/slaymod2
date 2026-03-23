using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterDoormakerImmortalityPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

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
        }
        else
        {
            await PowerCmd.Apply<SoulMonsterDoormakerImmortalityPower>(Owner, -1m, Owner, null);
        }
        await PowerCmd.Apply<SoulMonsterDoormakerReturnPower>(Owner, 1m, Owner, null);
    }
}
