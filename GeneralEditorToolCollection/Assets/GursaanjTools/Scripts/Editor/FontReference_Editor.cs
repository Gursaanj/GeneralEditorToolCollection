using System;
using System.Collections.Generic;
using System.Reflection;
using GursaanjTools;
using UnityEditor;
using UnityEngine;

public class FontReference_Editor : GuiControlEditorWindow
{
    #region Variables
    
    //GUI Labels
    private const string FontNameLabel = "Font Name:";
    private const string FullMethodLabel = "Full Method";

    private const float FontBoxHeight = 40f;
    private const float FontNamePadding = 10f;
    private const float PreviewMaxHeight = 130f;
    private const float VerticalPreviewPadding = 10f;
    private const float HorizontalPreviewPadding = 20f;
    private const float CopyContentWidth = 30f;
    private const float CopyContentHeight = 18f;
    
    //Warning Labels

    private const string FontsPath = "Fonts";

    private const int FixedFontSize = 15;
    
    private readonly string[] FontExtensions = new string[2] {".ttf", ".otf"};
    
    private List<GUIStyle> _styles = new List<GUIStyle>();
    private GUIStyle _currentStyle = GUIStyle.none;
    private EditorResourceLogic _logic;
    
    private Vector2 _scrollPosition = Vector2.zero;

    #endregion

    #region BuiltIn Mehtods

    private void OnEnable()
    {
        _logic = new EditorResourceLogic();
        List<string> fontNames = _logic.GetAppropriateNames(_logic.GetEditorAssetBundle(), GetFontsPath(), FontExtensions);

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
                Debug.Log($"{fontName} Could not be loaded");
                continue;
            }
            
            _styles.Add(GetGUIStyle(font));
        }
    }

    protected override void CreateGUI(string controlName)
    {
        using (new GUILayout.VerticalScope())
        {
            using (var scrollScope = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;

                for (int i = 0, count = _styles.Count; i < count; i++)
                {
                    GUIStyle style = _styles[i];
                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(FontBoxHeight)))
                    {
                        GUILayout.FlexibleSpace();
                        
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Space(FontNamePadding);
                            
                            if (GUILayout.Button(style.font.name, style))
                            {
                                _currentStyle = style;
                            }
                        }
                        
                        GUILayout.FlexibleSpace();
                    }
                }
            }

            if (_currentStyle.Equals(GUIStyle.none))
            {
                return;
            }
            
            GUILayout.FlexibleSpace();
            string fontName = $"\"{_currentStyle.font.name}.ttf\"";
            string fullMethodName = $"(Font) EditorGUIUtility.Load({fontName})";

            using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MaxHeight(PreviewMaxHeight)))
            {
                GUILayout.Space(VerticalPreviewPadding);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(HorizontalPreviewPadding);
                    GUILayout.Label(FontNameLabel, EditorStyles.boldLabel);
                    GUILayout.Label(fontName);
                    if (GUILayout.Button(_logic.CopyContent, GUILayout.Width(CopyContentWidth), GUILayout.Height(CopyContentHeight)))
                    {
                        EditorGUIUtility.systemCopyBuffer = fontName;
                    }
                    GUILayout.Space(HorizontalPreviewPadding);
                }
                
                GUILayout.Space(5f);

                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.VerticalScope())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(HorizontalPreviewPadding);
                            GUILayout.Label(FullMethodLabel, EditorStyles.boldLabel);
                            if (GUILayout.Button(_logic.CopyContent, GUILayout.Width(CopyContentWidth), GUILayout.Height(CopyContentHeight)))
                            {
                                EditorGUIUtility.systemCopyBuffer = fullMethodName;
                            }
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.Label(fullMethodName);
                    }
                }
            }
        }
        _logic.HandleContentEvents(ref _currentStyle);
    }

    #endregion

    #region Custom Methods

    private string GetFontsPath()
    {
#if UNITY_2018_3_OR_NEWER
        
        return UnityEditor.Experimental.EditorResources.fontsPath;
#else
        var assembly = typeof(EditorGUIUtility).Assembly;
        var resourceUtility = assembly.GetType(_logic.EditorResourceUtility);
        var pathProperty = resourceUtility.GetProperty(FontsPath, BindingFlags.Static | BindingFlags.Public);
        return (string)pathProperty.GetValue(null, new object[] { });
#endif
    }

    private GUIStyle GetGUIStyle(Font font)
    {
        GUIStyle style = new GUIStyle(EditorStyles.miniButton)
        {
            font = font,
            fontSize = FixedFontSize,
            alignment = TextAnchor.MiddleCenter
        };

        return style;
    }


    #endregion
}
