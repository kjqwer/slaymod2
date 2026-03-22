using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterGasBomb() : CustomCardModel(1, CardType.Skill, CardRarity.Event, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Turns", 1m),
        new DynamicVar("BombDamage", 15m)
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        TheBombPower? bombPower = await PowerCmd.Apply<TheBombPower>(Owner.Creature, DynamicVars["Turns"].BaseValue, Owner.Creature, this);
        bombPower?.SetDamage(DynamicVars["BombDamage"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BombDamage"].UpgradeValueBy(5m);
    }
}
