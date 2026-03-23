using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABStS2Mod.Cards.Powers;
using BaseLib.Abstracts;
using BaseLib.Cards;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterWaterfallGiant() : CustomCardModel(2, CardType.Skill, CardRarity.Event, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<SoulMonsterWaterfallPower>(10m)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<SoulMonsterWaterfallPower>(),
        HoverTipFactory.FromPower<SoulMonsterWaterfallCountdownPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SoulMonsterWaterfallPower? waterfall = await PowerCmd.Apply<SoulMonsterWaterfallPower>(Owner.Creature, DynamicVars["SoulMonsterWaterfallPower"].BaseValue, Owner.Creature, this);
        decimal waterfallPoints = waterfall?.Amount ?? DynamicVars["SoulMonsterWaterfallPower"].BaseValue;
        decimal damage = SoulMonsterWaterfallCountdownPower.CalculateDamage(waterfallPoints);
        SoulMonsterWaterfallCountdownPower? existingCountdown = Owner.Creature.Powers.OfType<SoulMonsterWaterfallCountdownPower>().FirstOrDefault();
        if (existingCountdown != null)
        {
            await PowerCmd.Remove(existingCountdown);
        }
        await PowerCmd.Apply<SoulMonsterWaterfallCountdownPower>(Owner.Creature, damage, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SoulMonsterWaterfallPower"].UpgradeValueBy(5m);
    }
}
