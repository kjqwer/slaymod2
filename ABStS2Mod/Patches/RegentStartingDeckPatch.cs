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
    private const string RegentStrikeId = "STRIKE_REGENT";

    public static void Postfix(ref IEnumerable<CardModel> __result)
    {
        var newDeck = new List<CardModel>(__result);
        if (!newDeck.Any(card => card.Id == ModelDb.Card<SoulCapture>().Id))
        {
//          newDeck.Add(ModelDb.Card<SoulCapture>());
//          newDeck.Add(ModelDb.Card<SoulCapture>());
            newDeck.Add(ModelDb.Card<SoulMonsterSlimedBerserker>());
            newDeck.Add(ModelDb.Card<SoulMonsterOwlMagistrate>());
            // newDeck.Add(ModelDb.Card<SoulMonsterOwlMagistrate>());
            // for (int i = 0; i < 2; i++)
            // {
            //     int strikeIndex = newDeck.FindIndex(card => card.Id.Entry == RegentStrikeId);
            //     if (strikeIndex < 0)
            //     {
            //         break;
            //     }

            //     newDeck.RemoveAt(strikeIndex);
            // }
        }
        // if (!newDeck.Any(card => card.Id == ModelDb.Card<SoulCaptureTest>().Id))
        // {
        //     newDeck.Add(ModelDb.Card<SoulCaptureTest>());
        // }
        __result = newDeck;
    }
}
