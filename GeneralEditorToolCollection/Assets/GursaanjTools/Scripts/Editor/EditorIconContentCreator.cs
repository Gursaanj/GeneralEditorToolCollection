using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public enum IconContentSize
    {
        Small,
        Medium,
        Large
    }
    
    public class EditorIconContentCreator
    {
        public readonly struct InternalEditorResourceImageInformation
        {
            public readonly string PrefixPath;
            public readonly string ImageName;
            public readonly string PluralName;
            public readonly string SubDirectory;
            public readonly Action CloseWindow;

            public InternalEditorResourceImageInformation(string prefixPath, string imageName, string pluralName, string subDirectory, Action closeWindow)
            {
                PrefixPath = prefixPath;
                ImageName = imageName;
                PluralName = pluralName;
                SubDirectory = subDirectory;
                CloseWindow = closeWindow;
            }
        }

        #region Variables
        
        //GUI Labels
        private const string IconSizesLabel = "Filter icons by size";
        private const string IconNameLabel = "Name of Icon";
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

        //Warning Labels
        private const string NoIconsFoundWarning = "No Icons Found!!";

        private const string InappropriateSizeWarning = "Inappropriate Size selected";

        private const string PngFileExtension = ".png";
        private const string AssetFileExtension = ".asset";
        private const string SubDirectory = "Icons";

        private const int SmallToMediumLimit = 36;
        private const int MediumToLargeLimit = 72;

        private const float SmallButtonSize = 40f;
        private const float MediumButtonSize = 70f;
        private const float LargeButtonSize = 100f;

        private readonly string[] IconContentSizes = Enum.GetNames(typeof(IconContentSize));

        private InternalEditorResourceImageInformation _information;
        private EditorResourceLogic _logic;

        private List<string> _imageNames = new List<string>();
        private List<GUIContent> _smallIcons = new List<GUIContent>();
        private List<GUIContent> _mediumIcons = new List<GUIContent>();
        private List<GUIContent> _largeIcons = new List<GUIContent>();
        private List<GUIContent> _currentSelection = new List<GUIContent>();
        private GUIContent _currentlySelected = GUIContent.none;
        private Vector2 _scrollPosition = Vector2.zero;
        private float _buttonSize = SmallButtonSize;

        //GUI Fields
        private string _searchField = string.Empty;
        private IconContentSize _selectedContentSize = IconContentSize.Small;
        
        #endregion

        #region Constructor

        public EditorIconContentCreator(InternalEditorResourceImageInformation info)
        {
            _information = info;
            InitializeContent();
        }

        #endregion

        #region Custom Methods

        public void CreateWindowGUI(string controlName, Rect position)
        {
            CreateToolbar(controlName);

            _currentSelection = GetSizeAppropriateIcons(_selectedContentSize);

            if (!string.IsNullOrEmpty(_searchField))
            {
                _currentSelection = _currentSelection
                    .Where(icon => icon.tooltip.ToLower().Contains(_searchField.ToLower())).ToList();
            }
            
            using (var scrollScope = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;
                float pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
                GUILayout.Space(PrimaryPadding);

                float renderWidth = Screen.width / pixelsPerPoint - _logic.ScrollBarWidth;
                int gridWidth = Mathf.FloorToInt(renderWidth / _buttonSize);
                float marginPadding = (renderWidth - _buttonSize * gridWidth) / 2;

                int currentRow = 0;
                int iconIndex = 0;
                int totalIconCount = _currentSelection.Count;

                while (iconIndex < totalIconCount)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(marginPadding);

                        for (int i = 0; i < gridWidth; i++)
                        {
                            int currentIconIndex = i + currentRow * gridWidth;
                            GUIContent currentIcon = _currentSelection[currentIconIndex];

                            if (GUILayout.Button(currentIcon, _logic.IconButtonStyle, GUILayout.Width(_buttonSize),
                                GUILayout.Height(_buttonSize)))
                            {
                                _currentlySelected = currentIcon;
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

            if (_currentlySelected.Equals(GUIContent.none))
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

                    GUILayout.Button(_currentlySelected, _logic.IsLightPreview ? _logic.WhitePreviewStyle : _logic.BlackPreviewStyle, GUILayout.Width(textureWidth - TextureBorderOffset),
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
                        info.AppendLine($"Width : {_currentlySelected.image.width} Height : {_currentlySelected.image.height}");
                        string proSkinLabel = _logic.IsProOnly(_currentlySelected.tooltip) ? YesLabel : NoLabel;
                        info.Append($"Is ProSkin Icon? {proSkinLabel}");
                        EditorGUILayout.HelpBox(info.ToString(), MessageType.None);
                        GUILayout.Space(DownloadButtonOffset);
                        if (GUILayout.Button(_logic.DownloadLabel))
                        {
                            if (_logic.DownloadImageContent(_currentlySelected, $"{SubDirectory}/{_selectedContentSize.ToString()}"))
                            { 
                                EditorUtility.DisplayDialog("Hi", string.Format(_logic.DownloadMessageLabel, _currentlySelected.tooltip), "yup");
                            }
                        }
                    }

                    GUILayout.Space(PreviewHeightPadding);
                    CreatePreviewLabel(previewWidth,IconNameLabel, $"\"{_currentlySelected.tooltip}\"");
                    GUILayout.Space(PreviewHeightPadding);
                    CreatePreviewLabel(previewWidth,IconFullMethod, $"EditorGUIUtility.IconContent(\"{_currentlySelected.tooltip}\")");
                    GUILayout.FlexibleSpace();
                }
            }

            _logic.HandleContentEvents(ref _currentlySelected);
        }


        private void InitializeContent()
        {
            _logic = new EditorResourceLogic();
            string[] iconExtensions = new string[2] {PngFileExtension, AssetFileExtension};
            _imageNames = _logic.GetAppropriateNames(_logic.GetEditorAssetBundle(), _information.PrefixPath, iconExtensions);
            
            if (_imageNames == null || _imageNames.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", NoIconsFoundWarning, "Sounds Good");
                _information.CloseWindow?.Invoke();
            }
            
            SortBySizes();
        }

        private void CreateToolbar(string controlName)
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(IconSizesLabel, EditorStyles.boldLabel, GUILayout.Width(IconSizeLabelWidth));
                _selectedContentSize = (IconContentSize)GUILayout.SelectionGrid((int)_selectedContentSize, IconContentSizes, IconContentSizes.Length, EditorStyles.toolbarButton, GUILayout.Width(IconSizesWidth));
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
        
        private void SortBySizes()
        {
            foreach (string iconName in _imageNames)
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

        private List<GUIContent> GetSizeAppropriateIcons(IconContentSize contentSize)
        {
            switch (contentSize)
            {
                case IconContentSize.Small:
                    _buttonSize = SmallButtonSize;
                    return _smallIcons;
                case IconContentSize.Medium:
                    _buttonSize = MediumButtonSize;
                    return _mediumIcons;
                case IconContentSize.Large:
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
