using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABStS2Mod.Cards;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace ABStS2Mod.Relics;

[Pool(typeof(RegentRelicPool))]
public sealed class SoulCaptureLantern : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != Owner.Creature.Side || combatState.RoundNumber > 1)
        {
            return;
        }

        Flash();
        CardModel card = combatState.CreateCard<SoulCapture>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, addedByPlayer: true);
    }
}

[Pool(typeof(RegentRelicPool))]
public sealed class InnateSoulSeal : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != Owner.Creature.Side || combatState.RoundNumber > 1)
        {
            return;
        }

        List<CardModel> monsterSoulCards = ModelDb.AllCards
            .Where(c => c.Id.Entry.StartsWith("ABSTS2MOD-SOUL_MONSTER_", StringComparison.Ordinal))
            .Where(c => c.Rarity == CardRarity.Event)
            .ToList();
        CardModel? canonicalCard = Owner.RunState.Rng.CombatCardGeneration.NextItem(monsterSoulCards);
        if (canonicalCard == null)
        {
            return;
        }

        CardModel card = combatState.CreateCard(canonicalCard, Owner);
        if (!card.EnergyCost.CostsX)
        {
            card.EnergyCost.SetThisCombat(0);
        }

        Flash();
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
    }
}
