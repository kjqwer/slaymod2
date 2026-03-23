using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterOvicopterPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.SummonStatic)
    };

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side || Owner.Player == null)
        {
            return;
        }

        Flash();
        await OstyCmd.Summon(choiceContext, Owner.Player, Amount, this);

        Creature? osty = Owner.Player.Osty;
        if (osty == null || !osty.IsAlive || osty.CurrentHp <= 0)
        {
            return;
        }

        foreach (Creature enemy in combatState.HittableEnemies)
        {
            await CreatureCmd.Damage(choiceContext, enemy, osty.CurrentHp, ValueProp.Move, osty, null);
        }
    }
}
