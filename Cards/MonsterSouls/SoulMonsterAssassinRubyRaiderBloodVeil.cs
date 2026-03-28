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
public sealed class SoulMonsterAssassinRubyRaiderBloodVeil() : CustomCardModel(1, CardType.Power, CardRarity.Event, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<SoulMonsterAssassinRubyRaiderBloodVeilStealthPower>(2m),
        new HealVar(3m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<SoulMonsterAssassinRubyRaiderBloodVeilStealthPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SoulMonsterAssassinRubyRaiderBloodVeilStealthPower? power = await PowerCmd.Apply<SoulMonsterAssassinRubyRaiderBloodVeilStealthPower>(
            Owner.Creature,
            DynamicVars["SoulMonsterAssassinRubyRaiderBloodVeilStealthPower"].BaseValue,
            Owner.Creature,
            this);
        if (power == null)
        {
            return;
        }
        power.SetHealAmount(DynamicVars.Heal.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SoulMonsterAssassinRubyRaiderBloodVeilStealthPower"].UpgradeValueBy(1m);
        DynamicVars.Heal.UpgradeValueBy(2m);
    }
}
