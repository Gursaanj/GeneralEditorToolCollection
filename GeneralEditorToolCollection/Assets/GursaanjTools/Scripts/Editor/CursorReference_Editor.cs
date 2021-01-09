using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GursaanjTools
{
    public class CursorReference_Editor : GuiControlEditorWindow, IHasCustomMenu
    {
        #region Variables

        //GUI Labels
        private const string CursorNameLabel = "Cursor Name";
        private const string ToImplementLabel = "To Implement";
        private const string OperatingSystemLabel = "Operating System";
        private const string DownloadVisibleCursors = "Download visible cursors";
        private const string ToUseLabel = "To use as cursor, simply donwload this image and use it as the texture2D argument for the Cursor.SetCursor() method";

        private const float ToolbarLabelWidth = 120f;
        private const float OperatingSystemWidth = 75f;
        private const float BorderPadding = 10f;
        private const float LabelPadding = 3f;
        private const float TexturePreviewRatio = 0.4f;
        private const float PreviewMaxHeight = 100f;
        private const float PreviewTextureHeight = 90f;
        private const float PreviewWidthPadding = 30f;
        private const float PreviewLabelPadding = 5f;
        private const float DownloadLabelPadding = 15f;
        private const float DownloadButtonHeight = 18f;
        private const float ClearContentWidth = 20f;
        private const float CursorButtonSize = 40f;
        
        //Warning Labels
        private const string UnableToLoadAssetsError = "Issue with loading Editor Asset Utilities, unable to retrieve information";

        private const string SubDirectory = "Cursors";
        private const string CursorsPath = "Cursors";
        private const string EditorResourceUtility = "UnityEditorInternal.EditorResourcesUtility";
        private const string LinuxCursorsPath = "linux";
        private const string MacCursorsPath = "macos";
        private const string WindowsCursorsPath = "windows";
        
        private readonly List<GUIContent> _windowCursors = new List<GUIContent>();
        private readonly List<GUIContent> _macCursors = new List<GUIContent>();
        private readonly List<GUIContent> _linuxCursors = new List<GUIContent>();
        private readonly List<GUIContent> _otherCursors = new List<GUIContent>();
        
        private EditorResourceLogic _logic;
        private List<string> _cursorNames = new List<string>();
        private OperatingSystemFamily _operatingSystem = OperatingSystemFamily.Windows;
        private List<GUIContent> _currentCursors = new List<GUIContent>();
        private GUIContent _currentlySelectedCursor = GUIContent.none;
        
        private Vector2 _scrollPosition = Vector2.zero;
        private string _searchField = string.Empty;

        #endregion

        #region BuiltIn Methods

        private void OnEnable()
        {
            _logic = new EditorResourceLogic();
            string[] extensions = new[] {_logic.PNGFileExtension};
            _cursorNames = _logic.GetAppropriateNames(_logic.GetEditorAssetBundle(), GetCursorsPath(), extensions);

            if (_cursorNames == null)
            {
                Debug.LogError(UnableToLoadAssetsError);
                Close();
            }

            SortCursorsByOS();

        }

        protected override void CreateGUI(string controlName)
        {
            CreateToolBar(controlName);

            _currentCursors = GetAppropriateOSCursors(_operatingSystem);
            
            if (!string.IsNullOrEmpty(_searchField))
            {
                _currentCursors = _currentCursors.Where(cursor => cursor.tooltip.ToLower().Contains(_searchField.ToLower())).ToList();
            }

            using (var scrollScope = new GUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;
                float pixelsPerPoint = EditorGUIUtility.pixelsPerPoint;
                GUILayout.Space(BorderPadding);

                float renderWidth = Screen.width / pixelsPerPoint - _logic.ScrollBarWidth;
                int gridWidth = Mathf.FloorToInt(renderWidth / CursorButtonSize);
                float marginPadding = (renderWidth - CursorButtonSize * gridWidth) / 2;

                int currentRow = 0;
                int cursorIndex = 0;
                int numberOfCursors = _currentCursors.Count;

                while (cursorIndex < numberOfCursors)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(marginPadding);

                        for (int i = 0; i < gridWidth; i++)
                        {
                            int currentIndex = i + currentRow * gridWidth;
                            GUIContent currentCursor = _currentCursors[currentIndex];

                            if (GUILayout.Button(currentCursor, _logic.IconButtonStyle, GUILayout.Width(CursorButtonSize),
                                GUILayout.Height(CursorButtonSize)))
                            {
                                _currentlySelectedCursor = currentCursor;
                            }

                            cursorIndex++;

                            if (cursorIndex == numberOfCursors)
                            {
                                break;
                            }
                        }
                    }
                    currentRow++;
                }
                
                GUILayout.Space(BorderPadding);
            }
            
            if (_currentlySelectedCursor.Equals(GUIContent.none))
            {
                return;
            }
                
            GUILayout.FlexibleSpace();
            string cursorName = $"\"{_currentlySelectedCursor.tooltip}\"";
            float textureWidth = position.width * TexturePreviewRatio;
            float previewStyleWidth = textureWidth / 2;
            float previewWidth = position.width - textureWidth - PreviewWidthPadding;

            using (new GUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.MaxHeight(PreviewMaxHeight)))
            {
                using (new GUILayout.VerticalScope(GUILayout.Width(textureWidth)))
                {
                    GUILayout.Space(LabelPadding);

                    GUILayout.Button(_currentlySelectedCursor, _logic.IsLightPreview ? _logic.WhitePreviewStyle : _logic.BlackPreviewStyle,
                        GUILayout.Width(textureWidth), GUILayout.Height(PreviewTextureHeight));
                    
                    GUILayout.FlexibleSpace();

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button(_logic.LightThemeLabel, EditorStyles.miniButton,
                            GUILayout.Width(previewStyleWidth)))
                        {
                            _logic.IsLightPreview = true;
                        }
                        
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(_logic.DarkThemeLabel, EditorStyles.miniButton,
                            GUILayout.Width(previewStyleWidth)))
                        {
                            _logic.IsLightPreview = false;
                        }
                    }
                }
                
                GUILayout.Space(BorderPadding);

                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope(GUILayout.Width(previewWidth)))
                    {
                        GUILayout.Space(PreviewLabelPadding);
                        EditorGUILayout.HelpBox($"Width : {_currentlySelectedCursor.image.width} Height : {_currentlySelectedCursor.image.height}", MessageType.None, true);
                        GUILayout.Space(DownloadLabelPadding);
                        if (GUILayout.Button(_logic.DownloadLabel, GUILayout.Height(DownloadButtonHeight)))
                        {
                            if (_logic.DownloadImageContent(_currentlySelectedCursor, $"{SubDirectory}/{_operatingSystem.ToString()}"))
                            {
                                DisplayDialogue(UpdateTitle, string.Format(_logic.DownloadMessageLabel, _currentlySelectedCursor.tooltip), false);
                            }
                        }
                    }
                    
                    GUILayout.Space(PreviewLabelPadding);
                    
                    using (new GUILayout.HorizontalScope(GUILayout.Width(previewWidth)))
                    {
                        GUILayout.Label(CursorNameLabel, EditorStyles.boldLabel);
                        GUILayout.Space(LabelPadding);
                        GUILayout.Label(cursorName, GUILayout.MaxWidth(previewWidth));
                        if (GUILayout.Button(_logic.CopyContent, EditorStyles.miniButtonRight))
                        {
                            EditorGUIUtility.systemCopyBuffer = cursorName;
                        }
                        GUILayout.FlexibleSpace();
                    }
                    
                    GUILayout.Space(LabelPadding);

                    using (new GUILayout.VerticalScope(GUILayout.Width(previewWidth)))
                    {
                        GUILayout.Label(ToImplementLabel, EditorStyles.boldLabel);
                        
                        GUILayout.TextArea(ToUseLabel, _logic.WordWrapStyle);
                    }
                    
                    GUILayout.FlexibleSpace();
                }
            }
            
            _logic.HandleContentEvents(ref _currentlySelectedCursor);
        }

        #endregion

        #region IHasCustomMenu Implementation

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(DownloadVisibleCursors), false, _logic.DownloadSelectionOfImages, new ContentInformation(_currentCursors, $"{SubDirectory}/{_operatingSystem.ToString()}"));
        }

        #endregion

        #region Custom Methods

        private void CreateToolBar(string controlName)
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(OperatingSystemLabel, EditorStyles.boldLabel, GUILayout.Width(ToolbarLabelWidth));
                _operatingSystem = (OperatingSystemFamily)EditorGUILayout.EnumPopup(_operatingSystem, EditorStyles.toolbarDropDown, GUILayout.Width(OperatingSystemWidth));
                GUI.SetNextControlName(controlName);
                _searchField = GUILayout.TextField(_searchField, EditorStyles.toolbarSearchField);
                if (GUILayout.Button(_logic.ClearSearch, EditorStyles.toolbarButton,
                    GUILayout.Width(ClearContentWidth)))
                {
                    _searchField = string.Empty;
                }
            }
        }

        private string GetCursorsPath()
        {
#if UNITY_2018_2_OR_NEWER
            return CursorsPath;
#else
            var assembly = typeof(EditorGUIUtility).Assembly;
            var resourceUtility = assembly.GetType(_logic.EditorResourceUtility);
            var PathProperty = resourceUtility.GetProperty(CursorsPath, BindingFlags.Static | BindingFlags.Public);
            return (string)PathProperty.GetValue(null, new object[] { });
#endif
        }

        private void SortCursorsByOS()
        {
            foreach (string cursorName in _cursorNames)
            {
                GUIContent cursor = _logic.GetImageContent(cursorName);

                if (cursor == null || cursor.Equals(GUIContent.none))
                {
                    continue;
                }

                Texture cursorImage = cursor.image;

                if (cursorImage == null)
                {
                    continue;
                }
                
                cursor.tooltip = cursorImage.name;

                if (cursorName.Contains(LinuxCursorsPath))
                {
                    _linuxCursors.Add(cursor);
                }
                else if (cursorName.Contains(MacCursorsPath))
                {
                    _macCursors.Add(cursor);
                }
                else if (cursorName.Contains(WindowsCursorsPath))
                {
                    _windowCursors.Add(cursor);
                }
                else
                {
                    _otherCursors.Add(cursor);
                }
            }
        }

        private List<GUIContent> GetAppropriateOSCursors(OperatingSystemFamily os)
        {
            switch (os)
            {
                case OperatingSystemFamily.Linux:
                    return _linuxCursors;
                case OperatingSystemFamily.MacOSX:
                    return _macCursors;
                case OperatingSystemFamily.Windows:
                    return _windowCursors;
                case OperatingSystemFamily.Other:
                    return _otherCursors;
                default:
                    return _windowCursors;
            }
        }

        #endregion
    }
}
