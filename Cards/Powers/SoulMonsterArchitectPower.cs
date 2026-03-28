using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterArchitectPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || CombatState == null)
        {
            return;
        }

        List<CardModel> monsterSoulCards = ModelDb.AllCards
            .Where(c => c.Id.Entry.StartsWith("ABSTS2MOD-SOUL_MONSTER_", StringComparison.Ordinal))
            .Where(c => c.Rarity == CardRarity.Event)
            .ToList();
        if (monsterSoulCards.Count == 0)
        {
            return;
        }

        Flash();
        for (int i = 0; i < Amount; i++)
        {
            CardModel? canonicalCard = player.RunState.Rng.CombatCardGeneration.NextItem(monsterSoulCards);
            if (canonicalCard == null)
            {
                continue;
            }

            CardModel cardModel = CombatState.CreateCard(canonicalCard, player);
            CardCmd.Upgrade(cardModel);
            CardCmd.ApplyKeyword(cardModel, CardKeyword.Ethereal);
            cardModel.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, addedByPlayer: true);
        }
    }
}
