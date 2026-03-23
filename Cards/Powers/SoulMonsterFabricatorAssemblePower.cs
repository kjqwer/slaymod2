using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using ABStS2Mod.Cards.MonsterSouls;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Cards.Powers;

public sealed class SoulMonsterFabricatorAssemblePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
    {
        HoverTipFactory.FromCard<SoulMonsterGuardbot>(),
        HoverTipFactory.FromCard<SoulMonsterNoisebot>(),
        HoverTipFactory.FromCard<SoulMonsterStabbot>(),
        HoverTipFactory.FromCard<SoulMonsterZapbot>()
    };

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || CombatState == null)
        {
            return;
        }

        Flash();
        await OstyCmd.Summon(choiceContext, Owner.Player, 5m, this);
        List<CardModel> candidates = new List<CardModel>
        {
            CombatState.CreateCard<SoulMonsterGuardbot>(Owner.Player),
            CombatState.CreateCard<SoulMonsterNoisebot>(Owner.Player),
            CombatState.CreateCard<SoulMonsterStabbot>(Owner.Player),
            CombatState.CreateCard<SoulMonsterZapbot>(Owner.Player)
        };
        int addCount = System.Math.Min(2, candidates.Count);
        for (int i = 0; i < addCount; i++)
        {
            CardModel? card = Owner.Player.RunState.Rng.CombatCardGeneration.NextItem(candidates);
            if (card == null)
            {
                continue;
            }

            candidates.Remove(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, addedByPlayer: true);
        }
    }
}
