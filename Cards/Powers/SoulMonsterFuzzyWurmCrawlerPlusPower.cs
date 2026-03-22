using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using ABStS2Mod.Cards.MonsterSouls;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterFuzzyWurmCrawlerPlusPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<SoulMonsterBurrowRaid>()
    };

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        if (player != Owner.Player || AmountOnTurnStart == 0)
        {
            return;
        }

        Flash();
        for (int i = 0; i < Amount; i++)
        {
            var card = combatState.CreateCard<SoulMonsterBurrowRaid>(Owner.Player);
            CardCmd.Upgrade(card, CardPreviewStyle.None);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
        }
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Side && AmountOnTurnStart != 0)
        {
            await PowerCmd.Remove(this);
        }
    }
}
