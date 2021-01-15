using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public enum ContentSize
    {
        Small,
        Medium,
        Large
    }
    
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
    
    public class EditorIconContentCreator
    {
        #region Variables
        
        //GUI Labels
        private const string FullMethodLabel = "Full Method";
        
        private const string YesLabel = "Yes";
        private const string NoLabel = "No";

        private const float ContentSizeLabelWidth = 120f;
        private const float ContentSizesWidth = 180f;
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
        private const string InappropriateSizeWarning = "Inappropriate Size selected";

        private const string PngFileExtension = ".png";
        private const string AssetFileExtension = ".asset";

        private const int SmallToMediumLimit = 36;
        private const int MediumToLargeLimit = 72;

        private const float SmallButtonSize = 40f;
        private const float MediumButtonSize = 70f;
        private const float LargeButtonSize = 100f;
        
        //Window Specific Labels
        private readonly string ContentSizeFilterLabel = "Filter {0} by size";
        private readonly string ContentNameLabel = "Name of {0}";
        private readonly string DownloadPresentedContent = "Download the presented {0}";
        private readonly string NoContentFoundWarning = "No {0} Found!!";
        
        private readonly string[] ContentSizes = Enum.GetNames(typeof(ContentSize));

        private readonly InternalEditorResourceImageInformation _information;
        
        private EditorResourceLogic _logic;

        private List<string> contentNames = new List<string>();
        private List<GUIContent> _smallContent = new List<GUIContent>();
        private List<GUIContent> _mediumContent = new List<GUIContent>();
        private List<GUIContent> _largeContent = new List<GUIContent>();
        private List<GUIContent> _currentSelection = new List<GUIContent>();
        private GUIContent _currentlySelected = GUIContent.none;
        private Vector2 _scrollPosition = Vector2.zero;
        private float _buttonSize = SmallButtonSize;

        //GUI Fields
        private string _searchField = string.Empty;
        private ContentSize _selectedContentSize = ContentSize.Small;
        
        #endregion

        #region Constructor

        public EditorIconContentCreator(InternalEditorResourceImageInformation info)
        {
            _information = info;
            ContentSizeFilterLabel = string.Format(ContentSizeFilterLabel, info.PluralName);
            ContentNameLabel = string.Format(ContentNameLabel, info.ImageName);
            DownloadPresentedContent = string.Format(DownloadPresentedContent, info.PluralName);
            NoContentFoundWarning = string.Format(NoContentFoundWarning, info.PluralName);
            InitializeContent();
        }

        #endregion
        
        #region Custom Methods

        public void CreateWindowGUI(string controlName, Rect position)
        {
            CreateToolbar(controlName);

            _currentSelection = GetSizeAppropriateContent(_selectedContentSize);

            if (!string.IsNullOrEmpty(_searchField))
            {
                _currentSelection = _currentSelection
                    .Where(content => content.tooltip.ToLower().Contains(_searchField.ToLower())).ToList();
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
                int contentIndex = 0;
                int totalContentCount = _currentSelection.Count;

                while (contentIndex < totalContentCount)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(marginPadding);

                        for (int i = 0; i < gridWidth; i++)
                        {
                            int currentContentIndex = i + currentRow * gridWidth;
                            GUIContent currentContent = _currentSelection[currentContentIndex];

                            if (GUILayout.Button(currentContent, _logic.IconButtonStyle, GUILayout.Width(_buttonSize),
                                GUILayout.Height(_buttonSize)))
                            {
                                _currentlySelected = currentContent;
                            }

                            contentIndex++;

                            if (contentIndex == totalContentCount)
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
                            if (_logic.DownloadImageContent(_currentlySelected, $"{_information.SubDirectory}/{_selectedContentSize.ToString()}"))
                            {
                                _logic.DisplayUpdate(string.Format(_logic.DownloadMessageLabel, _currentlySelected.tooltip));
                            }
                        }
                    }

                    GUILayout.Space(PreviewHeightPadding);
                    CreatePreviewLabel(previewWidth,ContentNameLabel, $"\"{_currentlySelected.tooltip}\"");
                    GUILayout.Space(PreviewHeightPadding);
                    CreatePreviewLabel(previewWidth,FullMethodLabel, $"EditorGUIUtility.IconContent(\"{_currentlySelected.tooltip}\")");
                    GUILayout.FlexibleSpace();
                }
            }

            _logic.HandleContentEvents(ref _currentlySelected);
        }
        
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(DownloadPresentedContent),false, _logic.DownloadSelectionOfImages, new ContentInformation(_currentSelection, $"{_information.SubDirectory}/{_selectedContentSize.ToString()}"));
        }
        
        private void InitializeContent()
        {
            _logic = new EditorResourceLogic();
            string[] iconExtensions = new string[2] {PngFileExtension, AssetFileExtension};
            contentNames = _logic.GetAppropriateNames(_logic.GetEditorAssetBundle(), _information.PrefixPath, iconExtensions);
            
            if (contentNames == null || contentNames.Count == 0)
            {
                _logic.DisplayError(NoContentFoundWarning);
                _information.CloseWindow?.Invoke();
            }
            
            SortBySizes();
        }

        private void CreateToolbar(string controlName)
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(ContentSizeFilterLabel, EditorStyles.boldLabel, GUILayout.Width(ContentSizeLabelWidth));
                _selectedContentSize = (ContentSize)GUILayout.SelectionGrid((int)_selectedContentSize, ContentSizes, ContentSizes.Length, EditorStyles.toolbarButton, GUILayout.Width(ContentSizesWidth));
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
            foreach (string iconName in contentNames)
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
                    _smallContent.Add(iconContent);
                }
                else if (icon.width <= MediumToLargeLimit || icon.height <= MediumToLargeLimit)
                {
                    _mediumContent.Add(iconContent);
                }
                else
                {
                    _largeContent.Add(iconContent);
                }
            }
        }

        private GUIContent GetIconContent(string iconName)
        {
            return string.IsNullOrEmpty(iconName) ? null : EditorGUIUtility.IconContent(iconName);
        }

        private List<GUIContent> GetSizeAppropriateContent(ContentSize contentSize)
        {
            switch (contentSize)
            {
                case ContentSize.Small:
                    _buttonSize = SmallButtonSize;
                    return _smallContent;
                case ContentSize.Medium:
                    _buttonSize = MediumButtonSize;
                    return _mediumContent;
                case ContentSize.Large:
                    _buttonSize = LargeButtonSize;
                    return _largeContent;
                default:
                    Debug.LogWarning(InappropriateSizeWarning);
                    _buttonSize = SmallButtonSize;
                    return _smallContent;
            }
        }
        

        #endregion
    }
}
