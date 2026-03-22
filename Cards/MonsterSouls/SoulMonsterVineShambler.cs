using System.Threading.Tasks;
using BaseLib.Abstracts;
using ABStS2Mod.Cards.Powers;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterVineShambler() : CustomCardModel(1, CardType.Skill, CardRarity.Event, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SoulMonsterVineShamblerPower>(Owner.Creature, 1, Owner.Creature, this);
        foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards)
        {
            if (card.Type == CardType.Attack)
            {
                card.EnergyCost.AddThisTurn(-1);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
