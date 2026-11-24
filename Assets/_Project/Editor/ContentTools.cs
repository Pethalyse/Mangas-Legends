using System.Collections.Generic;
using System.Linq;
using MangaLegends.Core.Content;
using MangaLegends.Data.Definitions;
using UnityEditor;
using UnityEngine;

namespace MangaLegends.Editor
{
    public static class ContentTools
    {
        private const string MenuRoot = "MangaLegends/Content/";

        [MenuItem(MenuRoot + "Validate")]
        public static void ValidateMenu()
        {
            bool ok = ValidateAll(out string summary);
            if (ok) Debug.Log(summary);
            else Debug.LogError(summary);
        }

        [MenuItem(MenuRoot + "Rebuild Registry From Project")]
        public static void RebuildRegistry()
        {
            ContentRegistry registry = FindAssets<ContentRegistry>().FirstOrDefault();
            if (registry == null)
            {
                Debug.LogError("No ContentRegistry asset found. Create one with Create > MangaLegends > Content Registry.");
                return;
            }

            List<ContentDefinition> entries = FindAssets<ContentDefinition>().OrderBy(d => d.Id).ToList();
            GameRulesConfig rules = registry.Rules != null ? registry.Rules : FindAssets<GameRulesConfig>().FirstOrDefault();

            Undo.RecordObject(registry, "Rebuild content registry");
            registry.SetEntries(rules, entries);
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log($"ContentRegistry rebuilt with {entries.Count} entries.");
        }

        public static bool ValidateAll(out string summary)
        {
            List<ContentRegistry> registries = FindAssets<ContentRegistry>().ToList();
            List<string> lines = new List<string>();
            bool ok = registries.Count > 0;
            if (!ok) lines.Add("No ContentRegistry asset found.");

            HashSet<ContentDefinition> registered = new HashSet<ContentDefinition>();
            foreach (ContentRegistry registry in registries)
            {
                ValidationReport report = registry.Validate();
                ok &= !report.HasErrors;
                lines.AddRange(report.Issues.Select(issue => issue.ToString()));
                registered.UnionWith(registry.Entries.Where(e => e != null));
            }

            foreach (ContentDefinition orphan in FindAssets<ContentDefinition>().Where(d => !registered.Contains(d)))
                lines.Add($"[Warning] {orphan.Id}: not referenced by any ContentRegistry ({AssetDatabase.GetAssetPath(orphan)}).");

            summary = $"Content validation {(ok ? "passed" : "FAILED")} ({lines.Count} issue(s)).\n" + string.Join("\n", lines);
            return ok;
        }

        // Entry point for batch mode: Unity -batchmode -quit -executeMethod MangaLegends.Editor.ContentTools.ValidateBatch
        public static void ValidateBatch()
        {
            bool ok = ValidateAll(out string summary);
            Debug.Log(summary);
            EditorApplication.Exit(ok ? 0 : 1);
        }

        private static IEnumerable<T> FindAssets<T>() where T : Object
        {
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null) yield return asset;
            }
        }
    }
}
