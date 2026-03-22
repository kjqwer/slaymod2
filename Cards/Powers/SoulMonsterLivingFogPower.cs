using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterLivingFogPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
    {
        if (player != Owner.Player)
        {
            return;
        }

        List<CardModel> skillCards = Owner.Player.Character.CardPool
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Skill)
            .ToList();
        if (skillCards.Count == 0)
        {
            return;
        }

        Flash();
        for (int i = 0; i < Amount; i++)
        {
            CardModel generatedCard = CardFactory.GetDistinctForCombat(player, skillCards, 1, Owner.Player.RunState.Rng.CombatCardGeneration).First();
            await CardPileCmd.AddGeneratedCardToCombat(generatedCard, PileType.Hand, addedByPlayer: true);
        }
    }
}
