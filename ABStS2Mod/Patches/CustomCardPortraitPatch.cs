using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Patches;

[HarmonyPatch(typeof(CardModel))]
public static class CustomCardPortraitPatch
{
    [HarmonyPatch(nameof(CardModel.PortraitPath), MethodType.Getter)]
    [HarmonyPostfix]
    public static void PortraitPathPostfix(CardModel __instance, ref string __result)
    {
        string? path = ResolveCustomPortraitPath(__instance, false);
        if (!string.IsNullOrEmpty(path))
        {
            __result = path;
        }
    }

    [HarmonyPatch(nameof(CardModel.BetaPortraitPath), MethodType.Getter)]
    [HarmonyPostfix]
    public static void BetaPortraitPathPostfix(CardModel __instance, ref string __result)
    {
        string? path = ResolveCustomPortraitPath(__instance, true);
        if (!string.IsNullOrEmpty(path))
        {
            __result = path;
        }
    }

    private static string? ResolveCustomPortraitPath(CardModel card, bool beta)
    {
        string entry = card.Id.Entry;
        if (!entry.StartsWith("ABSTS2MOD-", System.StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string idLower = entry.ToLowerInvariant();
        string shortId = entry;
        const string soulMonsterPrefix = "ABSTS2MOD-SOUL_MONSTER_";
        if (entry.StartsWith(soulMonsterPrefix, System.StringComparison.OrdinalIgnoreCase))
        {
            shortId = entry[soulMonsterPrefix.Length..];
        }
        else if (entry.StartsWith("ABSTS2MOD-", System.StringComparison.OrdinalIgnoreCase))
        {
            shortId = entry["ABSTS2MOD-".Length..];
        }

        string shortLower = shortId.ToLowerInvariant();
        string shortUpper = shortId.ToUpperInvariant();
        string shortFlat = shortLower.Replace("_", "").Replace("-", "");
        string betaFolder = beta ? "/beta" : "";

        HashSet<string> candidates = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            $"res://ABStS2Mod/images/cards{betaFolder}/{entry}.png",
            $"res://ABStS2Mod/images/cards{betaFolder}/{idLower}.png",
            $"res://ABStS2Mod/images/cards{betaFolder}/{shortId}.png",
            $"res://ABStS2Mod/images/cards{betaFolder}/{shortUpper}.png",
            $"res://ABStS2Mod/images/cards{betaFolder}/{shortLower}.png",
            $"res://ABStS2Mod/images/cards{betaFolder}/{shortFlat}.png",
            $"res://ABStS2Mod/images/{shortUpper}.png",
            $"res://ABStS2Mod/images/{shortLower}.png"
        };

        foreach (string candidate in candidates)
        {
            if (ResourceLoader.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }
}
