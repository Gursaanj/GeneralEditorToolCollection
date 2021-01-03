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

        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            foreach (string assetName in GetAppropriateIconNames(GetIconAssetBundle(), GetIconPath()))
            {
                Debug.Log(assetName);
            }
        }

        protected override void CreateGUI(string controlName)
        {
            
        }

        #endregion

        #region Custom Methods

        private void CreateGUIStyles()
        {
            
        }

        private AssetBundle GetIconAssetBundle()
        {
            MethodInfo editorAssetBundle = typeof(EditorGUIUtility).GetMethod("GetEditorAssetBundle", BindingFlags.NonPublic | BindingFlags.Static);
            return (AssetBundle) editorAssetBundle.Invoke(null, new object[] { });
        }

        private string GetIconPath()
        {
#if UNITY_2018_3_OR_NEWER
            return UnityEditor.Experimental.EditorResources.iconsPath;
#else
            var assembly = typeOf(EditorGUIUtility).Assembly;
            var resourceUtility = assembly.GetType("UnityEditorInternal.EditorResourcesUtility");
            var iconsPathProperty = resourceUtility.GetProperty("iconsPath", BindingFlags.Static | BindingFlags.Public);
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

                if (!assetName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                    !assetName.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                appropriateIconNames.Add(assetName);
            }

            return appropriateIconNames;
        }

        #endregion
    }

}