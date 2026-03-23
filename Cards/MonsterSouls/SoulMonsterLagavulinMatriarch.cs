using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Cards;
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
public sealed class SoulMonsterLagavulinMatriarch() : CustomCardModel(2, CardType.Power, CardRarity.Event, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<PlatingPower>(25m),
        new DynamicVar("Turns", 5m),
        new DynamicVar("BombDamage", 80m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.FromPower<TheBombPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PlatingPower>(Owner.Creature, DynamicVars["PlatingPower"].BaseValue, Owner.Creature, this);
        TheBombPower? bomb = await PowerCmd.Apply<TheBombPower>(Owner.Creature, DynamicVars["Turns"].BaseValue, Owner.Creature, this);
        bomb?.SetDamage(DynamicVars["BombDamage"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PlatingPower"].UpgradeValueBy(5m);
        DynamicVars["BombDamage"].UpgradeValueBy(20m);
    }
}
