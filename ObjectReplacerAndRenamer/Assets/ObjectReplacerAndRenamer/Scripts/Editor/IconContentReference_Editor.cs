using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public enum IconSize
    {
        Small,
        Medium,
        Large
    }

    public class IconContentReference_Editor : GuiControlEditorWindow
    {
        #region Variables
        
        //GUI Labels
        private const string IconSizesLabel = "Filter icons by size";
        private const string ClearIcon = "winbtn_win_close";
        
        private const float IconSizeLabelWidth = 120f;
        private const float IconSizesWidth = 180f;
        private const float ClearButtonWidth = 20f;

        private const float ScrollBarWidth = 13f;
        
        //Warning Labels
        private const string NoIconsFoundLabel = "No Icons Found!!";


        private const string EditorAssetBundleMethod = "GetEditorAssetBundle";
        private const string EditorResourceUtility = "UnityEditorInternal.EditorResourcesUtility";
        private const string IconsPath = "iconsPath";

        private const string PngFileExtension = ".png";
        private const string AssetFileExtension = ".asset";

        private const int SmallToMediumLimit = 36;
        private const int MediumToLargeLimit = 72;

        private const float SmallButtonSize = 40f;
        private const float MediumButtonSize = 70f;
        private const float LargeButtonSize = 100f;

        private readonly string[] IconSizes = Enum.GetNames(typeof(IconSize));
        
        private List<string> _iconNames = new List<string>();
        private List<GUIContent> _smallIcons = new List<GUIContent>();
        private List<GUIContent> _mediumIcons = new List<GUIContent>();
        private List<GUIContent> _largeIcons = new List<GUIContent>();
        private List<GUIContent> _currentSelectionOfIcons = new List<GUIContent>();
        private GUIContent _currentlySelectedIcon = GUIContent.none;
        private Vector2 _scrollPosition = Vector2.zero;
        private float _buttonSize = SmallButtonSize;

        //GUI Fields
        private string _searchField = string.Empty;
        private IconSize _selectedSize = 0;

        private GUIStyle _iconButtonStyle;
        
        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            _iconNames = GetAppropriateIconNames(GetEditorAssetBundle(), GetIconPath());
            
            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton);
            _iconButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            _iconButtonStyle.fixedHeight = 0;

            if (_iconNames == null || _iconNames.Count == 0)
            {
                DisplayDialogue(ErrorTitle, NoIconsFoundLabel, false);
                Close();
            }
            
            SortIconsBySizes();
            Debug.Log($"Number of Small Icons {_smallIcons.Count}");
            Debug.Log($"Number of Medium Icons {_mediumIcons.Count}");
            Debug.Log($"Number of Large Icons {_largeIcons.Count}");
        }

        protected override void CreateGUI(string controlName)
        {
            CreateToolbar(controlName);

            _currentSelectionOfIcons = GetSizeAppropriateIcons(_selectedSize);

            using (var scrollScope = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;
                float pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
                GUILayout.Space(10f);
                
                // foreach (string iconName in _iconNames)
                // {
                //     GUIContent content = GetIconContent(iconName);
                //
                //     if (content == null || content.image == null)
                //     {
                //         continue;
                //     }
                //
                //     if (GUILayout.Button(content.image))
                //     {
                //         Debug.Log(iconName);
                //     }
                // }

                float renderWidth = Screen.width / pixelsPerPoint - ScrollBarWidth;
                int gridWidth = Mathf.FloorToInt(renderWidth / _buttonSize);
                float marginPadding = (renderWidth - _buttonSize * gridWidth) / 2;

                int currentRow = 0;
                int iconIndex = 0;
                int totalIconCount = _currentSelectionOfIcons.Count;

                while (iconIndex < totalIconCount)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(marginPadding);

                        for (int i = 0; i < gridWidth; i++)
                        {
                            int currentIconIndex = i + currentRow * gridWidth;
                            GUIContent currentIcon = _currentSelectionOfIcons[currentIconIndex];

                            if (GUILayout.Button(currentIcon, _iconButtonStyle, GUILayout.Width(_buttonSize),
                                GUILayout.Height(_buttonSize)))
                            {
                                _currentlySelectedIcon = currentIcon;
                            }

                            iconIndex++;

                            if (iconIndex == totalIconCount)
                            {
                                break;
                            }
                        }
                    }

                    currentRow++;
                }
                
                GUILayout.Space(10f);

                // foreach (GUIContent icon in _currentSelectionOfIcons)
                // {
                //     if (icon == null || icon.image == null)
                //     {
                //         continue;
                //     }
                //
                //     using (new GUILayout.HorizontalScope())
                //     {
                //         if (GUILayout.Button(icon.image, EditorStyles.miniButtonMid, GUILayout.Width(icon.image.width + 2f), GUILayout.Height(icon.image.height)))
                //         {
                //             _currentlySelectedIcon = icon;
                //         }
                //     }
                // }
            }

            if (_currentlySelectedIcon.Equals(GUIContent.none))
            {
                return;
            }

            GUILayout.FlexibleSpace();
        }

        #endregion

        #region Custom Methods

        private void CreateToolbar(string controlName)
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(IconSizesLabel, EditorStyles.boldLabel, GUILayout.Width(IconSizeLabelWidth));
                _selectedSize = (IconSize)GUILayout.SelectionGrid((int)_selectedSize, IconSizes, IconSizes.Length, EditorStyles.toolbarButton, GUILayout.Width(IconSizesWidth));
                GUI.SetNextControlName(controlName);
                _searchField = GUILayout.TextField(_searchField, EditorStyles.toolbarSearchField);
                if (GUILayout.Button(EditorGUIUtility.IconContent(ClearIcon), EditorStyles.toolbarButton, GUILayout.Width(ClearButtonWidth)))
                {
                    _searchField = string.Empty;
                }
            }
        }

        private AssetBundle GetEditorAssetBundle()
        {
            MethodInfo editorAssetBundle = typeof(EditorGUIUtility).GetMethod(EditorAssetBundleMethod, BindingFlags.NonPublic | BindingFlags.Static);
            return (AssetBundle) editorAssetBundle.Invoke(null, new object[] { });
        }

        private string GetIconPath()
        {
#if UNITY_2018_3_OR_NEWER
            return UnityEditor.Experimental.EditorResources.iconsPath;
#else
            var assembly = typeof(EditorGUIUtility).Assembly;
            var resourceUtility = assembly.GetType(EditorResourceUtility);
            var iconsPathProperty = resourceUtility.GetProperty(IconsPath, BindingFlags.Static | BindingFlags.Public);
            return (string)iconsPathProperty.GetValue(null, new object[] { });
#endif
        }

        private List<string> GetAppropriateIconNames(AssetBundle bundle, string path)
        {
            if (bundle == null || string.IsNullOrEmpty(path))
            {
                return null;
            }

            string[] assetNames = bundle.GetAllAssetNames();

            if (assetNames == null)
            {
                return null;
            }
            
            List<string> appropriateIconNames = new List<string>();

            foreach (string assetName in assetNames)
            {
                if (!assetName.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!assetName.EndsWith(PngFileExtension, StringComparison.OrdinalIgnoreCase) &&
                    !assetName.EndsWith(AssetFileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                appropriateIconNames.Add(assetName);
            }

            return appropriateIconNames;
        }

        private void SortIconsBySizes()
        {
            foreach (string iconName in _iconNames)
            {
                GUIContent iconContent = GetIconContent(iconName);

                if (iconContent == null)
                {
                    //TODO: Add Debug message
                    continue;
                }

                Texture icon = iconContent.image;

                if (icon == null)
                {
                    continue;
                }

                if (icon.width <= SmallToMediumLimit || icon.height <= SmallToMediumLimit)
                {
                    _smallIcons.Add(iconContent);
                }
                else
                {
                    _mediumIcons.Add(iconContent);
                }
            }
        }

        private GUIContent GetIconContent(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
            {
                return null;
            } 
            
            return EditorGUIUtility.IconContent(iconName);
        }

        private List<GUIContent> GetSizeAppropriateIcons(IconSize size)
        {
            switch (size)
            {
                case IconSize.Small:
                    _buttonSize = SmallButtonSize;
                    return _smallIcons;
                case IconSize.Medium:
                    _buttonSize = MediumButtonSize;
                    return _mediumIcons;
                case IconSize.Large:
                    _buttonSize = LargeButtonSize;
                    return _largeIcons;
                default:
                    Debug.LogWarning("Inappropriate Icon Size selected");
                    _buttonSize = SmallButtonSize;
                    return _smallIcons;
            }
        }

        #endregion
    }

}