using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterSpectralKnightTruthPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return Task.CompletedTask;
        }

        List<CardModel> handCards = PileType.Hand.GetPile(player).Cards.ToList();
        bool flashed = false;
        foreach (CardModel card in handCards.Where(IsEtherealCard))
        {
            if (!flashed)
            {
                Flash();
                flashed = true;
            }

            RemoveEthereal(card);
            card.SetToFreeThisTurn();
        }

        return Task.CompletedTask;
    }

    private static bool IsEtherealCard(CardModel card)
    {
        IEnumerable<CardKeyword> keywords = card.CanonicalKeywords ?? Enumerable.Empty<CardKeyword>();
        return keywords.Contains(CardKeyword.Ethereal);
    }

    private static void RemoveEthereal(CardModel card)
    {
        MethodInfo? removeKeywordOnCard = card.GetType().GetMethod("RemoveKeyword", new[] { typeof(CardKeyword) });
        if (removeKeywordOnCard != null)
        {
            removeKeywordOnCard.Invoke(card, new object[] { CardKeyword.Ethereal });
            return;
        }

        MethodInfo? removeKeywordOnCmd = typeof(CardCmd).GetMethod("RemoveKeyword", BindingFlags.Public | BindingFlags.Static, new[] { typeof(CardModel), typeof(CardKeyword) });
        if (removeKeywordOnCmd != null)
        {
            removeKeywordOnCmd.Invoke(null, new object[] { card, CardKeyword.Ethereal });
        }
    }
}
