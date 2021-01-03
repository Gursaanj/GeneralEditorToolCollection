using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    
    public class IconContentReference_Editor : GuiControlEditorWindow
    {
        #region Variables
        
        //GUI Labels
        
        //Warning Labels
        private const string NoIconsFoundLabel = "No Icons Found!!";


        private const string EditorAssetBundleMethod = "GetEditorAssetBundle";
        private const string EditorResourceUtility = "UnityEditorInternal.EditorResourcesUtility";
        private const string IconsPath = "iconsPath";

        private const string PngFileExtension = ".png";
        private const string AssetFileExtension = ".asset";

        private const int SmallToMediumLimit = 36;
        private const int MediumToLargeLimit = 72;
        
        private List<string> _iconNames = new List<string>();
        private List<GUIContent> _smallIcons = new List<GUIContent>();
        private List<GUIContent> _mediumIcons = new List<GUIContent>();
        private List<GUIContent> _largeIcons = new List<GUIContent>();
        private Vector2 _scrollPosition = Vector2.zero;
        
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
            Debug.Log(_smallIcons.Count);
            Debug.Log(_mediumIcons.Count);
        }

        protected override void CreateGUI(string controlName)
        {
            using (var scrollScope = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;

                // foreach (string iconName in _iconNames)
                // {
                //     GUIContent iconContent = EditorGUIUtility.IconContent(iconName);
                //
                //     if (iconContent == null)
                //     {
                //         continue;
                //     }
                //
                //     using (new GUILayout.HorizontalScope(GUILayout.Height(30f)))
                //     {
                //         GUILayout.Label(iconContent);
                //     }
                // }

                foreach (GUIContent smallIcon in _smallIcons)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Box(smallIcon);
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

        #endregion
    }

}