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

    public class IconContentReference_Editor : GuiControlEditorWindow
    {
        #region Variables
        
        //GUI Labels
        private const string IconSizesLabel = "Filter icons by size";
        private const string ClearIcon = "winbtn_win_close";

        private const string CopyIcon = "winbtn_win_restore@2x";
        private const string CopyIconTooltip = "Copy to clipboard";
        private const string YesLabel = "Yes";
        private const string NoLabel = "No";

        private const string DownloadLabel = "Download Image";
        
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
        private const string ProOnlyIconIdentifier = "d_";

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
        private GUIStyle _previewStyle;
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

            if (!string.IsNullOrEmpty(_searchField))
            {
                _currentSelectionOfIcons = _currentSelectionOfIcons
                    .Where(icon => icon.tooltip.ToLower().Contains(_searchField.ToLower())).ToList();
            }
            
            using (var scrollScope = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;
                float pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
                GUILayout.Space(10f);

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
                
            }

            if (_currentlySelectedIcon.Equals(GUIContent.none))
            {
                return;
            }

            GUILayout.FlexibleSpace();
            float textureWidth = position.width / 2.5f;
            float previewWidth = position.width - textureWidth - 30f;

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox,GUILayout.MaxHeight(130)))
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(textureWidth)))
                {
                    GUILayout.Space(2f);

                    GUILayout.Button(_currentlySelectedIcon, _previewStyle, GUILayout.Width(textureWidth - 2f),
                        GUILayout.Height(128));
                    
                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(10f);

                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(5f);
                    using (new GUILayout.HorizontalScope(GUILayout.Width(previewWidth)))
                    {
                        StringBuilder info = new StringBuilder();
                        info.AppendLine($"Size: {_currentlySelectedIcon.image.width} X {_currentlySelectedIcon.image.height}");
                        string proSkinLabel = IsIconProOnly(_currentlySelectedIcon.tooltip) ? YesLabel : NoLabel;
                        info.Append($"Is ProSkin Icon? {proSkinLabel}");
                        EditorGUILayout.HelpBox(info.ToString(), MessageType.None);
                        GUILayout.Space(15f);
                        if (GUILayout.Button(DownloadLabel))
                        {
                            //Debug.Log("Add Download Functionality");
                            DownloadIcon(_currentlySelectedIcon);
                        }
                    }

                    GUILayout.Space(5f);
                    CreatePreviewLabel(previewWidth,"Name of Icon Content", $"\"{_currentlySelectedIcon.tooltip}\"");
                    GUILayout.Space(5f);
                    CreatePreviewLabel(previewWidth,"Full Method", $"EditorGUIUtility.IconContent(\"{_currentlySelectedIcon.tooltip}\")");
                    GUILayout.FlexibleSpace();
                }
            }

            if (Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
            {
                _currentlySelectedIcon = GUIContent.none;
            }
        }

        #endregion

        #region Custom Methods

        private void CreateGUIStyles()
        {
            _iconButtonStyle = new GUIStyle(EditorStyles.miniButton);
            _iconButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            _iconButtonStyle.fixedHeight = 0;
            
            Texture2D backgroundTexture = new Texture2D(1,1);
            backgroundTexture.SetPixel(0,0,new Color(0.15f,0.15f,0.15f));
            backgroundTexture.Apply();

            _previewStyle = new GUIStyle(_iconButtonStyle);
            
            _previewStyle.hover.background = _previewStyle.onHover.background = _previewStyle.focused.background =
                _previewStyle.active.background = _previewStyle.onActive.background = _previewStyle.normal.background =
                    _previewStyle.onNormal.background = backgroundTexture;

            _previewStyle.hover.scaledBackgrounds = _previewStyle.onHover.scaledBackgrounds =
                _previewStyle.focused.scaledBackgrounds = _previewStyle.active.scaledBackgrounds =
                    _previewStyle.onActive.scaledBackgrounds = _previewStyle.normal.scaledBackgrounds =
                        _previewStyle.onNormal.scaledBackgrounds = new Texture2D[] {backgroundTexture};
            
            _copyContent = EditorGUIUtility.IconContent(CopyIcon);
            _copyContent.tooltip = CopyIconTooltip;
            
            _previewLabel = new GUIStyle(EditorStyles.boldLabel);
            _previewLabel.padding = new RectOffset(0,0,0,-5);

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
                GUILayout.Space(3f);
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

        private bool IsIconProOnly(string name)
        {
            return name.IndexOf(ProOnlyIconIdentifier, StringComparison.Ordinal) == 0;
        }

        private void DownloadIcon(GUIContent iconContent)
        {
            Texture2D icon = (Texture2D)iconContent.image;
            string iconName = iconContent.tooltip;

            if (string.IsNullOrEmpty(iconName))
            {
                DisplayDialogue(ErrorTitle, "Unable to Download: No associated Name", false);
                return;
            }

            if (icon == null)
            {
                DisplayDialogue(ErrorTitle, "Unable to Download: No image to load", false);
                return;
            }
            
            Texture2D texture = new Texture2D(icon.width, icon.height, icon.format, icon.mipmapCount > 1);
            Graphics.CopyTexture(icon, texture);
            
            string folderPath = $"Assets/UnityInternal Icons/{_selectedSize.ToString()}";
            Directory.CreateDirectory(folderPath);
            
            File.WriteAllBytes(Path.Combine(folderPath, $"{iconName}{PngFileExtension}"), texture.EncodeToPNG());
            
            AssetDatabase.Refresh();
        }

        #endregion
    }

}