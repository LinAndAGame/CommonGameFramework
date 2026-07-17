using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CommonGameFramework.Editor
{
    /// <summary>
    /// 推荐插件目录条目（与 RecommendedPackages.json 字段对应）。
    /// </summary>
    [Serializable]
    public sealed class RecommendedPackageEntry
    {
        public string displayName;
        public string packageId;
        public string description;
        /// <summary>非空则走 Git URL 安装；为空则用 packageId 从注册表安装。</summary>
        public string gitUrl;
        public bool defaultEnabled;
    }

    [Serializable]
    sealed class RecommendedPackageCatalogDto
    {
        public RecommendedPackageEntry[] packages;
    }

    /// <summary>
    /// 从 Editor 内 JSON 加载推荐插件列表。
    /// </summary>
    public static class RecommendedPackageCatalog
    {
        const string CatalogFileName = "RecommendedPackages.json";

        public static IReadOnlyList<RecommendedPackageEntry> Load()
        {
            var path = FindCatalogAssetPath();
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"[PackageInstaller] 未找到 {CatalogFileName}，请放在 Editor/PackageInstaller/ 下。");
                return Array.Empty<RecommendedPackageEntry>();
            }

            var fullPath = Path.GetFullPath(path);
            var json = File.ReadAllText(fullPath);
            var dto = JsonUtility.FromJson<RecommendedPackageCatalogDto>(json);
            if (dto?.packages == null || dto.packages.Length == 0)
            {
                Debug.LogWarning($"[PackageInstaller] {CatalogFileName} 为空或解析失败。");
                return Array.Empty<RecommendedPackageEntry>();
            }

            return dto.packages;
        }

        public static string FindCatalogAssetPath()
        {
            var guids = AssetDatabase.FindAssets("RecommendedPackages");
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(CatalogFileName, StringComparison.OrdinalIgnoreCase))
                    return assetPath;
            }

            return null;
        }

        /// <summary>传给 Client.Add 的标识：优先 gitUrl，否则 packageId。</summary>
        public static string GetInstallIdentifier(RecommendedPackageEntry entry)
        {
            if (entry == null)
                return null;
            return string.IsNullOrWhiteSpace(entry.gitUrl)
                ? entry.packageId
                : entry.gitUrl.Trim();
        }
    }
}
