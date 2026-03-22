using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterMawler() : CustomCardModel(3, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(32m),
        new ExtraDamageVar(5m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
            Math.Min(
                CombatManager.Instance.History.Entries
                    .OfType<DamageReceivedEntry>()
                    .Count(entry => entry.Receiver == card.Owner.Creature && entry.Result.UnblockedDamage > 0),
                10)),
        new DynamicVar("MaxBonus", 50m),
        new MaxHpVar(2m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.Static(StaticHoverTip.Fatal)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel power) => power.ShouldOwnerDeathTriggerFatal());
        var attack = await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);

        if (shouldTriggerFatal && attack.Results.Any(result => result.WasTargetKilled))
        {
            await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.MaxHp.IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(10m);
    }
}
