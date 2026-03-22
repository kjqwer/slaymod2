using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterSludgeSpinner() : CustomCardModel(2, CardType.Skill, CardRarity.Event, TargetType.Self)
{
    private int _currentBlock = 14;

    [SavedProperty]
    public int CurrentBlock
    {
        get => _currentBlock;
        set
        {
            AssertMutable();
            _currentBlock = value;
            DynamicVars.Block.BaseValue = _currentBlock;
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Retain };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(CurrentBlock, ValueProp.Move),
        new IntVar("Poison", 2m),
        new IntVar("Increase", 4m)
    };

    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override async Task AfterCardRetained(CardModel card)
    {
        if (card != this || CombatState == null || Owner?.Creature == null)
        {
            return;
        }
        await PowerCmd.Apply<PoisonPower>(CombatState.HittableEnemies, DynamicVars["Poison"].BaseValue, Owner.Creature, this);
        CurrentBlock += DynamicVars["Increase"].IntValue;
    }

    protected override void OnUpgrade()
    {
        CurrentBlock += 4;
        DynamicVars["Poison"].UpgradeValueBy(1m);
        DynamicVars["Increase"].UpgradeValueBy(2m);
    }
}
