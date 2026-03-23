using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterWaterfallPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}
