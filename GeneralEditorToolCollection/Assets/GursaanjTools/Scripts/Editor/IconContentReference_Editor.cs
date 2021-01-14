using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private const string IconNameLabel = "Name of Icon Content";
        private const string IconFullMethod = "Full Method";
        
        private const string YesLabel = "Yes";
        private const string NoLabel = "No";
        private const string DownloadAllOfSizeLabel = "Download the presented {0} sized Icons";

        private const float IconSizeLabelWidth = 120f;
        private const float IconSizesWidth = 180f;
        private const float PreviewSectionMaxHeight = 130f;
        private const float ClearButtonWidth = 20f;
        private const float CopyButtonWidth = 20f;

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

        private const string InappropriateSizeWarning = "Inappropriate Icon Size selected";

        private const string EditorResourceUtility = "UnityEditorInternal.EditorResourcesUtility";
        private const string IconsPath = "iconsPath";

        private const string PngFileExtension = ".png";
        private const string AssetFileExtension = ".asset";
        private const string SubDirectory = "Icons";

        private const int SmallToMediumLimit = 36;
        private const int MediumToLargeLimit = 72;

        private const float SmallButtonSize = 40f;
        private const float MediumButtonSize = 70f;
        private const float LargeButtonSize = 100f;

        private readonly string[] IconSizes = Enum.GetNames(typeof(IconSize));

        private EditorResourceLogic _logic;

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
        private IconSize _selectedSize = IconSize.Small;
        
        
        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            _logic = new EditorResourceLogic();
            string[] iconExtensions = new string[2] {PngFileExtension, AssetFileExtension};
            _iconNames = _logic.GetAppropriateNames(_logic.GetEditorAssetBundle(), GetIconPath(), iconExtensions);
            
            if (_iconNames == null || _iconNames.Count == 0)
            {
                DisplayDialogue(ErrorTitle, NoIconsFoundWarning, false);
                Close();
            }
            
            SortIconsBySizes();
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

                            if (GUILayout.Button(currentIcon, _logic.IconButtonStyle, GUILayout.Width(_buttonSize),
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
            float textureWidth = position.width * TextureWidthRatio;
            float previewStyleWidth = textureWidth / 2;
            float previewWidth = position.width - textureWidth - PreviewWidthPadding;

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox,GUILayout.MaxHeight(PreviewSectionMaxHeight)))
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(textureWidth)))
                {
                    GUILayout.Space(TextureBorderOffset);

                    GUILayout.Button(_currentlySelectedIcon, _logic.IsLightPreview ? _logic.WhitePreviewStyle : _logic.BlackPreviewStyle, GUILayout.Width(textureWidth - TextureBorderOffset),
                        GUILayout.Height(TextureHeight));
                    
                    GUILayout.FlexibleSpace();

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(_logic.LightThemeLabel, EditorStyles.miniButton, GUILayout.Width(previewStyleWidth)))
                        {
                            _logic.IsLightPreview = true;
                        }
                        
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(_logic.DarkThemeLabel, EditorStyles.miniButton, GUILayout.Width(previewStyleWidth)))
                        {
                            _logic.IsLightPreview = false;
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
                        string proSkinLabel = _logic.IsProOnly(_currentlySelectedIcon.tooltip) ? YesLabel : NoLabel;
                        info.Append($"Is ProSkin Icon? {proSkinLabel}");
                        EditorGUILayout.HelpBox(info.ToString(), MessageType.None);
                        GUILayout.Space(DownloadButtonOffset);
                        if (GUILayout.Button(_logic.DownloadLabel))
                        {
                            if (_logic.DownloadImageContent(_currentlySelectedIcon, $"{SubDirectory}/{_selectedSize.ToString()}"))
                            {
                                DisplayDialogue(UpdateTitle, string.Format(_logic.DownloadMessageLabel, _currentlySelectedIcon.tooltip), false);
                            }
                        }
                    }

                    GUILayout.Space(PreviewHeightPadding);
                    CreatePreviewLabel(previewWidth,IconNameLabel, $"\"{_currentlySelectedIcon.tooltip}\"");
                    GUILayout.Space(PreviewHeightPadding);
                    CreatePreviewLabel(previewWidth,IconFullMethod, $"EditorGUIUtility.IconContent(\"{_currentlySelectedIcon.tooltip}\")");
                    GUILayout.FlexibleSpace();
                }
            }

            _logic.HandleContentEvents(ref _currentlySelectedIcon);
        }

        #endregion

        #region IHasCustomMenu Implementation

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(string.Format(DownloadAllOfSizeLabel, _selectedSize.ToString())),false, _logic.DownloadSelectionOfImages, new ContentInformation(_currentSelectionOfIcons, $"{SubDirectory}/{_selectedSize.ToString()}"));
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
                if (GUILayout.Button(_logic.ClearSearch, EditorStyles.toolbarButton, GUILayout.Width(ClearButtonWidth)))
                {
                    _searchField = string.Empty;
                }
            }
        }

        private void CreatePreviewLabel(float layoutWidth, string label, string content)
        {
            using (new GUILayout.HorizontalScope(GUILayout.Width(layoutWidth)))
            {
                GUILayout.Label(label, _logic.PreviewStyle);
                GUILayout.Space(PreviewLabelVerticalOffset);
                if (GUILayout.Button(_logic.CopyContent, EditorStyles.miniButtonRight, GUILayout.Width(CopyButtonWidth)))
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

        private void SortIconsBySizes()
        {
            foreach (string iconName in _iconNames)
            {
                GUIContent iconContent = GetIconContent(iconName);

                if (iconContent == null || iconContent.Equals(GUIContent.none))
                {
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
        
        #endregion
    }

}