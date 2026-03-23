using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterBattleFriendV3Power : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return Task.CompletedTask;
        }

        var handCards = PileType.Hand.GetPile(Owner.Player).Cards.ToList();
        if (handCards.Count == 0)
        {
            return Task.CompletedTask;
        }

        CardModel? card = player.RunState.Rng.CombatCardGeneration.NextItem(handCards);
        if (card == null)
        {
            return Task.CompletedTask;
        }

        Flash();
        card.SetToFreeThisTurn();
        card.BaseReplayCount += Amount;
        return Task.CompletedTask;
    }
}
