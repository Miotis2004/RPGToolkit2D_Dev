using System.Linq;
using SixStringSyn.RPGToolkit2D.Runtime.Maps;
using UnityEditor;
using UnityEngine;

namespace SixStringSyn.RPGToolkit2D.Editor.Windows
{
    public sealed class MapConnectionBrowserWindow : EditorWindow
    {
        private RPGMapDefinition _map;
        private Vector2 _scroll;

        [MenuItem("Tools/RPG Toolkit/Maps/Connection Browser")]
        public static void ShowWindow() => GetWindow<MapConnectionBrowserWindow>("Map Connections");

        private void OnGUI()
        {
            _map = (RPGMapDefinition)EditorGUILayout.ObjectField("Map", _map, typeof(RPGMapDefinition), false);
            if (_map == null)
            {
                EditorGUILayout.HelpBox("Select an RPG map to review outgoing exits, authored connections, and validation results.", MessageType.Info);
                return;
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawExits();
            DrawConnections();
            DrawValidation();
            EditorGUILayout.EndScrollView();
        }

        private void DrawExits()
        {
            EditorGUILayout.LabelField("Outgoing Exits", EditorStyles.boldLabel);
            foreach (var exit in _map.Exits.Where(exit => exit != null))
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"{exit.exitId} → {TargetLabel(exit.targetMap, exit.targetMapId)} / {exit.targetEntranceId}");
                if (exit.targetMap != null && GUILayout.Button("Ping", GUILayout.Width(48))) EditorGUIUtility.PingObject(exit.targetMap);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawConnections()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Linked Maps", EditorStyles.boldLabel);
            foreach (var connection in _map.Connections.Where(connection => connection != null))
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"{connection.connectionId} → {TargetLabel(connection.targetMap, connection.targetMapId)} / {connection.targetEntranceId}");
                if (connection.targetMap != null && GUILayout.Button("Ping", GUILayout.Width(48))) EditorGUIUtility.PingObject(connection.targetMap);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawValidation()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Connection Validation", EditorStyles.boldLabel);
            var messages = _map.ValidateMap().Messages.Where(message => message.Code.Contains("EXIT") || message.Code.Contains("CONNECTION")).ToArray();
            if (messages.Length == 0)
            {
                EditorGUILayout.HelpBox("No exit or connection issues found.", MessageType.Info);
                return;
            }

            foreach (var message in messages)
                EditorGUILayout.HelpBox($"{message.Code}: {message.Message}", MessageType.Warning);
        }

        private static string TargetLabel(RPGMapDefinition targetMap, string targetMapId) => targetMap != null ? targetMap.DisplayName : string.IsNullOrWhiteSpace(targetMapId) ? "<missing map>" : targetMapId;
    }
}
