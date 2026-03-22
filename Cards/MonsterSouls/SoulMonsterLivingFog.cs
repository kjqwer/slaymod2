using System.Threading.Tasks;
using ABStS2Mod.Cards.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterLivingFog() : CustomCardModel(2, CardType.Power, CardRarity.Event, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (IsUpgraded)
        {
            await PowerCmd.Apply<SoulMonsterLivingFogPlusPower>(Owner.Creature, 1, Owner.Creature, this);
            return;
        }

        await PowerCmd.Apply<SoulMonsterLivingFogPower>(Owner.Creature, 1, Owner.Creature, this);
    }
}
