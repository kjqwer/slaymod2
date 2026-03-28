using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABStS2Mod.Cards.MonsterSouls;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulCapture() : CustomCardModel(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    private bool _isWaitingForSoulChoice;

    protected override IEnumerable<DynamicVar> CanonicalVars
        => new DynamicVar[] { new DamageVar(15m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> ExtraHoverTips
        => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Fatal) };

    public override bool ShouldStopCombatFromEnding()
        => _isWaitingForSoulChoice;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        Player? owner = Owner;
        if (owner == null)
        {
            return;
        }

        Creature target = cardPlay.Target;
        AttackCommand attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        bool wasTargetDefeated = target.IsDead
            || attack.Results.Any((DamageResult result) => result.WasTargetKilled)
            || CombatManager.Instance.IsEnding
            || !CombatManager.Instance.IsInProgress;
        if (!wasTargetDefeated)
        {
            await Task.Yield();
            wasTargetDefeated = target.IsDead
                || CombatManager.Instance.IsEnding
                || !CombatManager.Instance.IsInProgress;
        }
        if (wasTargetDefeated)
        {
            await RemoveDeckVersionAndAddRewardCard(choiceContext, owner, target.ModelId.Entry);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        AddKeyword(CardKeyword.Retain);
    }

    private async Task RemoveDeckVersionAndAddRewardCard(PlayerChoiceContext choiceContext, Player owner, string defeatedMonsterId)
    {
        CardModel? deckVersion = DeckVersion;
        if (deckVersion == null)
        {
            deckVersion = owner.Deck.Cards.FirstOrDefault((CardModel card) => card.Id == Id && card.CurrentUpgradeLevel == CurrentUpgradeLevel);
        }

        if (deckVersion != null)
        {
            await CardPileCmd.RemoveFromDeck(deckVersion, showPreview: true);
        }

        _isWaitingForSoulChoice = true;
        try
        {
            CardModel soulCaptureRewardCard = owner.RunState.CreateCard<SoulCapture>(owner);
            List<CardModel> rewardCards = MonsterSoulCardRegistry.CreateRewardCards(owner, defeatedMonsterId).ToList();
            foreach (CardModel monsterRewardCard in rewardCards)
            {
                ApplyUpgradeLevel(this, monsterRewardCard);
            }
            ApplyUpgradeLevel(this, soulCaptureRewardCard);
            rewardCards.Add(soulCaptureRewardCard);
            await SelectAndGrantRewardCard(choiceContext, owner, rewardCards);
        }
        finally
        {
            _isWaitingForSoulChoice = false;
        }

        if (Pile != null && Pile.IsCombatPile && !HasBeenRemovedFromState)
        {
            await CardCmd.Exhaust(choiceContext, this);
        }
    }

    private static void ApplyUpgradeLevel(CardModel source, CardModel target)
    {
        for (int i = 0; i < source.CurrentUpgradeLevel && target.IsUpgradable; i++)
        {
            target.UpgradeInternal();
            target.FinalizeUpgradeInternal();
        }
    }

    private static async Task SelectAndGrantRewardCard(PlayerChoiceContext choiceContext, Player owner, List<CardModel> rewardCards)
    {
        if (rewardCards.Count == 0)
        {
            return;
        }

        CardModel chosen = await ChooseRewardCardSynced(choiceContext, owner, rewardCards);
        CardPileAddResult addResult = await CardPileCmd.Add(chosen, PileType.Deck);
        if (addResult.success)
        {
            CardCmd.PreviewCardPileAdd(addResult, 2f);
        }
    }

    private static async Task<CardModel> ChooseRewardCardSynced(PlayerChoiceContext choiceContext, Player owner, List<CardModel> rewardCards)
    {
        CardModel? chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, rewardCards, owner, canSkip: false);
        return chosen ?? rewardCards[0];
    }
}
