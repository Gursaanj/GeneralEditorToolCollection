using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using GursaanjTools;
using UnityEditor;
using UnityEngine;

public class FontReference_Editor : GuiControlEditorWindow
{
    #region Variables
    
    //GUI Labels
    
    //Warning Labels

    private const string FontsPath = "Fonts";
    
    private List<Font> _fonts = new List<Font>();
    private EditorResourceLogic _logic;
    
    private Vector2 _scrollPosition = Vector2.zero;
    
    #endregion

    #region BuiltIn Mehtods

    private void OnEnable()
    {
        _logic = new EditorResourceLogic();
        List<string> fontNames =
            _logic.GetAppropriateNames(_logic.GetEditorAssetBundle(), GetFontsPath(), new[] {".ttf", ".otf"});

        if (fontNames == null)
        {
            return;
        }

        foreach (string fontName in fontNames)
        {
            if (string.IsNullOrEmpty(fontName))
            {
                continue;
            }

            Font font = (Font) EditorGUIUtility.Load(fontName);

            if (font.Equals(null))
            {
                Debug.Log("damn");
                continue;
            }

            _fonts.Add(font);
        }

    }

    protected override void CreateGUI(string controlName)
    {
        using (new GUILayout.VerticalScope())
        {
            using (var scrollScope = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;

                foreach (Font font in _fonts)
                {
                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(60f)))
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("EXAMPLE Text", new GUIStyle{font = font});
                        GUILayout.FlexibleSpace();
                    }
                }
            }
        }
    }

    #endregion

    #region Custom Methods

    private string GetFontsPath()
    {
#if UNITY_2018_2_OR_NEWER
        
        return UnityEditor.Experimental.EditorResources.fontsPath;
#else
        var assembly = typeof(EditorGUIUtility).Assembly;
        var resourceUtility = assembly.GetType(_logic.EditorResourceUtility);
        var pathProperty = resourceUtility.GetProperty(FontsPath, BindingFlags.Static | BindingFlags.Public);
        return (string)pathProperty.GetValue(null, new object[] { });
#endif
    }
    

    #endregion
}
