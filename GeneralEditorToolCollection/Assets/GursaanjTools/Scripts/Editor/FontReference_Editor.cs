using System;
using System.Collections.Generic;
using System.Reflection;
using GursaanjTools;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

public class FontReference_Editor : GuiControlEditorWindow
{
    #region Variables
    
    //GUI Labels
    
    //Warning Labels

    private const string FontsPath = "Fonts";
    
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

                for (int i = 0, count =_styles.Count; i < count; i++)
                {
                    GUIStyle style = _styles[i];
                    using (new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.Height(40f)))
                    {
                        GUILayout.FlexibleSpace();
                        
                        using (new GUILayout.VerticalScope())
                        {
                            GUILayout.Space(10f);
                            
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

            using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MaxHeight(130f)))
            {
                GUILayout.Space(10f);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(20f);
                    GUILayout.Label("Font Name:", EditorStyles.boldLabel);
                    GUILayout.Label(fontName);
                    if (GUILayout.Button(_logic.CopyContent, GUILayout.Width(30f), GUILayout.Height(18f)))
                    {
                        EditorGUIUtility.systemCopyBuffer = fontName;
                    }
                    GUILayout.Space(20f);
                }
                
                GUILayout.Space(5f);

                using (new GUILayout.HorizontalScope())
                {
                    using (new GUILayout.VerticalScope())
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Space(20f);
                            GUILayout.Label("Full Method", EditorStyles.boldLabel);
                            if (GUILayout.Button(_logic.CopyContent, GUILayout.Width(30f), GUILayout.Height(18f)))
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
            fontSize = 15,
            alignment = TextAnchor.MiddleCenter
        };

        return style;
    }


    #endregion
}
