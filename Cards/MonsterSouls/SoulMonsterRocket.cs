using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABStS2Mod.Cards.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterRocket() : CustomCardModel(2, CardType.Skill, CardRarity.Event, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => new CardKeyword[] { CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<SoulMonsterRocketPower>(1m),
        new IntVar("Replay", 1m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<SoulMonsterRocketPower>(),
        HoverTipFactory.Static(StaticHoverTip.ReplayStatic)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int drawCount = 10 - PileType.Hand.GetPile(Owner).Cards.Count;
        if (drawCount > 0)
        {
            await CardPileCmd.Draw(choiceContext, drawCount, Owner);
        }

        await PowerCmd.Apply<SoulMonsterRocketPower>(Owner.Creature, DynamicVars["SoulMonsterRocketPower"].BaseValue, Owner.Creature, this);

        foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards.Where(card => card.Type == CardType.Skill))
        {
            card.BaseReplayCount += DynamicVars["Replay"].IntValue;
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
