using System;
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

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterWriggler() : CustomCardModel(1, CardType.Skill, CardRarity.Event, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<PoisonPower>(1m),
        new DynamicVar("BlockThreshold", 5m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<PoisonPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        int removedBlock = cardPlay.Target.Block;
        if (removedBlock > 0)
        {
            await CreatureCmd.LoseBlock(cardPlay.Target, removedBlock);
        }

        int stackAmount = removedBlock / DynamicVars["BlockThreshold"].IntValue;
        if (stackAmount > 0)
        {
            await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, stackAmount, Owner.Creature, this);
            await PowerCmd.Apply<PoisonPower>(cardPlay.Target, stackAmount, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockThreshold"].UpgradeValueBy(-1m);
    }
}
