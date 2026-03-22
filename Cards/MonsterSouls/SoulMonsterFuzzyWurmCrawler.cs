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
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterFuzzyWurmCrawler() : CustomCardModel(1, CardType.Skill, CardRarity.Event, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(10m, ValueProp.Move),
        new PowerVar<SoulMonsterFuzzyWurmCrawlerPower>(1m),
        new PowerVar<SoulMonsterFuzzyWurmCrawlerPlusPower>(1m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<SoulMonsterBurrowRaid>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (IsUpgraded)
        {
            await PowerCmd.Apply<SoulMonsterFuzzyWurmCrawlerPlusPower>(Owner.Creature, DynamicVars["SoulMonsterFuzzyWurmCrawlerPlusPower"].BaseValue, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<SoulMonsterFuzzyWurmCrawlerPower>(Owner.Creature, DynamicVars["SoulMonsterFuzzyWurmCrawlerPower"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4m);
    }
}
