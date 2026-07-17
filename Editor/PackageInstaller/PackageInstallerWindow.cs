using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace CommonGameFramework.Editor
{
    /// <summary>
    /// 勾选常用插件并经 UPM 安装（支持 Git URL 与注册表包名）。
    /// 默认勾选状态来自 RecommendedPackages.json 的 defaultEnabled。
    /// </summary>
    public sealed class PackageInstallerWindow : EditorWindow
    {
        Vector2 _scroll;
        readonly List<RowState> _rows = new();
        HashSet<string> _installedIds = new();
        ListRequest _listRequest;
        AddRequest _addRequest;
        Queue<string> _pendingInstalls;
        string _status = "就绪";
        bool _busy;

        sealed class RowState
        {
            public RecommendedPackageEntry Entry;
            public bool Selected;
        }

        [MenuItem("Tools/CommonGameFramework/常用插件安装")]
        public static void Open()
        {
            var window = GetWindow<PackageInstallerWindow>("常用插件安装");
            window.minSize = new Vector2(480, 360);
            window.Show();
        }

        void OnEnable()
        {
            ReloadCatalog();
            RefreshInstalledPackages();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("推荐插件", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "勾选后点「安装选中」。Git 包用 gitUrl；无 gitUrl 则按 packageId 从注册表安装。\n" +
                "默认勾选由 RecommendedPackages.json 的 defaultEnabled 控制。",
                MessageType.Info);

            DrawToolbar();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (var row in _rows)
                DrawRow(row);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            using (new EditorGUI.DisabledScope(_busy || _rows.Count == 0))
            {
                if (GUILayout.Button("安装选中", GUILayout.Height(32)))
                    BeginInstallSelected();
            }

            EditorGUILayout.LabelField("状态", _status, EditorStyles.wordWrappedLabel);
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("全选默认", GUILayout.Width(88)))
                    ApplyDefaultSelection();
                if (GUILayout.Button("全选未安装", GUILayout.Width(96)))
                {
                    foreach (var row in _rows)
                        row.Selected = !IsInstalled(row.Entry);
                }

                if (GUILayout.Button("全不选", GUILayout.Width(64)))
                {
                    foreach (var row in _rows)
                        row.Selected = false;
                }

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(_busy))
                {
                    if (GUILayout.Button("刷新已安装", GUILayout.Width(96)))
                        RefreshInstalledPackages();
                    if (GUILayout.Button("重载配置", GUILayout.Width(80)))
                        ReloadCatalog();
                }
            }
        }

        void DrawRow(RowState row)
        {
            var entry = row.Entry;
            var installed = IsInstalled(entry);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(_busy || installed))
                        row.Selected = EditorGUILayout.ToggleLeft(
                            entry.displayName ?? entry.packageId,
                            row.Selected,
                            EditorStyles.boldLabel);

                    var badge = installed ? "已安装" : (entry.defaultEnabled ? "默认" : "可选");
                    var badgeColor = installed ? Color.green : (entry.defaultEnabled ? new Color(0.4f, 0.7f, 1f) : Color.gray);
                    var prev = GUI.contentColor;
                    GUI.contentColor = badgeColor;
                    GUILayout.Label(badge, GUILayout.Width(48));
                    GUI.contentColor = prev;
                }

                if (!string.IsNullOrEmpty(entry.description))
                    EditorGUILayout.LabelField(entry.description, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.LabelField("Id", entry.packageId ?? "—", EditorStyles.miniLabel);
                var source = string.IsNullOrWhiteSpace(entry.gitUrl) ? "(注册表)" : entry.gitUrl;
                EditorGUILayout.LabelField("源", source, EditorStyles.miniLabel);
            }
        }

        void ReloadCatalog()
        {
            _rows.Clear();
            var packages = RecommendedPackageCatalog.Load();
            foreach (var entry in packages)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.packageId))
                    continue;
                _rows.Add(new RowState
                {
                    Entry = entry,
                    Selected = entry.defaultEnabled
                });
            }

            // 已安装的取消勾选，避免重复安装
            foreach (var row in _rows)
            {
                if (IsInstalled(row.Entry))
                    row.Selected = false;
            }

            _status = $"已加载 {_rows.Count} 个推荐插件";
            Repaint();
        }

        void ApplyDefaultSelection()
        {
            foreach (var row in _rows)
                row.Selected = row.Entry.defaultEnabled && !IsInstalled(row.Entry);
        }

        bool IsInstalled(RecommendedPackageEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.packageId))
                return false;
            return _installedIds.Contains(entry.packageId);
        }

        void RefreshInstalledPackages()
        {
            if (_listRequest != null && !_listRequest.IsCompleted)
                return;

            _status = "正在查询已安装包…";
            _listRequest = Client.List(true, true);
            EditorApplication.update += PollListRequest;
        }

        void PollListRequest()
        {
            if (_listRequest == null || !_listRequest.IsCompleted)
                return;

            EditorApplication.update -= PollListRequest;

            if (_listRequest.Status == StatusCode.Success)
            {
                _installedIds = new HashSet<string>(
                    _listRequest.Result.Select(p => p.name));
                _status = $"已安装包 {_installedIds.Count} 个";

                foreach (var row in _rows)
                {
                    if (IsInstalled(row.Entry))
                        row.Selected = false;
                }
            }
            else
            {
                _status = $"查询失败: {_listRequest.Error?.message}";
                Debug.LogError($"[PackageInstaller] {_status}");
            }

            _listRequest = null;
            Repaint();
        }

        void BeginInstallSelected()
        {
            var ids = _rows
                .Where(r => r.Selected && !IsInstalled(r.Entry))
                .Select(r => RecommendedPackageCatalog.GetInstallIdentifier(r.Entry))
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                _status = "没有可安装的选中项（可能均已安装）";
                return;
            }

            _pendingInstalls = new Queue<string>(ids);
            _busy = true;
            InstallNext();
        }

        void InstallNext()
        {
            if (_pendingInstalls == null || _pendingInstalls.Count == 0)
            {
                _busy = false;
                _status = "安装完成";
                RefreshInstalledPackages();
                Repaint();
                return;
            }

            var next = _pendingInstalls.Dequeue();
            _status = $"正在安装: {next}";
            _addRequest = Client.Add(next);
            EditorApplication.update += PollAddRequest;
            Repaint();
        }

        void PollAddRequest()
        {
            if (_addRequest == null || !_addRequest.IsCompleted)
                return;

            EditorApplication.update -= PollAddRequest;

            if (_addRequest.Status == StatusCode.Success)
            {
                var name = _addRequest.Result?.name ?? "(unknown)";
                Debug.Log($"[PackageInstaller] 已安装: {name}");
                if (!string.IsNullOrEmpty(name))
                    _installedIds.Add(name);
            }
            else
            {
                var msg = _addRequest.Error?.message ?? "未知错误";
                Debug.LogError($"[PackageInstaller] 安装失败: {msg}");
                _status = $"安装失败: {msg}（继续后续项）";
            }

            _addRequest = null;
            InstallNext();
        }
    }
}
