using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterToughEgg() : CustomCardModel(1, CardType.Skill, CardRarity.Event, TargetType.Self)
{
    protected override bool ShouldGlowRedInternal => Owner.IsOstyMissing;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(Owner.Creature);
        if (!Osty.CheckMissingWithAnim(Owner))
        {
            Creature? osty = Owner.Osty;
            if (osty == null)
            {
                return;
            }

            await CreatureCmd.GainBlock(Owner.Creature, osty.CurrentHp, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
