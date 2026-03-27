using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterPunchConstruct() : CustomCardModel(0, CardType.Skill, CardRarity.Event, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ArtifactPower>(3m),
        new BlockVar(5m, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<ArtifactPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<ArtifactPower>(Owner.Creature, DynamicVars["ArtifactPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ArtifactPower"].UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}
