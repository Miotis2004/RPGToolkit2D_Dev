using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Foundation
{
    public sealed class RPGToolkitGameBuilderWindow : EditorWindow
    {
        private string _selectedPageId;

        [MenuItem("Window/RPG Toolkit/Game Builder")]
        public static void Open()
        {
            GetWindow<RPGToolkitGameBuilderWindow>("RPG Game Builder").Show();
        }

        public void SelectPage(string pageId)
        {
            _selectedPageId = pageId;
            Repaint();
        }

        private void OnEnable()
        {
            RPGBuilderNavigation.RegisterPage(new RPGBuilderPage("foundation", "Foundation", 0, DrawFoundationPage));
        }

        private void OnGUI()
        {
            var pages = RPGBuilderNavigation.RegisteredPages;
            if (pages.Count == 0)
            {
                EditorGUILayout.HelpBox("No RPG Toolkit pages are registered.", MessageType.Info);
                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedPageId)) _selectedPageId = pages[0].Id;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(180));
            foreach (var page in pages)
            {
                if (GUILayout.Toggle(_selectedPageId == page.Id, page.Title, "Button")) _selectedPageId = page.Id;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            foreach (var page in pages)
            {
                if (_selectedPageId == page.Id)
                {
                    page.Draw?.Invoke();
                    break;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawFoundationPage()
        {
            EditorGUILayout.LabelField("Phase 0 Foundation", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Shared content IDs, manifests, validation, sprite import profiles, serialization versions, and extension interfaces are available for future RPG Toolkit systems.", MessageType.Info);
        }
    }
}
