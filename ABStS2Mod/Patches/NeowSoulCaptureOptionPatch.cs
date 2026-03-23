using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ABStS2Mod.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;

namespace ABStS2Mod.Patches;

[HarmonyPatch(typeof(Neow), "GenerateInitialOptions")]
public static class NeowSoulCaptureOptionPatch
{
    private const string SoulCaptureOptionKey = "ABSTS2MOD_NEOW_SOUL_CAPTURE";
    private const string PositiveDoneDescriptionKey = "NEOW.pages.DONE.POSITIVE.description";
    private static readonly FieldInfo? CustomDonePageField = AccessTools.Field(typeof(AncientEventModel), "_customDonePage");
    private static readonly MethodInfo? DoneMethod = AccessTools.Method(typeof(AncientEventModel), "Done");

    public static void Postfix(Neow __instance, ref IReadOnlyList<EventOption> __result)
    {
        if (__instance.Owner?.RunState.Modifiers.Count > 0)
        {
            return;
        }

        List<EventOption> options = __result.ToList();
        if (options.Any(option => option.TextKey == SoulCaptureOptionKey))
        {
            return;
        }

        EventOption soulCaptureOption = new EventOption(
            __instance,
            () => OnSoulCaptureChosen(__instance),
            SoulCaptureOptionKey,
            HoverTipFactory.FromCardWithCardHoverTips<SoulCapture>());

        options.Add(soulCaptureOption);
        __result = options;
    }

    private static async Task OnSoulCaptureChosen(Neow neow)
    {
        if (neow.Owner == null)
        {
            neow.StartPreFinished();
            return;
        }

        int removeCount = neow.Owner.Deck.Cards.Count(card => card.IsRemovable);
        removeCount = removeCount > 3 ? 3 : removeCount;
        if (removeCount > 0)
        {
            List<CardModel> cardsToRemove = (await CardSelectCmd.FromDeckForRemoval(neow.Owner, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, removeCount))).ToList();
            await CardPileCmd.RemoveFromDeck(cardsToRemove);
        }

        CardModel[] soulCaptures = Enumerable.Range(0, 3)
            .Select(_ => (CardModel)neow.Owner.RunState.CreateCard<SoulCapture>(neow.Owner))
            .ToArray();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(soulCaptures, PileType.Deck), 2f);

        if (CustomDonePageField != null)
        {
            CustomDonePageField.SetValue(neow, PositiveDoneDescriptionKey);
        }

        if (DoneMethod != null)
        {
            DoneMethod.Invoke(neow, null);
        }
        else
        {
            neow.StartPreFinished();
        }
    }
}
