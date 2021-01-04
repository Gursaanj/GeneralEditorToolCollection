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
        
        //Warning Labels
        private const string NoIconsFoundLabel = "No Icons Found!!";


        private const string EditorAssetBundleMethod = "GetEditorAssetBundle";
        private const string EditorResourceUtility = "UnityEditorInternal.EditorResourcesUtility";
        private const string IconsPath = "iconsPath";

        private const string IconSizesLabel = "Filter icons By size";
        private const string ClearIcon = "winbtn_win_close";
        private const int IconSizeLabelWidth = 120;
        private const int IconSizesWidth = 180;
        private const int ClearButtonWidth = 20;
        
        private const string PngFileExtension = ".png";
        private const string AssetFileExtension = ".asset";

        private const int SmallToMediumLimit = 36;
        private const int MediumToLargeLimit = 72;

        private readonly string[] IconSizes = Enum.GetNames(typeof(IconSize));
        
        private List<string> _iconNames = new List<string>();
        private List<GUIContent> _smallIcons = new List<GUIContent>();
        private List<GUIContent> _mediumIcons = new List<GUIContent>();
        private List<GUIContent> _largeIcons = new List<GUIContent>();
        private List<GUIContent> _currentlySelectedIcons = new List<GUIContent>();
        private Vector2 _scrollPosition = Vector2.zero;
        
        //GUI Fields
        private string _searchField = string.Empty;
        private IconSize _selectedSize = 0;
        
        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            _iconNames = GetAppropriateIconNames(GetEditorAssetBundle(), GetIconPath());

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

            _currentlySelectedIcons = GetSizeAppropriateIcons(_selectedSize);

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

                foreach (GUIContent icon in _currentlySelectedIcons)
                {
                    if (icon == null || icon.image == null)
                    {
                        continue;
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Box(icon);
                    }
                }
            }
        }

        #endregion

        #region Custom Methods
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
                    return _smallIcons;
                case IconSize.Medium:
                    return _mediumIcons;
                case IconSize.Large:
                    return _largeIcons;
                default:
                    Debug.LogWarning("Inappropriate Icon Size selected");
                    return _smallIcons;
            }
        }

        #endregion
    }

}