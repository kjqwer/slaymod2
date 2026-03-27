using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterTunneler() : CustomCardModel(2, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(18m, ValueProp.Move),
        new DynamicVar("BlockDamage", 36m),
        new DynamicVar("Cards", 1m)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        bool hadBlock = Owner.Creature.Block > 0;
        decimal damage = hadBlock ? DynamicVars["BlockDamage"].BaseValue : DynamicVars.Damage.BaseValue;
        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        if (hadBlock)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars["Cards"].BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["BlockDamage"].UpgradeValueBy(6m);
    }
}
