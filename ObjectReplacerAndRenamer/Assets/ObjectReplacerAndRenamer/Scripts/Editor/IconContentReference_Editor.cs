using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

    public class IconContentReference_Editor : GuiControlEditorWindow, IHasCustomMenu
    {
        #region Variables
        
        //GUI Labels
        private const string IconSizesLabel = "Filter icons by size";
        private const string ClearIcon = "winbtn_win_close";
        private const string IconNameLabel = "Name of Icon Content";
        private const string IconFullMethod = "Full Method";

        private const string CopyIcon = "winbtn_win_restore@2x";
        private const string CopyIconTooltip = "Copy to clipboard";
        private const string YesLabel = "Yes";
        private const string NoLabel = "No";
        private const string LightThemeLabel = "Light";
        private const string DarkThemeLabel = "Dark";

        private const string DownloadAllOfSizeLabel = "Download the presented {0} sized Icons";
        private const string DownloadProgressTitle = "Downloading presented {0} sized Icons";
        private const string DownloadLabel = "Download Image";
        private const string DownloadCountMessage = "Downloading {0} Icons";
        private const string DownloadingMessage = "Downloading {0}";
        
        private const float IconSizeLabelWidth = 120f;
        private const float IconSizesWidth = 180f;
        private const float PreviewSectionMaxHeight = 130f;
        private const float ClearButtonWidth = 20f;

        private const float PrimaryPadding = 10f;
        private const float TextureHeight = 115f;
        private const float TextureWidthRatio = 0.4f;
        private const float TextureBorderOffset = 2f;
        private const float PreviewWidthPadding = 30f;
        private const float PreviewHeightPadding = 5f;
        private const float PreviewLabelVerticalOffset = 3f;
        private const float DownloadButtonOffset = 15f;
        private const float ScrollBarWidth = 13f;
        
        //Warning Labels
        private const string NoIconsFoundWarning = "No Icons Found!!";
        private const string IconDownloadedMessage = "{0} has been downloaded";
        private const string NoIconsError = "No Icons to download";
        private const string CantDownloadIconError = "Icon number {0} can't be downloaded";

        private const string InappropriateSizeWarning = "Inappropriate Icon Size selected";
        private const string IconAlreadyExistsMessage = "{0} Already exists, not downloading";
        
        private const string EditorAssetBundleMethod = "GetEditorAssetBundle";
        private const string EditorResourceUtility = "UnityEditorInternal.EditorResourcesUtility";
        private const string IconsPath = "iconsPath";

        private const string PngFileExtension = ".png";
        private const string AssetFileExtension = ".asset";
        private const string ProOnlyIconIdentifier = "d_";
        private const string MainDirectory = "Assets/UnityInternal Icons";

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
        private bool _isLightBackdrop = false;

        //GUI Fields
        private string _searchField = string.Empty;
        private IconSize _selectedSize = IconSize.Small;

        private GUIStyle _iconButtonStyle;
        private GUIStyle _blackPreviewStyle;
        private GUIStyle _whitePreviewStyle;
        private GUIStyle _previewLabel;
        private GUIContent _copyContent;
        
        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            _iconNames = GetAppropriateIconNames(GetEditorAssetBundle(), GetIconPath());

            CreateGUIStyles();

            if (_iconNames == null || _iconNames.Count == 0)
            {
                DisplayDialogue(ErrorTitle, NoIconsFoundWarning, false);
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

            if (!string.IsNullOrEmpty(_searchField))
            {
                _currentSelectionOfIcons = _currentSelectionOfIcons
                    .Where(icon => icon.tooltip.ToLower().Contains(_searchField.ToLower())).ToList();
            }
            
            using (var scrollScope = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;
                float pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
                GUILayout.Space(PrimaryPadding);

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
                
                GUILayout.Space(PrimaryPadding);
                
            }

            if (_currentlySelectedIcon.Equals(GUIContent.none))
            {
                return;
            }

            GUILayout.FlexibleSpace();
            float textureWidth = position.width / TextureWidthRatio;
            float previewStyleWidth = textureWidth / 2;
            float previewWidth = position.width - textureWidth - PreviewWidthPadding;

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox,GUILayout.MaxHeight(PreviewSectionMaxHeight)))
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(textureWidth)))
                {
                    GUILayout.Space(TextureBorderOffset);

                    GUILayout.Button(_currentlySelectedIcon, _isLightBackdrop ? _whitePreviewStyle : _blackPreviewStyle, GUILayout.Width(textureWidth - TextureBorderOffset),
                        GUILayout.Height(TextureHeight));
                    
                    GUILayout.FlexibleSpace();

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(LightThemeLabel, EditorStyles.miniButton, GUILayout.Width(previewStyleWidth)))
                        {
                            _isLightBackdrop = true;
                        }
                        
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(DarkThemeLabel, EditorStyles.miniButton, GUILayout.Width(previewStyleWidth)))
                        {
                            _isLightBackdrop = false;
                        }
                    }
                }

                GUILayout.Space(PrimaryPadding);

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(PreviewHeightPadding);
                    using (new GUILayout.HorizontalScope(GUILayout.Width(previewWidth)))
                    {
                        StringBuilder info = new StringBuilder();
                        info.AppendLine($"Width : {_currentlySelectedIcon.image.width} Height : {_currentlySelectedIcon.image.height}");
                        string proSkinLabel = IsIconProOnly(_currentlySelectedIcon.tooltip) ? YesLabel : NoLabel;
                        info.Append($"Is ProSkin Icon? {proSkinLabel}");
                        EditorGUILayout.HelpBox(info.ToString(), MessageType.None);
                        GUILayout.Space(DownloadButtonOffset);
                        if (GUILayout.Button(DownloadLabel))
                        {
                            DownloadIcon(_currentlySelectedIcon, true);
                        }
                    }

                    GUILayout.Space(PreviewHeightPadding);
                    CreatePreviewLabel(previewWidth,IconNameLabel, $"\"{_currentlySelectedIcon.tooltip}\"");
                    GUILayout.Space(PreviewHeightPadding);
                    CreatePreviewLabel(previewWidth,IconFullMethod, $"EditorGUIUtility.IconContent(\"{_currentlySelectedIcon.tooltip}\")");
                    GUILayout.FlexibleSpace();
                }
            }

            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
            {
                _currentlySelectedIcon = GUIContent.none;
            }
        }

        #endregion

        #region IHasCustomMenu Implementation

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(string.Format(DownloadAllOfSizeLabel, _selectedSize.ToString())),false, DownloadAllIconsOfSameSize);
        }

        #endregion

        #region Custom Methods

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
            
            _previewLabel = new GUIStyle(EditorStyles.boldLabel);
            _previewLabel.padding = new RectOffset(0,0,0,-5);
        }

        private void SetPreviewBackgrounds(ref GUIStyle style, Texture2D backgroundTexture)
        {
            style.hover.background = style.onHover.background = style.focused.background = style.active.background =
                style.onActive.background = style.normal.background = style.onNormal.background = backgroundTexture;

            style.hover.scaledBackgrounds = style.onHover.scaledBackgrounds = style.focused.scaledBackgrounds =
                style.active.scaledBackgrounds = style.onActive.scaledBackgrounds = style.normal.scaledBackgrounds =
                    style.onNormal.scaledBackgrounds = new Texture2D[] {backgroundTexture};
        }

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

        private void CreatePreviewLabel(float layoutWidth, string label, string content)
        {
            using (new GUILayout.HorizontalScope(GUILayout.Width(layoutWidth)))
            {
                GUILayout.Label(label, _previewLabel);
                GUILayout.Space(PreviewLabelVerticalOffset);
                if (GUILayout.Button(_copyContent, EditorStyles.miniButtonRight, GUILayout.Width(20f)))
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

                iconContent.tooltip = icon.name;

                if (icon.width <= SmallToMediumLimit || icon.height <= SmallToMediumLimit)
                {
                    _smallIcons.Add(iconContent);
                }
                else if (icon.width <= MediumToLargeLimit || icon.height <= MediumToLargeLimit)
                {
                    _mediumIcons.Add(iconContent);
                }
                else
                {
                    _largeIcons.Add(iconContent);
                }
            }
        }

        private GUIContent GetIconContent(string iconName)
        {
            return string.IsNullOrEmpty(iconName) ? null : EditorGUIUtility.IconContent(iconName);
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
                    Debug.LogWarning(InappropriateSizeWarning);
                    _buttonSize = SmallButtonSize;
                    return _smallIcons;
            }
        }

        private bool IsIconProOnly(string iconName)
        {
            return iconName.IndexOf(ProOnlyIconIdentifier, StringComparison.Ordinal) == 0;
        }

        private void DownloadAllIconsOfSameSize()
        {
            int totalCount = _currentSelectionOfIcons.Count;
            
            if (totalCount == 0)
            {
                DisplayDialogue(ErrorTitle, NoIconsError, false);
                return;
            }

            string progressTitle = string.Format(DownloadProgressTitle, _selectedSize.ToString());

            EditorUtility.DisplayProgressBar(progressTitle, string.Format(DownloadCountMessage, totalCount), 0.0f);

            for (int i = 0; i < totalCount; i++)
            {
                GUIContent content = _currentSelectionOfIcons[i];

                if (content.Equals(GUIContent.none) || content.image == null || string.IsNullOrEmpty(content.tooltip))
                {
                    Debug.LogError(string.Format(CantDownloadIconError, i));
                    continue;
                }

                if (EditorUtility.DisplayCancelableProgressBar(progressTitle,
                    string.Format(DownloadingMessage, content.tooltip), (float) i / totalCount))
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                
                DownloadIcon(content);
            }
            
            EditorUtility.ClearProgressBar();
        }

        private void DownloadIcon(GUIContent iconContent, bool displayConfirmation = false)
        {
            string iconName = iconContent.tooltip;
            
            string folderPath = $"{MainDirectory}/{_selectedSize.ToString()}";
            Directory.CreateDirectory(folderPath);
            string completePath = Path.Combine(folderPath, $"{iconName}{PngFileExtension}");

            if (File.Exists(completePath))
            {
                Debug.Log(string.Format(IconAlreadyExistsMessage, iconName));
                return;
            }
            
            Texture2D icon = (Texture2D)iconContent.image;
            Texture2D texture = new Texture2D(icon.width, icon.height, icon.format, icon.mipmapCount > 1);
            Graphics.CopyTexture(icon, texture);
            
            File.WriteAllBytes(completePath, texture.EncodeToPNG());
            
            AssetDatabase.Refresh();

            if (displayConfirmation)
            {
                DisplayDialogue(UpdateTitle, string.Format(IconDownloadedMessage, iconName), false);
            }
        }

        #endregion
    }

}