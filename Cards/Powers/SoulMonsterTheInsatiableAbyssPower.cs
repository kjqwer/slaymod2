using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterTheInsatiableAbyssPower : CustomPowerModel
{
    private const decimal KillThreshold = 6m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || Owner.IsDead || Amount < KillThreshold)
        {
            return;
        }

        Flash();
        await DoomPower.DoomKill(new Creature[] { Owner });
    }
}
