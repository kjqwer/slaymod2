using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterFrogKnightPower : CustomPowerModel
{
    private const decimal BaseBlockPerStack = 15m;
    private const decimal LowHpBlockPerStack = 20m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        decimal blockPerStack = BaseBlockPerStack;
        if (Owner.MaxHp > 0 && Owner.CurrentHp * 2m < Owner.MaxHp)
        {
            blockPerStack = LowHpBlockPerStack;
        }

        Flash();
        await CreatureCmd.GainBlock(Owner, Amount * blockPerStack, ValueProp.Unpowered, null);
    }
}
