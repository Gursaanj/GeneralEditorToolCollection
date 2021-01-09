using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class EditorResourceLogic
    {
        #region Variables
        
        private const string CopyIcon = "winbtn_win_restore@2x";
        private const string CopyIconTooltip = "Copy to clipboard";
        private const string ClearIcon = "winbtn_win_close";
        private const string ClearSearchTooltip = "Clear Search";
        private const string YesLabel = "Yes";
        private const string NoLabel = "No";
        private const string LightThemeLabel = "Light";
        private const string DarkThemeLabel = "Dark";
        private const string DownloadLabel = "Download Image";
        
        private const float CopyButtonWidth = 20f;
        private const float DownloadButtonOffset = 15f;

        private const float _ScrollBarWidth = 13f;
        
        private const string DownloadedMessage = "{0} has been downloaded";
        private const string ImageAlreadyExistsMessage = "{0} Already exists within designated folder, unable to download";
        
        private const string EditorAssetBundleMethod = "GetEditorAssetBundle";
        private const string EditorResourceUtility = "UnityEditorInternal.EditorResourcesUtility";
        private const string ProOnlyIconIdentifier = "d_";
        private const string MainDirectory = "Assets/UnityEditorResources";
        
        private const string PngFileExtension = ".png";

        private GUIStyle _iconButtonStyle;
        private GUIStyle _blackPreviewStyle;
        private GUIStyle _whitePreviewStyle;
        private GUIContent _copyContent;
        private GUIContent _clearSearch;

        #endregion

        #region Properties

        public GUIStyle IconButtonStyle => _iconButtonStyle;

        public bool IsLightPreview { get; set; } = false;

        public GUIStyle BlackPreviewStyle => _blackPreviewStyle;

        public GUIStyle WhitePreviewStyle => _whitePreviewStyle;
        public string BlackPreviewLabel => DarkThemeLabel;
        public string WhitePreviewLabel => LightThemeLabel;

        public GUIContent CopyContent => _copyContent;

        public GUIContent ClearSearch => _clearSearch;

        public string PNGFileExtension => PngFileExtension;

        public float ScrollBarWidth => _ScrollBarWidth;

        #endregion

        #region Constructor

        public EditorResourceLogic()
        {
            CreateGUIStyles();
        }

        #endregion
    
        #region Custom Methods
        
        public bool IsProOnly(string nameInQuestion)
        {
            return nameInQuestion.IndexOf(ProOnlyIconIdentifier, StringComparison.Ordinal) == 0;
        }
        
        public AssetBundle GetEditorAssetBundle()
        {
            MethodInfo editorAssetBundle = typeof(EditorGUIUtility).GetMethod(EditorAssetBundleMethod, BindingFlags.NonPublic | BindingFlags.Static);
            return editorAssetBundle == null ? null : (AssetBundle) editorAssetBundle.Invoke(null, new object[] { });
        }
        public GUIContent GetImageContent(string imageName)
        {
            return string.IsNullOrEmpty(imageName) ? null : EditorGUIUtility.IconContent(imageName);
        }
        
        public List<string> GetAppropriateNames(AssetBundle bundle, string path, string[] extensions)
        {
            if (bundle == null || string.IsNullOrEmpty(path) || extensions == null)
            {
                return null;
            }

            string[] assetNames = bundle.GetAllAssetNames();

            if (assetNames == null)
            {
                return null;
            }
            
            List<string> appropriateNames = new List<string>();

            foreach (string assetName in assetNames)
            {
                if (string.IsNullOrEmpty(assetName) || !assetName.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                foreach (string extension in extensions)
                {
                    if (string.IsNullOrEmpty(extension) || !assetName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    
                    appropriateNames.Add(assetName);
                    break;
                }
            }

            return appropriateNames;
        }
        
        public void DownloadImageContent(GUIContent content, string subDirectory, bool displayConfirmation = false)
        {
            string contentName = content.tooltip;
            
            string folderPath = $"{MainDirectory}/{subDirectory}";
            Directory.CreateDirectory(folderPath);
            string completePath = Path.Combine(folderPath, $"{contentName}{PngFileExtension}");

            if (File.Exists(completePath))
            {
                Debug.Log(string.Format(ImageAlreadyExistsMessage, contentName));
                return;
            }
            
            Texture2D image = (Texture2D)content.image;
            Texture2D texture = new Texture2D(image.width, image.height, image.format, image.mipmapCount > 1);
            Graphics.CopyTexture(image, texture);
            
            File.WriteAllBytes(completePath, texture.EncodeToPNG());
            
            AssetDatabase.Refresh();

            if (displayConfirmation)
            {
                //DisplayDialogue(UpdateTitle, string.Format(DownloadedMessage, contentName), false);
            }
        }
        
        public void CreatePreviewLabel(float layoutWidth, string label, string content, GUIStyle previewLabelStyle = null)
        {
            using (new GUILayout.HorizontalScope(GUILayout.Width(layoutWidth)))
            {
                if (previewLabelStyle == null)
                {
                    GUILayout.Label(label);
                }
                else
                {
                    GUILayout.Label(label, previewLabelStyle);
                }

                GUILayout.Space(3f);
                if (GUILayout.Button(_copyContent, EditorStyles.miniButtonRight, GUILayout.Width(CopyButtonWidth)))
                {
                    EditorGUIUtility.systemCopyBuffer = content;
                }
                GUILayout.FlexibleSpace();
            }
            
            using (new GUILayout.HorizontalScope(GUILayout.Width(layoutWidth)))
            {
                GUILayout.Label(content, GUILayout.MaxWidth(layoutWidth));
            }
        }
        
        
        private void CreateGUIStyles()
        {
            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton);
            _iconButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            _iconButtonStyle.fixedHeight = 0;
            
            Texture2D blackBackground = new Texture2D(1,1);
            blackBackground.SetPixel(0,0,new Color(0.15f,0.15f,0.15f));
            blackBackground.Apply();
            
            Texture2D whiteBackground = new Texture2D(1,1);
            whiteBackground.SetPixel(0,0,new Color(0.85f,0.85f,0.85f));
            whiteBackground.Apply();
            
            _blackPreviewStyle = new GUIStyle(_iconButtonStyle);
            SetPreviewBackgrounds(ref _blackPreviewStyle, blackBackground);

            _whitePreviewStyle = new GUIStyle(_iconButtonStyle);
            SetPreviewBackgrounds(ref _whitePreviewStyle, whiteBackground);
            
            _copyContent = EditorGUIUtility.IconContent(CopyIcon);
            _copyContent.tooltip = CopyIconTooltip;

            _clearSearch = EditorGUIUtility.IconContent(ClearIcon);
            _clearSearch.tooltip = ClearSearchTooltip;
        }
        
        private void SetPreviewBackgrounds(ref GUIStyle style, Texture2D backgroundTexture)
        {
            style.hover.background = style.onHover.background = style.focused.background = style.active.background =
                style.onActive.background = style.normal.background = style.onNormal.background = backgroundTexture;

            style.hover.scaledBackgrounds = style.onHover.scaledBackgrounds = style.focused.scaledBackgrounds =
                style.active.scaledBackgrounds = style.onActive.scaledBackgrounds = style.normal.scaledBackgrounds =
                    style.onNormal.scaledBackgrounds = new Texture2D[] {backgroundTexture};
        }

        #endregion
    }
}
