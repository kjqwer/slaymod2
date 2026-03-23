using System.Collections.Generic;
using System.Threading.Tasks;
using ABStS2Mod.Cards.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterTheInsatiable() : CustomCardModel(1, CardType.Skill, CardRarity.Event, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<SoulMonsterTheInsatiableAbyssPower>(1m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<SoulMonsterTheInsatiableAbyssPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null || Owner?.Creature == null)
        {
            return;
        }

        await PowerCmd.Apply<SoulMonsterTheInsatiableAbyssPower>(CombatState.HittableEnemies, DynamicVars["SoulMonsterTheInsatiableAbyssPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
