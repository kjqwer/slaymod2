using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ABStS2Mod.Cards.MonsterSouls;
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
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

        bool wasTargetDefeated = target.IsDead || attack.Results.Any((DamageResult result) => result.WasTargetKilled);
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
        CombatRoom? combatRoom = owner.RunState.CurrentRoom as CombatRoom;

        CardModel? deckVersion = DeckVersion;
        if (deckVersion == null)
        {
            deckVersion = owner.Deck.Cards.FirstOrDefault((CardModel card) => card.Id == Id && card.IsUpgraded == IsUpgraded);
        }

        if (deckVersion == null)
        {
            return;
        }

        await CardPileCmd.RemoveFromDeck(deckVersion, showPreview: true);
        if (Pile != null && Pile.IsCombatPile && !HasBeenRemovedFromState)
        {
            await CardCmd.Exhaust(choiceContext, this);
        }

        if (combatRoom != null)
        {
            CardModel monsterRewardCard = MonsterSoulCardRegistry.Create(owner, defeatedMonsterId);
            CardModel soulCaptureRewardCard = owner.RunState.CreateCard<SoulCapture>(owner);
            if (IsUpgraded && monsterRewardCard.IsUpgradable)
            {
                CardCmd.Upgrade(monsterRewardCard, CardPreviewStyle.None);
            }
            if (IsUpgraded && soulCaptureRewardCard.IsUpgradable)
            {
                CardCmd.Upgrade(soulCaptureRewardCard, CardPreviewStyle.None);
            }
            CardCreationOptions rewardOptions = new CardCreationOptions(
                new CardPoolModel[] { ModelDb.CardPool<ColorlessCardPool>() },
                CardCreationSource.Other,
                CardRarityOddsType.Uniform);
            CardReward reward = new CardReward(rewardOptions, 2, owner);
            ForceRewardCards(reward, monsterRewardCard, soulCaptureRewardCard);
            combatRoom.AddExtraReward(owner, reward);
        }
    }

    private static void ForceRewardCards(CardReward reward, params CardModel[] rewardCards)
    {
        FieldInfo? cardsField = typeof(CardReward).GetField("_cards", BindingFlags.Instance | BindingFlags.NonPublic);
        FieldInfo? manualField = typeof(CardReward).GetField("_cardsWereManuallySet", BindingFlags.Instance | BindingFlags.NonPublic);
        if (cardsField == null || manualField == null)
        {
            return;
        }
        List<CardCreationResult>? cards = cardsField.GetValue(reward) as List<CardCreationResult>;
        if (cards == null)
        {
            return;
        }
        cards.Clear();
        foreach (CardModel rewardCard in rewardCards)
        {
            cards.Add(new CardCreationResult(rewardCard));
        }
        manualField.SetValue(reward, true);
    }
}

public sealed class SoulCaptureRewardPool : CardPoolModel
{
    public override string Title => "soul_capture_reward";
    public override string EnergyColorName => "colorless";
    public override string CardFrameMaterialPath => "card_frame_colorless";
    public override Color DeckEntryCardColor => new Color("A3A3A3FF");
    public override bool IsColorless => true;

    protected override CardModel[] GenerateAllCards()
    {
        return new CardModel[] { ModelDb.Card<SoulCaptureTest>() };
    }
}
