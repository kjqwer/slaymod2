using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterCorpseSlug() : CustomCardModel(1, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy)
{
    private int _currentDamage = 10;

    private int _increasedDamage;

    [SavedProperty]
    public int CurrentDamage
    {
        get => _currentDamage;
        set
        {
            AssertMutable();
            _currentDamage = value;
            DynamicVars.Damage.BaseValue = _currentDamage;
        }
    }

    [SavedProperty]
    public int IncreasedDamage
    {
        get => _increasedDamage;
        set
        {
            AssertMutable();
            _increasedDamage = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(CurrentDamage, ValueProp.Move),
        new MaxHpVar(2m),
        new IntVar("Increase", 5m)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        bool shouldTriggerFatal = cardPlay.Target.Powers.All(p => p.ShouldOwnerDeathTriggerFatal());
        AttackCommand attackResult = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (shouldTriggerFatal && attackResult.Results.Any(r => r.WasTargetKilled))
        {
            await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.MaxHp.IntValue);
            int increase = DynamicVars["Increase"].IntValue;
            BuffFromFatal(increase);
            (DeckVersion as SoulMonsterCorpseSlug)?.BuffFromFatal(increase);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.MaxHp.UpgradeValueBy(1m);
        DynamicVars["Increase"].UpgradeValueBy(3m);
    }

    protected override void AfterDowngraded()
    {
        UpdateDamage();
    }

    private void BuffFromFatal(int extraDamage)
    {
        IncreasedDamage += extraDamage;
        UpdateDamage();
    }

    private void UpdateDamage()
    {
        CurrentDamage = 10 + IncreasedDamage;
    }
}
