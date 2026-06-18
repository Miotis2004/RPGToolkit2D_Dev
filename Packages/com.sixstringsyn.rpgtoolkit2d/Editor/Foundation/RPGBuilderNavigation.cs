using System;
using System.Collections.Generic;
using UnityEditor;

namespace SixStringSyn.RPGToolkit2D.Editor.Foundation
{
    public readonly struct RPGBuilderPage
    {
        public RPGBuilderPage(string id, string title, int order, Action draw)
        {
            Id = id ?? string.Empty;
            Title = title ?? string.Empty;
            Order = order;
            Draw = draw;
        }

        public string Id { get; }
        public string Title { get; }
        public int Order { get; }
        public Action Draw { get; }
    }

    public static class RPGBuilderNavigation
    {
        private static readonly List<RPGBuilderPage> Pages = new List<RPGBuilderPage>();

        public static IReadOnlyList<RPGBuilderPage> RegisteredPages => Pages;

        public static void RegisterPage(RPGBuilderPage page)
        {
            if (string.IsNullOrWhiteSpace(page.Id)) return;
            Pages.RemoveAll(existing => string.Equals(existing.Id, page.Id, StringComparison.OrdinalIgnoreCase));
            Pages.Add(page);
            Pages.Sort((left, right) => left.Order == right.Order ? string.Compare(left.Title, right.Title, StringComparison.Ordinal) : left.Order.CompareTo(right.Order));
        }

        public static void OpenPage(string pageId)
        {
            var window = EditorWindow.GetWindow<RPGToolkitGameBuilderWindow>();
            window.SelectPage(pageId);
            window.Show();
        }
    }
}
