using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using ABStS2Mod.Cards.MonsterSouls;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Cards;
using ABStS2Mod.Cards;

namespace ABStS2Mod.Patches;

[HarmonyPatch(typeof(Regent), nameof(Regent.StartingDeck), MethodType.Getter)]
public static class RegentStartingDeckPatch
{
    public static void Postfix(ref IEnumerable<CardModel> __result)
    {
        var newDeck = new List<CardModel>(__result);
        if (!newDeck.Any(card => card.Id == ModelDb.Card<SoulCapture>().Id))
        {
            newDeck.Add(ModelDb.Card<SoulCapture>());
            newDeck.Add(ModelDb.Card<SoulMonsterAssassinRubyRaider>());
        }
        // if (!newDeck.Any(card => card.Id == ModelDb.Card<SoulCaptureTest>().Id))
        // {
        //     newDeck.Add(ModelDb.Card<SoulCaptureTest>());
        // }
        __result = newDeck;
    }
}
