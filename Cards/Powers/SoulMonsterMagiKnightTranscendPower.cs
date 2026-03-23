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

public sealed class SoulMonsterMagiKnightTranscendPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return Task.CompletedTask;
        }

        var upgradableCards = PileType.Hand.GetPile(player).Cards
            .Where(card => card.IsUpgradable && !card.IsUpgraded)
            .ToList();
        if (upgradableCards.Count == 0)
        {
            return Task.CompletedTask;
        }

        Flash();
        foreach (CardModel card in upgradableCards)
        {
            CardCmd.Upgrade(card);
        }

        return Task.CompletedTask;
    }
}
