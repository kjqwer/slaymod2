using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
public sealed class SoulCapture() : CustomCardModel(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
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
            await RemoveDeckVersionAndAddRewardCard(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        AddKeyword(CardKeyword.Retain);
    }

    private async Task RemoveDeckVersionAndAddRewardCard(PlayerChoiceContext choiceContext)
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
            CardCreationOptions options = new CardCreationOptions(
                new CardPoolModel[] { ModelDb.CardPool<SoulCaptureRewardPool>() },
                CardCreationSource.Other,
                CardRarityOddsType.Uniform);
            CardReward reward = new CardReward(options, 1, owner);
            if (IsUpgraded)
            {
                reward.AfterGenerated += delegate
                {
                    foreach (CardModel card in reward.Cards)
                    {
                        if (card.IsUpgradable)
                        {
                            CardCmd.Upgrade(card, CardPreviewStyle.None);
                        }
                    }
                };
            }
            combatRoom.AddExtraReward(owner, reward);
        }
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
