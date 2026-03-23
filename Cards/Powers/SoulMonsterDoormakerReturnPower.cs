using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterDoormakerReturnPower : CustomPowerModel
{
    private class Data
    {
        public int TurnStarts;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.TurnStarts++;
        if (data.TurnStarts == 1)
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, 40m, ValueProp.Unpowered, null);
            return;
        }

        if (data.TurnStarts >= 2)
        {
            Flash();
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<SoulMonsterDoormakerImmortalityPower>(Owner, 1m, Owner, null);
        }
    }
}
