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
using MegaCrit.Sts2.Core.Models.Powers;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterInfestedPrism() : CustomCardModel(1, CardType.Power, CardRarity.Event, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<LethalityPower>(50m),
        new PowerVar<SoulMonsterInfestedPrismPower>(1m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<LethalityPower>(),
        HoverTipFactory.FromPower<SoulMonsterInfestedPrismPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<LethalityPower>(Owner.Creature, DynamicVars["LethalityPower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<SoulMonsterInfestedPrismPower>(Owner.Creature, DynamicVars["SoulMonsterInfestedPrismPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LethalityPower"].UpgradeValueBy(25m);
    }
}
