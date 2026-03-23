using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterGlobeHeadCurrentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner || card.Type != CardType.Power)
        {
            return false;
        }

        bool isInHandOrPlay = card.Pile?.Type == PileType.Hand || card.Pile?.Type == PileType.Play;
        if (!isInHandOrPlay)
        {
            return false;
        }

        modifiedCost = decimal.Max(0m, originalCost - Amount);
        return modifiedCost != originalCost;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player || cardPlay.Card.Type != CardType.Power || CombatState == null)
        {
            return;
        }

        Flash();
        foreach (Creature enemy in CombatState.HittableEnemies)
        {
            await DamageCmd.Attack(12m)
                .FromCard(cardPlay.Card)
                .Targeting(enemy)
                .WithHitFx("vfx/vfx_attack_lightning")
                .Execute(context);
        }
    }
}
