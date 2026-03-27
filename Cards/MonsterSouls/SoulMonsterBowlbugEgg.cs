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
public sealed class SoulMonsterBowlbugEgg() : CustomCardModel(1, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(7m, ValueProp.Move),
        new BlockVar(7m, ValueProp.Move),
        new DynamicVar("Energy", 1m)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        bool hadBlock = Owner.Creature.Block > 0;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (hadBlock)
        {
            await PlayerCmd.GainEnergy(DynamicVars["Energy"].BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Energy"].UpgradeValueBy(1m);
    }
}
