using System.Linq;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterInfestedPrismPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card.Owner.Creature != Owner || card.Type != CardType.Attack)
        {
            return false;
        }

        bool isInValidPile = card.Pile?.Type == PileType.Hand || card.Pile?.Type == PileType.Play;
        if (!isInValidPile)
        {
            return false;
        }

        int attacksPlayedThisTurn = CombatManager.Instance.History.CardPlaysStarted.Count(
            (CardPlayStartedEntry entry) => entry.HappenedThisTurn(CombatState) &&
                                            entry.CardPlay.Card.Type == CardType.Attack &&
                                            entry.CardPlay.Card.Owner.Creature == Owner);

        int currentCardOffset = card.Pile?.Type == PileType.Play ? 1 : 0;
        int attackIndexThisTurn = attacksPlayedThisTurn - currentCardOffset + 1;
        if (attackIndexThisTurn > Amount)
        {
            return false;
        }

        modifiedCost = 0m;
        return true;
    }
}
