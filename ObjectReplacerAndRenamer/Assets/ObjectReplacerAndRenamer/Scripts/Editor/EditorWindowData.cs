using System.Collections.Generic;
using UnityEngine;

namespace GursaanjTools
{
    public struct EditorWindowInformation
    {
        // private Vector2 _minSize;
        // private Vector2 _maxSize;

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
        public static Dictionary<string, EditorWindowInformation> EditorWindowDictionary =
            new Dictionary<string, EditorWindowInformation>
            {
                {"Object Replcer", new EditorWindowInformation(new GUIContent("Replace Selected Objects"),new Vector2(300,100), new Vector2(300,175))}
            };
        
        #region Object Windows

        // public static readonly GUIContent ObjectReplacer_Name = new GUIContent("Replace Selected Objects");
        // public static readonly Vector2 ObjectReplace_MinSize = new Vector2(300, 100);
        // public static readonly Vector2 ObjectReplace_MaxSize = new Vector2(300, 175);

        #endregion
    }
}
