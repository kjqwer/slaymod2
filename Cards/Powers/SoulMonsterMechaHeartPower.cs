using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterMechaHeartPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<ArtifactPower>(),
        HoverTipFactory.ForEnergy(this)
    };

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<ArtifactPower>(Owner, Amount * 10m, Owner, null);

        List<CardModel> cardsToExhaust = PileType.Hand.GetPile(player)
            .Cards
            .Where(IsStatusOrCurse)
            .ToList();
        foreach (CardModel card in cardsToExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);
            await CardPileCmd.Draw(choiceContext, 1m, player);
            await PlayerCmd.GainEnergy(1m, player);
        }
    }

    private static bool IsStatusOrCurse(CardModel card)
    {
        return card.Type is CardType.Status || card.Type is not (CardType.Attack or CardType.Skill or CardType.Power);
    }
}
