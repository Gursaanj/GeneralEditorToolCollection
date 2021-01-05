using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class FontReferences_Editor : GuiControlEditorWindow
    {
        #region Variables

        //GUI Labels
        
        //Warning Labels
        
        //Undo Labels
        
        private const string EditorAssetBundleMethod = "GetEditorAssetBundle";
        private const string EditorResourceUtility = "UnityEditorInternal.EditorResourcesUtility";
        private const string FontsPath = "fontsPath";

        private const string FontsExtension = ".ttf";
        
        private List<string> _fontNames = new List<string>();
        
        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            _fontNames = GetAppropriateFontNames(GetEditorAssetBundle(), GetFontPath());

            if (_fontNames == null || _fontNames.Count == 0)
            {
                Debug.LogError("No Fonts");
                _window.Close();
            }

            foreach (string fontName in _fontNames)
            {
                Debug.Log(fontName);
            }
        }

        protected override void CreateGUI(string controlName)
        {
            
        }

        #endregion

        #region IHasCustomMenu Implementation

        

        #endregion

        #region Custom Methods

        private AssetBundle GetEditorAssetBundle()
        {
            MethodInfo editorAssetBundle = typeof(EditorGUIUtility).GetMethod(EditorAssetBundleMethod, BindingFlags.NonPublic | BindingFlags.Static);
            return editorAssetBundle == null ? null : (AssetBundle) editorAssetBundle.Invoke(null, new object[] { });
        }
        
        private string GetFontPath()
        {
#if UNITY_2018_3_OR_NEWER
            return UnityEditor.Experimental.EditorResources.fontsPath;
#else
            var assembly = typeof(EditorGUIUtility).Assembly;
            var resourceUtility = assembly.GetType(EditorResourceUtility);
            var fontsPathProperty = resourceUtility.GetProperty(FontsPath, BindingFlags.Static | BindingFlags.Public);
            return (string)fontsPathProperty.GetValue(null, new object[] { });
#endif
        }

        private List<string> GetAppropriateFontNames(AssetBundle bundle, string path)
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
            
            List<string> fontNames = new List<string>();

            foreach (string assetName in assetNames)
            {
                if (!assetName.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!assetName.EndsWith(FontsExtension, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                fontNames.Add(assetName);
            }

            return fontNames;
        }

        #endregion
    }
}
