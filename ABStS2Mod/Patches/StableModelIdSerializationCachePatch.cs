using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using System.Reflection;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Timeline;

namespace ABStS2Mod.Patches;

[HarmonyPatch(typeof(ModelIdSerializationCache), nameof(ModelIdSerializationCache.Init))]
public static class StableModelIdSerializationCachePatch
{
    [HarmonyPrefix]
    public static bool Prefix()
    {
        try
        {
            byte[] buffer = new byte[512];
            XxHash32 xxHash = new();

            Dictionary<string, int> categoryNameToNetIdMap = GetField<Dictionary<string, int>>("_categoryNameToNetIdMap");
            List<string> netIdToCategoryNameMap = GetField<List<string>>("_netIdToCategoryNameMap");
            Dictionary<string, int> entryNameToNetIdMap = GetField<Dictionary<string, int>>("_entryNameToNetIdMap");
            List<string> netIdToEntryNameMap = GetField<List<string>>("_netIdToEntryNameMap");
            Dictionary<string, int> epochNameToNetIdMap = GetField<Dictionary<string, int>>("_epochNameToNetIdMap");
            List<string> netIdToEpochNameMap = GetField<List<string>>("_netIdToEpochNameMap");

            categoryNameToNetIdMap.Clear();
            categoryNameToNetIdMap[ModelId.none.Category] = 0;
            netIdToCategoryNameMap.Clear();
            netIdToCategoryNameMap.Add(ModelId.none.Category);

            entryNameToNetIdMap.Clear();
            entryNameToNetIdMap[ModelId.none.Entry] = 0;
            netIdToEntryNameMap.Clear();
            netIdToEntryNameMap.Add(ModelId.none.Entry);

            epochNameToNetIdMap.Clear();
            netIdToEpochNameMap.Clear();

            List<ModelId> modelIds = ModelDb.AllAbstractModelSubtypes.Select(ModelDb.GetId).Distinct().ToList();
            modelIds.Sort(CompareModelId);
            foreach (ModelId modelId in modelIds)
            {
                if (!categoryNameToNetIdMap.ContainsKey(modelId.Category))
                {
                    categoryNameToNetIdMap[modelId.Category] = netIdToCategoryNameMap.Count;
                    netIdToCategoryNameMap.Add(modelId.Category);
                }

                if (!entryNameToNetIdMap.ContainsKey(modelId.Entry))
                {
                    entryNameToNetIdMap[modelId.Entry] = netIdToEntryNameMap.Count;
                    netIdToEntryNameMap.Add(modelId.Entry);
                }

                int bytes = Encoding.UTF8.GetBytes(modelId.Category, 0, modelId.Category.Length, buffer, 0);
                xxHash.Append(buffer.AsSpan(0, bytes));
                bytes = Encoding.UTF8.GetBytes(modelId.Entry, 0, modelId.Entry.Length, buffer, 0);
                xxHash.Append(buffer.AsSpan(0, bytes));
            }

            List<string> epochIds = EpochModel.AllEpochIds.Distinct(StringComparer.Ordinal).ToList();
            epochIds.Sort(StringComparer.Ordinal);
            foreach (string epochId in epochIds)
            {
                if (!epochNameToNetIdMap.ContainsKey(epochId))
                {
                    epochNameToNetIdMap[epochId] = netIdToEpochNameMap.Count;
                    netIdToEpochNameMap.Add(epochId);
                }

                int bytes = Encoding.UTF8.GetBytes(epochId, 0, epochId.Length, buffer, 0);
                xxHash.Append(buffer.AsSpan(0, bytes));
            }

            int maxCategoryId = netIdToCategoryNameMap.Count;
            int maxEntryId = netIdToEntryNameMap.Count;
            int maxEpochId = netIdToEpochNameMap.Count;

            SetProperty("CategoryIdBitSize", ComputeBitSize(maxCategoryId));
            SetProperty("EntryIdBitSize", ComputeBitSize(maxEntryId));
            SetProperty("EpochIdBitSize", ComputeBitSize(maxEpochId));

            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(), maxCategoryId);
            xxHash.Append(buffer.AsSpan(0, 4));
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(), maxEntryId);
            xxHash.Append(buffer.AsSpan(0, 4));
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(), maxEpochId);
            xxHash.Append(buffer.AsSpan(0, 4));

            SetProperty("Hash", xxHash.GetCurrentHashAsUInt32());
            MainFile.Logger.Info($"[MultiplayerFix] Stable ModelId hash initialized. Categories: {maxCategoryId} Entries: {maxEntryId} Epochs: {maxEpochId} Hash: {ModelIdSerializationCache.Hash}");
            return false;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Error($"[MultiplayerFix] Failed to apply stable ModelId hash patch: {ex}");
            return true;
        }
    }

    private static int CompareModelId(ModelId id1, ModelId id2)
    {
        int categoryCompare = string.CompareOrdinal(id1.Category, id2.Category);
        if (categoryCompare != 0)
        {
            return categoryCompare;
        }

        return string.CompareOrdinal(id1.Entry, id2.Entry);
    }

    private static int ComputeBitSize(int count)
    {
        if (count <= 1)
        {
            return 0;
        }

        return Mathf.CeilToInt(Math.Log2(count));
    }

    private static T GetField<T>(string fieldName) where T : class
    {
        FieldInfo field = AccessTools.Field(typeof(ModelIdSerializationCache), fieldName);
        return (field.GetValue(null) as T)!;
    }

    private static void SetProperty(string propertyName, object value)
    {
        PropertyInfo property = typeof(ModelIdSerializationCache).GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        property.SetValue(null, value);
    }
}
