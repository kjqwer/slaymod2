using System.Collections.Generic;
using System.Threading.Tasks;
using ABStS2Mod.Cards.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards.MonsterSouls;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulMonsterKnowledgeDemon() : CustomCardModel(2, CardType.Skill, CardRarity.Event, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<SoulMonsterKnowledgeBlessingPower>(1m),
        new DamageVar(30m, ValueProp.Move)
    };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromPower<SoulMonsterKnowledgeBlessingPower>()
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal blessingAmount = Owner.Creature.GetPower<SoulMonsterKnowledgeBlessingPower>()?.Amount ?? 0m;
        if (blessingAmount >= 3m)
        {
            if (CombatState == null)
            {
                return;
            }

            foreach (Creature enemy in CombatState.HittableEnemies)
            {
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                    .FromCard(this)
                    .Targeting(enemy)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(choiceContext);
            }
            return;
        }

        await PowerCmd.Apply<SoulMonsterKnowledgeBlessingPower>(Owner.Creature, DynamicVars["SoulMonsterKnowledgeBlessingPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
