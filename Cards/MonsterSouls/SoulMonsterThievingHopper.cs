using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterThievingHopper() : CustomCardModel(1, CardType.Skill, CardRarity.Event, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(Owner.Creature);
        ArgumentNullException.ThrowIfNull(Owner.Creature.CombatState);
        List<CardModel> monsterSoulCards = ModelDb.AllCards
            .Where(c => c.Id.Entry.StartsWith("ABSTS2MOD-SOUL_MONSTER_"))
            .Where(c => c.Rarity == CardRarity.Event)
            .Where(c => c.Id != Id)
            .ToList();
        CardModel? canonicalCard = Owner.RunState.Rng.CombatCardGeneration.NextItem(monsterSoulCards);
        if (canonicalCard == null)
        {
            return;
        }
        CardModel cardModel = Owner.Creature.CombatState.CreateCard(canonicalCard, Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(cardModel);
        }
        cardModel.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
    }
}
