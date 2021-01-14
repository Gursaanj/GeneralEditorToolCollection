using System.Collections.Generic;
using UnityEngine;

namespace GursaanjTools
{
    public readonly struct EditorWindowInformation
    {
        public GUIContent Title { get; }
        public Vector2 MinSize { get; }
        public Vector2 MaxSize { get; }
        public bool ShowUtility { get; }

        public EditorWindowInformation(GUIContent title, Vector2 minSize, Vector2 maxSize, bool showUtility = false)
        {
            Title = title;
            MinSize = minSize;
            MaxSize = maxSize;
            ShowUtility = showUtility;
        }
    }
    
    public static class EditorWindowData
    {
        public static Dictionary<string, EditorWindowInformation> EditorWindowInformations { get; } = new Dictionary<string, EditorWindowInformation>
        {
            {"Object Replacer", new EditorWindowInformation(new GUIContent("Replace Selected Objects"),new Vector2(300,100), new Vector2(300,175))},
            {"Object Renamer", new EditorWindowInformation(new GUIContent("Rename Selected Objects"), new Vector2(300,140), new Vector2(300, 180))},
            {"Object Grouper", new EditorWindowInformation(new GUIContent("Group Selected Objects"),new Vector2(300, 140), new Vector2(300, 140))},
            {"Object Aligner", new EditorWindowInformation(new GUIContent("Align Selected Objects"), new Vector2(330, 260), new Vector2(330, 260))},
            {"GameObject Finder", new EditorWindowInformation(new GUIContent("Find References"), new Vector2(300, 400), new Vector2(300, 400))},
            {"Selection Log", new EditorWindowInformation(new GUIContent("View Selection Log"), new Vector2(300,200), new Vector2(300,200))},
            {"Gist Importer", new EditorWindowInformation(new GUIContent("Import Gist File"), new Vector2(350, 50), new Vector2(350, 50))},
            {"Camera Capture", new EditorWindowInformation(new GUIContent("Take Screenshot"), new Vector2(500, 400), new Vector2(800, 500))},
            {"Spotlight Searcher", new EditorWindowInformation(new GUIContent("Asset Search"), new Vector2(500, 500), new Vector2(500, 500))},
            {"IconContent Reference", new EditorWindowInformation(new GUIContent("Internal Editor Icons"), new Vector2(600, 300), new Vector2(600, 600))},
            {"Cursor Reference", new EditorWindowInformation(new GUIContent("Internal Cursors"), new Vector2(500, 250), new Vector2(600, 250))},
            {"Fonts Reference", new EditorWindowInformation(new GUIContent("Internal Fonts"), new Vector2(370, 400), new Vector2(370, 500))}
        };
    }
}