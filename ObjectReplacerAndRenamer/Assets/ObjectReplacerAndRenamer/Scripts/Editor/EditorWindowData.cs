using System.Collections.Generic;
using UnityEngine;

namespace GursaanjTools
{
    public readonly struct EditorWindowInformation
    {
        public GUIContent Title { get; }
        public Vector2 MinSize { get; }
        public Vector2 MaxSize { get; }

        public EditorWindowInformation(GUIContent title, Vector2 minSize, Vector2 maxSize)
        {
            Title = title;
            MinSize = minSize;
            MaxSize = maxSize;
        }
    }
    
    public static class EditorWindowData
    {
        public static Dictionary<string, EditorWindowInformation> EditorWindowInformations { get; } = new Dictionary<string, EditorWindowInformation>
        {
            {"Object Replacer", new EditorWindowInformation(new GUIContent("Replace Selected Objects"),new Vector2(300,100), new Vector2(300,175))},
            {"Object Renamer", new EditorWindowInformation(new GUIContent("Rename Selected Objects"), new Vector2(300,140), new Vector2(300, 180))},
            {"Object Grouper", new EditorWindowInformation(new GUIContent("Group Selected Objects"),new Vector2(300, 140), new Vector2(300, 140))},
            {"Object Aligner", new EditorWindowInformation(new GUIContent("Align Selected Objects"), new Vector2(330, 260), new Vector2(330, 260))}
            
        };
    }
}
