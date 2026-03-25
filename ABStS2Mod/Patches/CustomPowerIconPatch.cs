using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace ABStS2Mod.Patches;

[HarmonyPatch(typeof(PowerModel))]
public static class CustomPowerIconPatch
{
    private static readonly HashSet<string> _loggedResolutions = new(System.StringComparer.OrdinalIgnoreCase);
    private static readonly object _logLock = new();

    [HarmonyPatch(nameof(PowerModel.PackedIconPath), MethodType.Getter)]
    [HarmonyPostfix]
    public static void PackedIconPathPostfix(PowerModel __instance, ref string __result)
    {
        string? path = ResolveCustomPowerSmallIconPath(__instance);
        if (!string.IsNullOrEmpty(path))
        {
            __result = path;
        }
    }

    [HarmonyPatch(nameof(PowerModel.IconPath), MethodType.Getter)]
    [HarmonyPostfix]
    public static void IconPathPostfix(PowerModel __instance, ref string __result)
    {
        string? path = ResolveCustomPowerSmallIconPath(__instance);
        if (!string.IsNullOrEmpty(path))
        {
            __result = path;
        }
    }

    [HarmonyPatch(nameof(PowerModel.Icon), MethodType.Getter)]
    [HarmonyPostfix]
    public static void IconPostfix(PowerModel __instance, ref Texture2D __result)
    {
        string? path = ResolveCustomPowerSmallIconPath(__instance);
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

    [HarmonyPatch(nameof(PowerModel.ResolvedBigIconPath), MethodType.Getter)]
    [HarmonyPostfix]
    public static void ResolvedBigIconPathPostfix(PowerModel __instance, ref string __result)
    {
        string? path = ResolveCustomPowerBigIconPath(__instance);
        if (!string.IsNullOrEmpty(path))
        {
            __result = path;
        }
    }

    [HarmonyPatch(nameof(PowerModel.BigIcon), MethodType.Getter)]
    [HarmonyPostfix]
    public static void BigIconPostfix(PowerModel __instance, ref Texture2D __result)
    {
        string? path = ResolveCustomPowerBigIconPath(__instance);
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

    private static string? ResolveCustomPowerSmallIconPath(PowerModel power)
    {
        if (!IsModPower(power.Id.Entry))
        {
            return null;
        }

        List<string> tried = new();
        foreach (string candidate in BuildSmallCandidates(power.Id.Entry))
        {
            tried.Add(candidate);
            if (CanLoadTexture(candidate))
            {
                LogResolution(power.Id.Entry, "small", candidate, tried);
                return candidate;
            }
        }

        LogResolution(power.Id.Entry, "small", null, tried);
        return null;
    }

    private static string? ResolveCustomPowerBigIconPath(PowerModel power)
    {
        if (!IsModPower(power.Id.Entry))
        {
            return null;
        }

        List<string> tried = new();
        foreach (string candidate in BuildBigCandidates(power.Id.Entry))
        {
            tried.Add(candidate);
            if (CanLoadTexture(candidate))
            {
                LogResolution(power.Id.Entry, "big", candidate, tried);
                return candidate;
            }
        }

        LogResolution(power.Id.Entry, "big", null, tried);
        return null;
    }

    private static void LogResolution(string entry, string size, string? matchedPath, List<string> tried)
    {
        if (!IsModPower(entry))
        {
            return;
        }

        string outcome = matchedPath == null ? "MISS" : "HIT";
        string key = $"{entry}|{size}|{outcome}";
        lock (_logLock)
        {
            if (!_loggedResolutions.Add(key))
            {
                return;
            }
        }

        if (matchedPath != null)
        {
            MainFile.Logger.Info($"[PowerIcon][{size}] {entry} -> {matchedPath}");
            return;
        }

        MainFile.Logger.Warn($"[PowerIcon][{size}] {entry} -> MISS. Tried: {string.Join(" | ", tried)}");
    }

    private static bool IsModPower(string entry)
    {
        return entry.StartsWith("ABSTS2MOD-", StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanLoadTexture(string path)
    {
        return ResourceLoader.Exists(path);
    }

    private static IEnumerable<string> BuildSmallCandidates(string entry)
    {
        foreach (string name in BuildNameCandidates(entry))
        {
            yield return $"res://ABStS2Mod/images/powers/{name}.png";
            yield return $"res://ABStS2Mod/images/{name}.png";
        }
    }

    private static IEnumerable<string> BuildBigCandidates(string entry)
    {
        foreach (string name in BuildNameCandidates(entry))
        {
            yield return $"res://ABStS2Mod/images/powers/big/{name}.png";
            yield return $"res://ABStS2Mod/images/powers/{name}.png";
            yield return $"res://ABStS2Mod/images/powers/beta/{name}.png";
            yield return $"res://ABStS2Mod/images/{name}.png";
        }
    }

    private static HashSet<string> BuildNameCandidates(string entry)
    {
        string entryLower = entry.ToLowerInvariant();
        string shortId = entry;
        if (entry.StartsWith("ABSTS2MOD-SOUL_MONSTER_", System.StringComparison.OrdinalIgnoreCase))
        {
            shortId = entry["ABSTS2MOD-SOUL_MONSTER_".Length..];
        }
        else if (entry.StartsWith("ABSTS2MOD-", System.StringComparison.OrdinalIgnoreCase))
        {
            shortId = entry["ABSTS2MOD-".Length..];
        }

        string shortUpper = shortId.ToUpperInvariant();
        string shortLower = shortId.ToLowerInvariant();
        string shortFlat = shortLower.Replace("_", "").Replace("-", "");
        string noPowerSuffix = shortLower.EndsWith("_power", System.StringComparison.Ordinal)
            ? shortLower[..^"_power".Length]
            : shortLower;
        string noPowerSuffixUpper = shortUpper.EndsWith("_POWER", System.StringComparison.Ordinal)
            ? shortUpper[..^"_POWER".Length]
            : shortUpper;
        string noSoulMonsterPrefix = shortLower.StartsWith("soul_monster_", System.StringComparison.Ordinal)
            ? shortLower["soul_monster_".Length..]
            : shortLower;
        string noSoulMonsterPrefixUpper = shortUpper.StartsWith("SOUL_MONSTER_", System.StringComparison.Ordinal)
            ? shortUpper["SOUL_MONSTER_".Length..]
            : shortUpper;
        string noSoulMonsterPrefixNoPowerSuffix = noSoulMonsterPrefix.EndsWith("_power", System.StringComparison.Ordinal)
            ? noSoulMonsterPrefix[..^"_power".Length]
            : noSoulMonsterPrefix;
        string noSoulMonsterPrefixNoPowerSuffixUpper = noSoulMonsterPrefixUpper.EndsWith("_POWER", System.StringComparison.Ordinal)
            ? noSoulMonsterPrefixUpper[..^"_POWER".Length]
            : noSoulMonsterPrefixUpper;

        return new HashSet<string>(System.StringComparer.Ordinal)
        {
            entry,
            entryLower,
            shortId,
            shortUpper,
            shortLower,
            shortFlat,
            noPowerSuffix,
            noPowerSuffixUpper,
            noSoulMonsterPrefix,
            noSoulMonsterPrefixUpper,
            noSoulMonsterPrefixNoPowerSuffix,
            noSoulMonsterPrefixNoPowerSuffixUpper
        };
    }
}
