using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterQueenSoulBindPower : CustomPowerModel
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

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        StunIntent.GetStaticHoverTip()
    };

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        data.TurnStarts++;
        if (data.TurnStarts % 3 == 0)
        {
            if (CombatState == null)
            {
                return;
            }

            Flash();
            foreach (Creature enemy in CombatState.HittableEnemies)
            {
                await CreatureCmd.Stun(enemy);
            }
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(Owner, 40m, ValueProp.Unpowered, null);
    }
}
