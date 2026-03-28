using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABStS2Mod.Cards.MonsterSouls;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
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
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.ValueProps;

namespace ABStS2Mod.Cards;

[Pool(typeof(ColorlessCardPool))]
public sealed class SoulCapture() : CustomCardModel(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars
        => new DynamicVar[] { new DamageVar(15m, ValueProp.Move) };

    protected override IEnumerable<IHoverTip> ExtraHoverTips
        => new IHoverTip[] { HoverTipFactory.Static(StaticHoverTip.Fatal) };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

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
        if (wasTargetDefeated)
        {
            await RemoveDeckVersionAndAddRewardCard(choiceContext, target.ModelId.Entry);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        AddKeyword(CardKeyword.Retain);
    }

    private async Task RemoveDeckVersionAndAddRewardCard(PlayerChoiceContext choiceContext, string defeatedMonsterId)
    {
        Player? owner = Owner;
        if (owner == null)
        {
            return;
        }
        CardModel? deckVersion = DeckVersion;
        if (deckVersion == null)
        {
            deckVersion = owner.Deck.Cards.FirstOrDefault((CardModel card) => card.Id == Id && card.CurrentUpgradeLevel == CurrentUpgradeLevel);
        }

        if (deckVersion != null)
        {
            await CardPileCmd.RemoveFromDeck(deckVersion, showPreview: true);
        }
        if (Pile != null && Pile.IsCombatPile && !HasBeenRemovedFromState)
        {
            await CardCmd.Exhaust(choiceContext, this);
        }

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

        CardModel chosen;
        if (CombatManager.Instance.IsEnding || !CombatManager.Instance.IsInProgress)
        {
            CardModel? endingChoice = await ChooseCardDuringCombatEnding(owner, rewardCards);
            if (endingChoice != null)
            {
                chosen = endingChoice;
            }
            else
            {
                List<CardModel> monsterRewards = rewardCards.Where(card => card is not SoulCapture).ToList();
                List<CardModel> fallbackPool = monsterRewards.Count > 0 ? monsterRewards : rewardCards;
                chosen = owner.RunState.Rng.CombatCardGeneration.NextItem(fallbackPool) ?? fallbackPool[0];
            }
        }
        else
        {
            List<CardCreationResult> options = rewardCards.Select(card => new CardCreationResult(card)).ToList();
            CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1);
            List<CardModel> selected = (await CardSelectCmd.FromSimpleGridForRewards(choiceContext, options, owner, prefs)).ToList();
            chosen = selected.Count > 0 ? selected[0] : rewardCards[0];
        }

        CardPileAddResult addResult = await CardPileCmd.Add(chosen, PileType.Deck);
        if (addResult.success)
        {
            CardCmd.PreviewCardPileAdd(addResult, 2f);
        }
    }

    private static async Task<CardModel?> ChooseCardDuringCombatEnding(Player owner, List<CardModel> rewardCards)
    {
        if (!LocalContext.IsMe(owner))
        {
            return null;
        }

        NChooseACardSelectionScreen? screen = NChooseACardSelectionScreen.ShowScreen(rewardCards, canSkip: false);
        if (screen == null)
        {
            return null;
        }

        List<CardModel> selected = (await screen.CardsSelected()).ToList();
        return selected.FirstOrDefault();
    }
}
