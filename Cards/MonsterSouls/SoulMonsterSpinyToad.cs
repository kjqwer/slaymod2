using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterSpinyToad() : CustomCardModel(1, CardType.Skill, CardRarity.Event, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ThornsPower>(5m),
        new DynamicVar("Multiplier", 2m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<ThornsPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ThornsPower>(Owner.Creature, DynamicVars["ThornsPower"].BaseValue, Owner.Creature, this);
        ThornsPower? thorns = Owner.Creature.Powers.OfType<ThornsPower>().FirstOrDefault();
        if (thorns == null || thorns.Amount <= 10m)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        decimal burstDamage = thorns.Amount * DynamicVars["Multiplier"].BaseValue;
        await PowerCmd.Remove(thorns);
        await DamageCmd.Attack(burstDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(1m);
    }
}
