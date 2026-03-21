using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABStS2Mod.Cards.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterFlyconid() : CustomCardModel(1, CardType.Power, CardRarity.Event, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<VulnerablePower>(1m),
        new PowerVar<SoulMonsterFlyconidPower>(2m),
        new BlockVar(10m, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await PowerCmd.Apply<VulnerablePower>(cardPlay.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<SoulMonsterFlyconidPower>(Owner.Creature, DynamicVars["SoulMonsterFlyconidPower"].BaseValue, Owner.Creature, this);

        int attacksPlayedThisTurn = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.CardPlay.Card.Owner == Owner && e.CardPlay.Card.Type == CardType.Attack && e.HappenedThisTurn(CombatState));
        if (attacksPlayedThisTurn == 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SoulMonsterFlyconidPower"].UpgradeValueBy(1m);
    }
}
