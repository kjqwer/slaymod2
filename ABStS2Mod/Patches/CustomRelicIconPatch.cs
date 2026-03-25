using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Patches;

[HarmonyPatch(typeof(RelicModel))]
public static class CustomRelicIconPatch
{
    [HarmonyPatch(nameof(RelicModel.PackedIconPath), MethodType.Getter)]
    [HarmonyPostfix]
    public static void PackedIconPathPostfix(RelicModel __instance, ref string __result)
    {
        string? path = ResolveCustomRelicSmallIconPath(__instance);
        if (!string.IsNullOrEmpty(path))
        {
            __result = path;
        }
    }

    [HarmonyPatch("PackedIconOutlinePath", MethodType.Getter)]
    [HarmonyPostfix]
    public static void PackedIconOutlinePathPostfix(RelicModel __instance, ref string __result)
    {
        string? path = ResolveCustomRelicSmallIconPath(__instance);
        if (!string.IsNullOrEmpty(path))
        {
            __result = path;
        }
    }

    [HarmonyPatch(nameof(RelicModel.BigIcon), MethodType.Getter)]
    [HarmonyPostfix]
    public static void BigIconPostfix(RelicModel __instance, ref Texture2D __result)
    {
        string? path = ResolveCustomRelicBigIconPath(__instance);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        Texture2D? texture = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
        if (texture != null)
        {
            __result = texture;
        }
    }

    private static string? ResolveCustomRelicSmallIconPath(RelicModel relic)
    {
        foreach (string candidate in BuildSmallCandidates(relic.Id.Entry))
        {
            if (ResourceLoader.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string? ResolveCustomRelicBigIconPath(RelicModel relic)
    {
        foreach (string candidate in BuildBigCandidates(relic.Id.Entry))
        {
            if (ResourceLoader.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<string> BuildSmallCandidates(string entry)
    {
        foreach (string name in BuildNameCandidates(entry))
        {
            yield return $"res://ABStS2Mod/images/relics/{name}.png";
            yield return $"res://ABStS2Mod/images/{name}.png";
        }
    }

    private static IEnumerable<string> BuildBigCandidates(string entry)
    {
        foreach (string name in BuildNameCandidates(entry))
        {
            yield return $"res://ABStS2Mod/images/relics/big/{name}.png";
            yield return $"res://ABStS2Mod/images/relics/{name}.png";
            yield return $"res://ABStS2Mod/images/{name}.png";
        }
    }

    private static HashSet<string> BuildNameCandidates(string entry)
    {
        string entryLower = entry.ToLowerInvariant();
        string shortId = entry;
        if (entry.StartsWith("ABSTS2MOD-", System.StringComparison.OrdinalIgnoreCase))
        {
            shortId = entry["ABSTS2MOD-".Length..];
        }

        string shortUpper = shortId.ToUpperInvariant();
        string shortLower = shortId.ToLowerInvariant();
        string shortFlat = shortLower.Replace("_", "").Replace("-", "");
        string noRelicSuffix = shortLower.EndsWith("_relic", System.StringComparison.Ordinal)
            ? shortLower[..^"_relic".Length]
            : shortLower;
        string noSoulPrefix = shortLower.StartsWith("soul_", System.StringComparison.Ordinal)
            ? shortLower["soul_".Length..]
            : shortLower;

        return new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            entry,
            entryLower,
            shortId,
            shortUpper,
            shortLower,
            shortFlat,
            noRelicSuffix,
            noSoulPrefix
        };
    }
}
